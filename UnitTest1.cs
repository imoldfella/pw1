namespace pw1;

using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

public class MyContext : IDisposable
{


    // compatible with WebDriver - synchronous
    public PlaywrightDriver driver;
    public MyContext(PlaywrightDriver driver)
    {
        this.driver = driver;
    }
    public static async Task<MyContext> create(TestContext t)
    {
        await Task.CompletedTask;
        return new MyContext(new PlaywrightDriver());
    }

    public void Dispose()
    {
    }
}

[TestClass]
public class UnitTest2
{
    public TestContext? TestContext { get; set; }

    // build a class that returns the residual steps
    public class Background
    {
        // some step that may be reused.
        public MyContext step;
        public Background(MyContext context)
        {
            this.step = context;
        }
    }
    static public async Task<Background> background(MyContext x)
    {
        await Task.CompletedTask;
        //step = x;

        return new Background(x);
        // exec steps, but leave the return the initalized steps.
    }
    [TestMethod]
    public void Test1()
    {
        using (var driver = new PlaywrightDriver())
        {
            driver.Navigate().GoToUrl("https://playwright.dev");
            var x = driver.Title;
            StringAssert.Matches(x, new Regex("Playwright"));
        }
    }

    [TestClass]
    public class UnitTest1 : PageTest
    {
        [TestMethod]
        public async Task HomepageHasPlaywrightInTitleAndGetStartedLinkLinkingtoTheIntroPage()
        {
            await Page.GotoAsync("https://playwright.dev");

            // Expect a title "to contain" a substring.
            await Expect(Page).ToHaveTitleAsync(new Regex("Playwright"));

            // create a locator
            var getStarted = Page.GetByRole(AriaRole.Link, new() { Name = "Get started" });

            // Expect an attribute "to be strictly equal" to the value.
            await Expect(getStarted).ToHaveAttributeAsync("href", "/docs/intro");

            // Click the get started link.
            await getStarted.ClickAsync();

            // Expects the URL to contain intro.
            await Expect(Page).ToHaveURLAsync(new Regex(".*intro"));
        }
    }
}