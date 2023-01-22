namespace pw1;

using System.Collections.ObjectModel;
using System.Drawing;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

using ProxyFn = Func<Proxy, Task<object>>;
public class Proxy : IAsyncDisposable
{
    public IPlaywright playwright;
    public IBrowser browser;
    public IPage page;
    public object? rvalue = null;
    public Proxy(IPlaywright p, IBrowser b, IPage page)
    {
        this.playwright = p;
        this.browser = b;
        this.page = page;
    }

    public static async Task<Proxy> create()
    {
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();
        var proxy = new Proxy(playwright, browser, page);
        return proxy;
    }

    public async ValueTask DisposeAsync()
    {
        await browser.DisposeAsync();
        playwright.Dispose();
    }
}
public class PlaywrightDriver : IWebDriver, INavigation, IDisposable
{
    Semaphore rpc = new Semaphore(0, 1);
    Semaphore reply = new Semaphore(0, 1);
    Func<Proxy, Task<object>>? fn;
    bool quit = false;
    Exception? e;
    Proxy? proxy;

    public void Dispose()
    {
        if (!quit)
        {
            stop();
        }
    }

    public void stop()
    {
        quit = true;
        rpc.Release();
        reply.WaitOne();
    }

    public static async Task ThreadProc(PlaywrightDriver p)
    {
        p.proxy = await Proxy.create();
        await p.proxy.page.GotoAsync("https://playwright.dev");
        string t = await p.proxy.page.TitleAsync();
        while (true)
        {
            p.reply.Release();
            if (p.quit) break;
            p.rpc.WaitOne();
            try
            {
                p.proxy.rvalue = await p.fn!(p.proxy);
            }
            catch (Exception o)
            {
                p.e = o;
            }
        }
        await p.proxy.DisposeAsync();
    }
    public T exec<T>(Func<Proxy, Task<object>> fn)
    {
        e = null;
        this.fn = fn;
        rpc.Release();
        reply.WaitOne();
        if (e != null)
        {
            throw e;
        }
        return (T)proxy!.rvalue!;
    }

    public PlaywrightDriver()
    {
        Task.Run(async () => await ThreadProc(this));
        reply.WaitOne();
    }

    public string Url
    {
        get
        {
            return "";
        }
        set
        {

        }
    }

    public string Title
    {
        get
        {
            return exec<string>(async Task<object> (Proxy p) =>
                 await p.page.TitleAsync())!;
        }
    }

    public void Back()
    {
        exec<bool>(async Task<object> (Proxy p) =>
        {
            await p.page.GoBackAsync();
            return true;
        });
    }
    public void Forward()
    {
        exec<bool>(async Task<object> (Proxy p) =>
        {
            await p.page.GoForwardAsync();
            return true;
        });

    }
    public void GoToUrl(string url)
    {
        exec<bool>(async Task<object> (Proxy p) =>
        {
            await p.page.GotoAsync(url);
            return true;
        });

    }
    public void GoToUrl(Uri url)
    {
        GoToUrl(url.ToString());
    }
    public void Refresh()
    {
        exec<bool>(async Task<object> (Proxy p) =>
        {
            await p.page.ReloadAsync();
            return true;
        });
    }

    public string PageSource
    {
        get
        {
            return "";
        }
    }

    public string CurrentWindowHandle
    {
        get
        {
            return "";
        }
    }

    public ReadOnlyCollection<string> WindowHandles
    {
        get
        {
            return new ReadOnlyCollection<string>(new List<string>());
        }
    }

    public void Close()
    {
        exec<bool>(async Task<object> (Proxy p) =>
{
    await p.page.CloseAsync();
    return true;
});
    }

    public static string locatorString(By by)
    {

        return "";
    }
    public IWebElement FindElement(By by)
    {
        return FindElement(by, null);
    }
    // if this doesn't find, it throws.
    public IWebElement FindElement(By by, IElementHandle? root)
    {
        return exec<PWebElement>(async Task<object> (Proxy p) =>
         {
             var h = await p.page.WaitForSelectorAsync(locatorString(by), new()
             {
                 Timeout = 30000,
                 State = WaitForSelectorState.Visible,
                 Strict = false
             });
             return new PWebElement(this, h!);
         });
    }
    public ReadOnlyCollection<IWebElement> FindElements(By by)
    {
        return FindElements(by, null);
    }
    public ReadOnlyCollection<IWebElement> FindElements(By by, IElementHandle? root)
    {
        return exec<ReadOnlyCollection<IWebElement>>(async Task<object> (Proxy p) =>
        {
            var a = await p.page.QuerySelectorAllAsync("");
            var lst = a.Select(e => (IWebElement)new PWebElement(this, e)).ToList();
            return new ReadOnlyCollection<IWebElement>(lst);
        })!;
    }

    public INavigation Navigate()
    {
        return this;
    }

    public void Quit()
    {
        stop();
    }

    public ITargetLocator SwitchTo()
    {
        return new PlaywrightTargetLocator();
    }
}

