namespace pw1;

using System.Collections.ObjectModel;
using Microsoft.Playwright;

using ProxyFn = Func<Proxy, Task<object>>;
public class Proxy : IAsyncDisposable
{
    public IPlaywright playwright;
    public IBrowser browser;
    public IPage page;
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
    object? rvalue = null;
    Func<Proxy, Task<object>>? fn;
    bool quit = false;
    Exception? e;

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
        var proxy = await Proxy.create();
        await proxy.page.GotoAsync("https://playwright.dev");
        string t = await proxy.page.TitleAsync();
        while (true)
        {
            p.reply.Release();
            if (p.quit) break;
            p.rpc.WaitOne();
            try
            {
                p.rvalue = await p.fn!(proxy);
            }
            catch (Exception o)
            {
                p.e = o;
            }
        }
        await proxy.DisposeAsync();
    }
    object? exec(Func<Proxy, Task<object>> fn)
    {
        e = null;
        this.fn = fn;
        rpc.Release();
        reply.WaitOne();
        if (e != null)
        {
            throw e;
        }
        return rvalue;
    }

    public PlaywrightDriver()
    {
        Task.Run(async () => await ThreadProc(this));
        reply.WaitOne();
    }

    public string Url { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public string Title
    {
        get
        {
            ProxyFn fn = async Task<object> (Proxy p) => await p.page.TitleAsync();
            return (string)exec(fn)!;
        }
    }

    public void Back()
    {
        exec(async Task<object> (Proxy p) => { await p.page.GoBackAsync(); return null!; });
    }
    public void Forward()
    {
        exec(async Task<object> (Proxy p) => { await p.page.GoForwardAsync(); return null!; });
    }
    public void GoToUrl(string url)
    {
        ProxyFn fn = async Task<object> (Proxy p) => { await p.page.GotoAsync(url); return null!; };
        exec(fn);
    }
    public void GoToUrl(Uri url)
    {
        GoToUrl(url.ToString());
    }
    public void Refresh()
    {
        exec(async Task<object> (Proxy p) => { await p.page.ReloadAsync(); return null!; });
    }

    public string PageSource => throw new NotImplementedException();

    public string CurrentWindowHandle => throw new NotImplementedException();

    public ReadOnlyCollection<string> WindowHandles => throw new NotImplementedException();

    public void Close()
    {
        exec(async Task<object> (Proxy p) => { await p.page.CloseAsync(); return null!; });
    }

    public IWebElement FindElement(By by)
    {
        throw new NotImplementedException();
    }

    public ReadOnlyCollection<IWebElement> FindElements(By by)
    {
        throw new NotImplementedException();
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

public class PlaywrightTargetLocator : ITargetLocator
{
    public IWebElement ActiveElement()
    {
        throw new NotImplementedException();
    }

    public IAlert Alert()
    {
        throw new NotImplementedException();
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