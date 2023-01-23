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
    public BrowserType browserType;
    public BrowserTypeLaunchOptions options;
    public BrowserNewContextOptions contextOptions;

    public PlaywrightOptions(BrowserType browserType, BrowserTypeLaunchOptions? options = null, BrowserNewContextOptions? contextOptions = null)
    {
        this.options = options ?? new BrowserTypeLaunchOptions();
        this.contextOptions = contextOptions ?? new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = 1920,
                Height = 1080
            },
            RecordVideoDir = "videos"
        };
        this.browserType = browserType;
    }


    public async Task<IBrowser> launchAsync(IPlaywright playwright)
    {
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




