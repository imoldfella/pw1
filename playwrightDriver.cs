namespace pw1;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

public class PwProxy : IAsyncDisposable
{
    public IPlaywright playwright;
    public IBrowser browser;
    public IPage page;
    public object? rvalue = null;
    public IElementHandle? current=null;
    public IBrowserContext context;
    public PwProxy(IPlaywright p, IBrowser b, IPage page, IBrowserContext context)
    {
        this.playwright = p;
        this.browser = b;
        this.page = page;
        this.context = context;
    }

    public async ValueTask DisposeAsync()
    {

        await page.CloseAsync();
        await context.CloseAsync();
        await browser.DisposeAsync();
        playwright.Dispose();
    }
}
public class PlaywrightDriver : IWebDriver, INavigation, IDisposable,
ISearchContext, IJavaScriptExecutor, IFindsElement, ITakesScreenshot
//,ISupportsPrint, IAllowsFileDetection, IHasCapabilities, IHasCommandExecutor, IHasSessionId, ICustomDriverCommandExecutor, IHasVirtualAuthenticator

{
    PlaywrightOptions options;
    Semaphore rpc = new Semaphore(0, 1);
    Semaphore reply = new Semaphore(0, 1);
    List<Func<PwProxy, Task<object>>> fn = new List<Func<PwProxy, Task<object>>>();
    bool quit = false;
    Exception? e;
    PwProxy? proxy;

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
        var playwright = await Playwright.CreateAsync();
        await playwright.Firefox.LaunchAsync();
        IBrowser? browser = null;

        var opt = p.options.options;
        switch (p.options.browserType)
        {
            case BrowserType.Chrome:
                browser = await playwright.Chromium.LaunchAsync(opt);
                break;
            case BrowserType.Edge:
                // channel msedge
                browser = await playwright.Chromium.LaunchAsync(opt);
                break;
            case BrowserType.Safari:
                browser = await playwright.Webkit.LaunchAsync(opt);
                break;
            case BrowserType.Firefox:
                browser = await playwright.Firefox.LaunchAsync(opt);
                break;
        }
        if (browser == null)
        {
            throw new Exception("Browser not found");
        }

        var context = await browser.NewContextAsync(p.options.contextOptions);
        var page = await context.NewPageAsync();
        p.proxy = new PwProxy(playwright, browser, page,context);
        await context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,
            Snapshots = true,
            //Sources = true,
            //Name = "trace",
            
        });
        
        while (!p.quit)
        {
            p.reply.Release();
            p.rpc.WaitOne();
            try
            {
                foreach (var e in p.fn)
                {
                    p.proxy.rvalue = await e(p.proxy);
                }
            }
            catch (Exception o)
            {
                p.e = o;
            }
        }
        await context.Tracing.StopAsync(new(){
            Path = p.options.trace
        });
        await p.proxy.DisposeAsync();
        p.reply.Release();
    }
    public T exec<T>(Func<PwProxy, Task<object>> fn)
    {
        e = null;
        this.fn.Clear();
        this.fn.Add(fn);
        rpc.Release();
        reply.WaitOne();
        if (e != null)
        {
            throw e;
        }
        return (T)proxy!.rvalue!;
    }
    public void Perform(List<Func<PwProxy, Task<object>>> fn)
    {
        e = null;
        this.fn = fn;
        rpc.Release();
        reply.WaitOne();
        if (e != null)
        {
            throw e;
        }
    }

    public PlaywrightDriver(PlaywrightOptions? options = null)
    {
        this.options = options ?? new PlaywrightOptions();
        Task.Run(async () => await ThreadProc(this));
        reply.WaitOne();
    }

    public string Url
    {
        get
        {
            return exec<string>(async Task<object> (PwProxy p) =>
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
            return exec<string>(async Task<object> (PwProxy p) =>
                 await p.page.TitleAsync())!;
        }
    }

    public void Back()
    {
        exec<bool>(async Task<object> (PwProxy p) =>
        {
            await p.page.GoBackAsync();
            return true;
        });
    }
    public void Forward()
    {
        exec<bool>(async Task<object> (PwProxy p) =>
        {
            await p.page.GoForwardAsync();
            return true;
        });

    }
    public void GoToUrl(string url)
    {
        exec<bool>(async Task<object> (PwProxy p) =>
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
        exec<bool>(async Task<object> (PwProxy p) =>
        {
            await p.page.ReloadAsync();
            return true;
        });
    }

    public string PageSource
    {
        get
        {
            return exec<string>(async Task<object> (PwProxy p) =>
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
        exec<bool>(async Task<object> (PwProxy p) =>
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
        return exec<PWebElement>(async Task<object> (PwProxy p) =>
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
        return exec<ReadOnlyCollection<IWebElement>>(async Task<object> (PwProxy p) =>
        {
            var a = await p.page.QuerySelectorAllAsync(by.description);
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


    public Screenshot GetScreenshot()
    {
        return exec<Screenshot>(async Task<object> (PwProxy p) =>
        {
            var s = await p.page.ScreenshotAsync(new()
            {
                Type = ScreenshotType.Png,
                FullPage = true
            });
            return new Screenshot(s);
        });
    }

    public IWebElement FindElement(string mechanism, string value)
    {
        throw new NotImplementedException();
    }

    public ReadOnlyCollection<IWebElement> FindElements(string mechanism, string value)
    {
        throw new NotImplementedException();
    }

    public object ExecuteScript(string script, params object[] args)
    {
        throw new NotImplementedException();
    }

    public object ExecuteScript(PinnedScript script, params object[] args)
    {
        throw new NotImplementedException();
    }

    public object ExecuteAsyncScript(string script, params object[] args)
    {
        throw new NotImplementedException();
    }
}

public class PWebElement : IWebElement, IFindsElement, IWrapsDriver, ILocatable, ITakesScreenshot //, IWebDriverObjectReference
{
    // we might need some frame sudo reference for context?
    public PlaywrightDriver driver;
    public IElementHandle h;

    public PWebElement(PlaywrightDriver driver, IElementHandle h)
    {
        this.driver = driver;
        this.h = h;
    }

    public string TagName
    {
        get
        {
            return driver.exec<string>(async Task<object> (PwProxy p) =>
            {
                return await h.GetPropertyAsync("tagName");
            });
        }
    }
    public string Text
    {
        get
        {
            return driver.exec<string>(async Task<object> (PwProxy p) =>
            {
                return await h.TextContentAsync() ?? "";
            });
        }
    }
    public bool Enabled
    {
        get
        {
            return driver.exec<bool>(async Task<object> (PwProxy p) =>
            {
                return await h.IsEnabledAsync();
            });
        }
    }
    public bool Selected
    {
        get
        {
            return driver.exec<bool>(async Task<object> (PwProxy p) =>
            {
                await Task.CompletedTask;
                return true;
            });
        }
    }

    public Point Location
    {
        get
        {
            return driver.exec<Point>(async Task<object> (PwProxy p) =>
            {
                var o = await h.BoundingBoxAsync();
                return new Point((int)o!.X, (int)o.Y);
            });
        }
    }

    public Size Size
    {
        get
        {
            return driver.exec<Size>(async Task<object> (PwProxy p) =>
            {
                var o = await h.BoundingBoxAsync();
                return new Size((int)o!.Width, (int)o.Height);
            });
        }
    }
    public bool Displayed
    {
        get
        {
            return driver.exec<bool>(async Task<object> (PwProxy p) =>
            {
                return await h.IsVisibleAsync();
            });
        }
    }

    public IWebDriver WrappedDriver => throw new NotImplementedException();

    public Point LocationOnScreenOnceScrolledIntoView => throw new NotImplementedException();

    public ICoordinates Coordinates => throw new NotImplementedException();

    public string ObjectReferenceId => throw new NotImplementedException();

    public void Clear()
    {
        SendKeys("");
    }
    public void SendKeys(string text)
    {
        driver.exec<bool>(async Task<object> (PwProxy p) =>
        {
            await h.FillAsync(text);
            return true;
        });
    }


    public void Click()
    {
        driver.exec<bool>(async Task<object> (PwProxy p) =>
        {
            await h.ClickAsync();
            return true;
        });
    }
    public void Submit()
    {
        // clasms that we can submit on any element in the form
        driver.exec<bool>(async Task<object> (PwProxy p) =>
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
        return driver.exec<string>(async Task<object> (PwProxy p) =>
        {
            return await p.page.Locator("div").EvaluateAsync($"e => window.getComputedStyle(e).{propertyName}");
        });
    }

    public string GetAttribute(string attributeName)
    {
        // getAttribute is pre-w3c way, use DomAttribute instead
        return driver.exec<string>(async Task<object> (PwProxy p) =>
        {
            return (await h.GetPropertyAsync(attributeName))?.ToString()??"";
        });
    }
    public string GetDomAttribute(string attributeName)
    {
        return driver.exec<string>(async Task<object> (PwProxy p) =>
      {
          return await h.GetAttributeAsync(attributeName) ?? "";
      });
    }

    public string GetDomProperty(string propertyName)
    {
        return driver.exec<string>(async Task<object> (PwProxy p) =>
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

    public IWebElement FindElement(string mechanism, string value)
    {
        throw new NotImplementedException();
    }

    public ReadOnlyCollection<IWebElement> FindElements(string mechanism, string value)
    {
        throw new NotImplementedException();
    }

    public Screenshot GetScreenshot()
    {
        throw new NotImplementedException();
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