public class PWebElement : IWebElement
{
    // we might need some frame sudo reference for context?
    PlaywrightDriver driver;
    IElementHandle h;
    public string tagName = "", text = "";
    public bool enabled = false, selected = false, displayed = false;
    public Point location = new Point(0, 0);
    public Size size = new Size(0, 0);


    public PWebElement(PlaywrightDriver driver, IElementHandle h)
    {
        this.driver = driver;
        this.h = h;
    }
    public string TagName => tagName;

    public string Text => text;

    public bool Enabled => enabled;

    public bool Selected => selected;

    public Point Location => location;

    public Size Size => size;

    public bool Displayed => displayed;

    public void Clear()
    {
        SendKeys("");
    }
     public void SendKeys(string text)
    {
        driver.exec<bool>(async Task<object> (Proxy p) =>
        {
            await h.FillAsync(text);
            return true;
        });
    }


    public void Click()
    {
        driver.exec<bool>(async Task<object> (Proxy p) =>
        {
            await h.ClickAsync();
            return true;
        });
    }
    public void Submit()
    {
        // clasms that we can submit on any element in the form
        driver.exec<bool>(async Task<object> (Proxy p) =>
        {
            await h.ClickAsync();
            return true;
        });
    
    }

    public IWebElement FindElement(By by)
    {
        return driver.FindElement(by, h);
    }

    public ReadOnlyCollection<IWebElement> FindElements(By by)
    {
        return driver.FindElements(by, h);
    }

    public string GetAttribute(string attributeName)
    {
        return driver.exec<string>(async Task<object> (Proxy p) =>
        {
            return await h.GetAttributeAsync(attributeName)??"";
        });
    }

    public string GetCssValue(string propertyName)
    {
        return driver.exec<string>(async Task<object> (Proxy p) =>
        {
            return await h.GetPropertyAsync(propertyName)??"";
        });
    }

    public string GetDomAttribute(string attributeName)
    {
        throw new NotImplementedException();
    }

    public string GetDomProperty(string propertyName)
    {
        throw new NotImplementedException();
    }

    public ISearchContext GetShadowRoot()
    {
        throw new NotImplementedException();
    }



}

public class PwAlert : IAlert
{
    public string Text => throw new NotImplementedException();

    public void Accept()
    {
        throw new NotImplementedException();
    }

    public void Dismiss()
    {
        throw new NotImplementedException();
    }

    public void SendKeys(string keysToSend)
    {
        throw new NotImplementedException();
    }
}
public class PlaywrightTargetLocator : ITargetLocator
{

    public IWebElement ActiveElement()
    {
        throw new NotImplementedException();
    }

    public IAlert Alert()
    {
        return new PwAlert();
    }

    public IWebDriver DefaultContent()
    {
        throw new NotImplementedException();
    }

    public IWebDriver Frame(int frameIndex)
    {
        throw new NotImplementedException();
    }

    public IWebDriver Frame(string frameName)
    {
        throw new NotImplementedException();
    }

    public IWebDriver Frame(IWebElement frameElement)
    {
        throw new NotImplementedException();
    }

    public IWebDriver NewWindow(WindowType typeHint)
    {
        throw new NotImplementedException();
    }

    public IWebDriver ParentFrame()
    {
        throw new NotImplementedException();
    }

    public IWebDriver Window(string windowName)
    {
        throw new NotImplementedException();
    }
}



public interface IFindsElement
{
    IWebElement FindElement(string mechanism, string value);
    ReadOnlyCollection<IWebElement> FindElements(string mechanism, string value);
}

public class By
{
    public string description;

    public By(string description)
    {
        this.description = description;
    }
    public override int GetHashCode()
    {
        return this.description.GetHashCode();
    }

    public string Criteria
    {
        get { return this.description; }
    }

    public static bool operator ==(By one, By two)
    {
        return one.description == two.description;
    }

    public static bool operator !=(By one, By two)
    {
        return !(one == two);
    }
    public override bool Equals(object? obj)
    {
        return description.Equals(obj);
    }


    public static By Id(string idToFind)
    {
        return new By($"id={idToFind}");
    }


    public static By LinkText(string linkTextToFind)
    {
        return new By($"text={linkTextToFind}");
    }


    public static By Name(string nameToFind)
    {
        return new By($"name={nameToFind}");
    }

    public static By XPath(string xpathToFind)
    {
        return new By($"xpath=[{xpathToFind}]");
    }


    public static By ClassName(string classNameToFind)
    {
        return new By($".{classNameToFind}");
    }


    public static By PartialLinkText(string partialLinkTextToFind)
    {
        return new By($"text={partialLinkTextToFind}");
    }

    public static By TagName(string tagNameToFind)
    {
        return new By(tagNameToFind);
    }


    public static By CssSelector(string cssSelectorToFind)
    {
        return new By($"css=[{cssSelectorToFind}");
    }


    public virtual IWebElement FindElement(ISearchContext context)
    {
        return context.FindElement(this);
    }


    public virtual ReadOnlyCollection<IWebElement> FindElements(ISearchContext context)
    {
        return context.FindElements(this);
    }


    public override string ToString()
    {
        return this.description;
    }




}

