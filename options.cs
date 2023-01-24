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

}





