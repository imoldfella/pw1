using Boa.Constrictor.Screenplay;
using Boa.Constrictor.Selenium;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Chrome;
using pw1;

namespace Boa.Constrictor.Example
{
    [TestClass]
    public class ScreenplayWebUiTest
    {
        private IActor Actor;

        [TestInitialize]
        public void InitializeScreenplay()
        {
            var options = new ChromeOptions();
            options.AddArgument("headless");   // Remove this line to "see" the browser run
            var driver = new pw1.PlaywrightDriver(options);
            Actor = new Actor(name: "Andy", logger: new ConsoleLogger());
            Actor.Can(BrowseTheWeb.With(driver));
        }

        [TestCleanup]
        public void QuitBrowser()
        {
            Actor.AttemptsTo(QuitWebDriver.ForBrowser());
        }

        [TestMethod]
        public void TestDuckDuckGoWebSearch()
        {
            Actor.AttemptsTo(Navigate.ToUrl(SearchPage.Url));
            Actor.AskingFor(ValueAttribute.Of(SearchPage.SearchInput)).Should().BeEmpty();
            Actor.AttemptsTo(SearchDuckDuckGo.For("panda"));
            Actor.WaitsUntil(Appearance.Of(ResultPage.ResultLinks), IsEqualTo.True());
        }
    }
}
