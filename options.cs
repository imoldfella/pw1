using Microsoft.Playwright;
namespace pw1;

public enum BrowserType
{
    Chrome,
    Edge,
    Safari,
    Firefox
}
public class PlaywrightOptions 
{
    List<string> args = new();
    public void AddArgument(string opts)
    {
        args.Add(opts);
    }
    BrowserType browserType;
    BrowserTypeLaunchOptions options;

    public PlaywrightOptions(BrowserType browserType,BrowserTypeLaunchOptions? options = null)
    {
        this.options = options??new BrowserTypeLaunchOptions();
        this.browserType = browserType;
    }


    public async Task<IBrowser> launchAsync(IPlaywright playwright)
    {
        options.Args = args.ToArray();

        switch (browserType)
        {
            case BrowserType.Chrome:
                return await playwright.Chromium.LaunchAsync(options);
            case BrowserType.Edge:
                // channel msedge
                return await playwright.Chromium.LaunchAsync(options);
            case BrowserType.Safari:
                return await playwright.Webkit.LaunchAsync(options);
            case BrowserType.Firefox:
                return await playwright.Firefox.LaunchAsync(options);

        }
        throw new NotImplementedException();
    }
}




