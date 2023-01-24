using Boa.Constrictor.Screenplay;
using Boa.Constrictor.Playwright;
using FluentAssertions;
using Microsoft.Playwright;
using Datagrove.Playwright;
using static Boa.Constrictor.Playwright.WebLocator;

namespace pw1.Test;
// this example doesn't work any more, duckduckgo has changed their search pages

public class AsiActor : Actor, IDisposable
{
    pw1.PlaywrightDriver driver;

    public AsiActor(TestContext context, string name)
    : base(logger: new ConsoleLogger())
    {
        var images = "/users/jim/dev/test"; //context!.TestResultsDirectory + "/../../images/";
        var options = new pw1.PlaywrightOptions
        {
            browserType = pw1.BrowserType.Chrome,
            options = new BrowserTypeLaunchOptions
            {
                Headless = true,
                Args = new string[] { "--ignore-certificate-errors" },
                //TracesDir = "/Users/jim/dev/test/traces"
            },
            contextOptions = new BrowserNewContextOptions
            {
                IgnoreHTTPSErrors = true,
                ViewportSize = new ViewportSize
                {
                    Width = 1920,
                    Height = 1080
                },
                //RecordVideoDir = "/Users/jim/dev/test/videos",
            },
            trace = images + "/" + context.TestName + ".zip"
        };
        driver = new pw1.PlaywrightDriver(options);
        Can(BrowseTheWeb.With(driver));
    }
    public void Dispose()
    {
        // driver.GetScreenshot().SaveAsFile("/Users/jim/dev/test/screenshot.png", ScreenshotImageFormat.Png);
        AttemptsTo(QuitWebDriver.ForBrowser());
    }
}

// what are the tradeoffs of using a TestInitialize vs just using?
// we still need a context though.

[TestClass]
public class AsiWebUiTest
{
    public TestContext? TestContext { set; get; }

    [TestMethod]
    public void TestAsi()
    {
        using (var actor = new AsiActor(TestContext, "asoria"))
        {
            actor.AttemptsTo(Navigate.ToUrl(StaffLoginPage.Url));
            actor.AttemptsTo(StaffLogin.For("carlyk", "demo123"));
            actor.WaitsUntil(Appearance.Of(StaffNavigation1Page.communityDashboardloadup), IsEqualTo.True());
        }
    }
    [TestMethod]
    public void TestAsi2()
    {
        using (var actor = new AsiActor(TestContext, "asoria"))
        {
            actor.AttemptsTo(Navigate.ToUrl(StaffLoginPage.Url));
            actor.AttemptsTo(StaffLogin.For("asoria", "demo123"));
            actor.WaitsUntil(Appearance.Of(StaffNavigation1Page.communityDashboardloadup), IsEqualTo.True());
        }
    }
}


public class StaffLogin : ITask
{
    public string user, password;

    private StaffLogin(string User, string Password)
    {
        user = User;
        password = Password;
    }

    public static StaffLogin For(string User, string Password) =>
      new StaffLogin(User, Password);

    public void PerformAs(IActor actor)
    {
        actor.AttemptsTo(SendKeys.To(StaffLoginPage.Username, user));
        actor.AttemptsTo(SendKeys.To(StaffLoginPage.Password, password));
        actor.AttemptsTo(Click.On(StaffLoginPage.Submit));

    }
}
public class StaffLoginPage
{
    public const string Url = "https://windev2212eval";

    public static IWebLocator Username => L(
        "Username",
        By.Placeholder("Username"));

    public static IWebLocator Password => L(
        "Password",
        By.Placeholder("Password"));

    public static IWebLocator Submit => L(
        "Sign in",
        By.Id("ctl01_TemplateBody_WebPartManager1_gwpciSignIn_ciSignIn_SubmitButton"));

    public static IWebLocator ErrorMessage => L(
      "error message",
      By.ClassName("iPartRenderError"));
}

public class StaffNavigation1Page
//for members
{
    public static IWebLocator findContacts => L(
      " Find Contacts",
      By.Id("ctl01_TemplateBody_WebPartManager1_gwpciContactsbytype_ciContactsbytype_RadChart1")
    );

    public const string findContactsUrl = "Staff/Community/Find_contacts/iCore/Contacts/Directory.aspx?hkey=3d853e42-16b2-4cf9-89e2-652bea25f4f6";

    public static IWebLocator communityDashboardloadup => L(
        " Load Circle Graph Contacts by type",
        By.Id("ctl01_TemplateBody_WebPartManager1_gwpciContactsbytype_ciContactsbytype_RadChart1")
    );

    public const string findContactspage = "Staff/Community/Find_contacts/iCore/Contacts/Directory.aspx";

    public static IWebLocator contactsSelectAQuery => L(
        " all criteria queary",
        By.Id("ctl01_TemplateBody_WebPartManager1_gwpciDirectory_ciDirectory_PeopleSearch_querySelectDropdown")
    );

    public static IWebLocator multiSelectQuery => L(
        "select a queary multi fields",
        By.Id("ctl01_TemplateBody_WebPartManager1_gwpciDirectory_ciDirectory_PeopleSearch_querySelectDropdown")
    );

    public static IWebLocator firstName => L(
        " input name",
        By.Id("ctl01_TemplateBody_WebPartManager1_gwpciDirectory_ciDirectory_PeopleSearch_ResultsGrid_Sheet0_Input0_TextBox1")
    );

    public static IWebLocator lastName => L(
        " Last name",
        By.Id("ctl01_TemplateBody_WebPartManager1_gwpciDirectory_ciDirectory_PeopleSearch_ResultsGrid_Sheet0_Input1_TextBox1")
    );


    public static IWebLocator submit => L(
        "Find",
        By.Id("ctl01_TemplateBody_WebPartManager1_gwpciDirectory_ciDirectory_PeopleSearch_ResultsGrid_Sheet0_SubmitButton")
    );

    // By id is shorthand for:
    //By.CssSelector("#search_button_homepage")
    // Good practice for your DOM queries.

    public static IWebLocator recentsearchAlexMorga => L(
        "Alex Morgan ",
        By.Id("tl01_TemplateBody_WebPartManager1_gwpciRecentcontacts_ciRecentcontacts_LinkRepeater_ctl00_Link")
    );

    public const string findaMorganUrl2 = "iCore/Contacts/ContactLayouts/Account_Page_Staff.aspx?WebsiteKey=4243d9e2-e91e-468c-97c2-2046d70c1e1a";

}