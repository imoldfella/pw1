namespace pw1;

using System.Collections.ObjectModel;
using System.Drawing;
public interface IWebElement : ISearchContext
{
    string TagName { get; }
    string Text { get; }
    bool Enabled { get; }
    bool Selected { get; }
    Point Location { get; }
    Size Size { get; }
    bool Displayed { get; }

    void Clear();
    void Click();
    string GetAttribute(string attributeName);
    string GetCssValue(string propertyName);
    string GetDomAttribute(string attributeName);
    string GetDomProperty(string propertyName);
    ISearchContext GetShadowRoot();
    void SendKeys(string text);
    void Submit();
}
public interface ISearchContext
{
    IWebElement FindElement(By by);
    ReadOnlyCollection<IWebElement> FindElements(By by);
}
public interface IAlert
{
    string Text { get; }

    void Accept();
    void Dismiss();
    void SendKeys(string keysToSend);
}
public enum WindowType
{
    Window = 0,
    Tab = 1
}
public interface ITargetLocator
{
    IWebElement ActiveElement();
    IAlert Alert();
    IWebDriver DefaultContent();
    IWebDriver Frame(int frameIndex);
    IWebDriver Frame(string frameName);
    IWebDriver Frame(IWebElement frameElement);
    IWebDriver NewWindow(WindowType typeHint);
    IWebDriver ParentFrame();
    IWebDriver Window(string windowName);
}
public interface INavigation
{
    void Back();
    void Forward();
    void GoToUrl(string url);
    void GoToUrl(Uri url);
    void Refresh();
}
public interface IWebDriver : ISearchContext, IDisposable
{
    string Url { get; set; }
    string Title { get; }
    string PageSource { get; }
    string CurrentWindowHandle { get; }
    ReadOnlyCollection<string> WindowHandles { get; }

    void Close();
    //IOptions Manage();
    INavigation Navigate();
    void Quit();
    ITargetLocator SwitchTo();
}