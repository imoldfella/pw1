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
            return exec<string>(async Task<object> (Proxy p) =>
            {
                await Task.CompletedTask;
                return p.page.Url;
            });
        }
        set
        {
            GoToUrl(Url);
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
            return exec<string>(async Task<object> (Proxy p) =>
            {
                return await p.page.ContentAsync();
            });
        }
    }

    // // window handles are unique string ids assigned the window.
    // https://www.selenium.dev/documentation/webdriver/interactions/windows/
    // Clicking a link which opens in a new window will focus the new window or tab on screen, but WebDriver will not know which window the Operating System considers active. To work with the new window you will need to switch to it. If you have only two tabs or windows open, and you know which window you start with, by the process of elimination you can loop over both windows or tabs that WebDriver can see, and switch to the one which is not the original.


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
            // https://playwright.dev/dotnet/docs/pages#multiple-pages
            // Get all new pages (including popups) in the context
            // context.Page += async  (_, page) => {
            //     await page.WaitForLoadStateAsync();
            //     Console.WriteLine(await page.TitleAsync());
            // };

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


    public IWebElement FindElement(By by)
    {
        return FindElement(by, null);
    }
    // if this doesn't find, it throws.
    public IWebElement FindElement(By by, IElementHandle? root)
    {
        return exec<PWebElement>(async Task<object> (Proxy p) =>
         {
             var h = await p.page.WaitForSelectorAsync(by.description, new()
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
        return new PlaywrightTargetLocator(this);
    }
}

public class PWebElement : IWebElement
{
    // we might need some frame sudo reference for context?
    PlaywrightDriver driver;
    IElementHandle h;

    public PWebElement(PlaywrightDriver driver, IElementHandle h)
    {
        this.driver = driver;
        this.h = h;
    }

    public string TagName {
        get
        {
            return driver.exec<string>(async Task<object> (Proxy p) =>
            {
                return await h.GetPropertyAsync("tagName");
            });
        }
    }
    public string Text  {
        get
        {
            return driver.exec<string>(async Task<object> (Proxy p) =>
            {
                return await h.TextContentAsync()??"";
            });
        }
    }
    public bool Enabled {
        get
        {
            return driver.exec<bool>(async Task<object> (Proxy p) =>
            {
                return await h.IsEnabledAsync();
            });
        }
    }
    public bool Selected {
        get
        {
            return driver.exec<bool>(async Task<object> (Proxy p) =>
            {
                await Task.CompletedTask;
                return true;
            });
        }
    }

    public Point Location{
        get
        {
            return driver.exec<Point>(async Task<object> (Proxy p) =>
            {
                var o =  await h.BoundingBoxAsync();
                return new Point((int)o!.X, (int)o.Y);
            });
        }
    }

    public Size Size {
        get
        {
            return driver.exec<Size>(async Task<object> (Proxy p) =>
            {
                var o = await h.BoundingBoxAsync();
                return new Size((int)o!.Width, (int)o.Height);
            });
        }
    }
    public bool Displayed {
        get
        {
            return driver.exec<bool>(async Task<object> (Proxy p) =>
            {
                return await h.IsVisibleAsync();
            });
        }
    }

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



    // java: System.out.println(page.locator("body").evaluate("element => getComputedStyle(element)['background-color']"));
    public string GetCssValue(string propertyName)
    {
        // window.getComputedStyle(e).getPropertyValue("color")
        return driver.exec<string>(async Task<object> (Proxy p) =>
        {
            return await p.page.Locator("div").EvaluateAsync($"e => window.getComputedStyle(e).{propertyName}");
        });
    }

    // deprecated, 
    public string GetAttribute(string attributeName)
    {
        return driver.exec<string>(async Task<object> (Proxy p) =>
        {
            return await h.GetAttributeAsync(attributeName) ?? "";
        });
    }
    public string GetDomAttribute(string attributeName)
    {
        return driver.exec<string>(async Task<object> (Proxy p) =>
      {
          return await h.GetAttributeAsync(attributeName) ?? "";
      });
    }

    public string GetDomProperty(string propertyName)
    {
        return driver.exec<string>(async Task<object> (Proxy p) =>
         {
             var o = await h.GetPropertyAsync(propertyName);
             var s = o.ToString();
             return s!;
         });
    }

    public ISearchContext GetShadowRoot()
    {
        return this;
    }
}

// these are auto dismissed by playwright, but can be handled by event.
// page.Dialog += async (_, dialog) =>
// {
//     System.Console.WriteLine(dialog.Message);
//     await dialog.DismissAsync();
// };
public class PwAlert : IAlert
{
    PlaywrightDriver driver;
    public PwAlert(PlaywrightDriver driver)
    {
        this.driver = driver;
    }
    public string Text
    {
        get
        {
            return "";
        }
    }

    public void Accept()
    {

    }

    public void Dismiss()
    {

    }

    public void SendKeys(string keysToSend)
    {

    }
}

// playwright has a very different model of windows.

// // Get page after a specific action (e.g. clicking a link)
// var newPage = await context.RunAndWaitForPageAsync(async () =>
// {
//     await page.GetByText("open new tab").ClickAsync();
// });

public class PlaywrightTargetLocator : ITargetLocator
{
    PlaywrightDriver driver;
    public PlaywrightTargetLocator(PlaywrightDriver driver)
    {
        this.driver = driver;
    }

    public IWebElement ActiveElement()
    {
        throw new NotImplementedException();
    }

    public IAlert Alert()
    {
        return new PwAlert(driver);
    }

    public IWebDriver DefaultContent()
    {
        return driver;
    }

    public IWebDriver Frame(int frameIndex)
    {
        return driver;
    }

    public IWebDriver Frame(string frameName)
    {
        return driver;
    }

    public IWebDriver Frame(IWebElement frameElement)
    {
        return driver;
    }

    public IWebDriver NewWindow(WindowType typeHint)
    {
        return driver;
    }

    public IWebDriver ParentFrame()
    {
        return driver;
    }

    public IWebDriver Window(string windowName)
    {
        return driver;
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

