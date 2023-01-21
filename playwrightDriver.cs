namespace pw1;

using System.Collections.ObjectModel;
using Microsoft.Playwright;

class PlaywrightDriver : IWebDriver
{
    public string Url { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public string Title => throw new NotImplementedException();

    public string PageSource => throw new NotImplementedException();

    public string CurrentWindowHandle => throw new NotImplementedException();

    public ReadOnlyCollection<string> WindowHandles => throw new NotImplementedException();

    public void Close()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    public void Quit()
    {
        throw new NotImplementedException();
    }

    public ITargetLocator SwitchTo()
    {
        throw new NotImplementedException();
    }
}