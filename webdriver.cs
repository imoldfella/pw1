namespace pw1;

using System.Collections.ObjectModel;
using System.Drawing;
using System;
using System.Globalization;

// modified for nullable
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

// each webelement is a search context.
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

    public interface IWait<T>
    {
        /// <summary>
        /// Gets or sets how long to wait for the evaluated condition to be true.
        /// </summary>
        TimeSpan Timeout { get; set; }

        /// <summary>
        /// Gets or sets how often the condition should be evaluated.
        /// </summary>
        TimeSpan PollingInterval { get; set; }

        /// <summary>
        /// Gets or sets the message to be displayed when time expires.
        /// </summary>
        string Message { get; set; }

        /// <summary>
        /// Configures this instance to ignore specific types of exceptions while waiting for a condition.
        /// Any exceptions not whitelisted will be allowed to propagate, terminating the wait.
        /// </summary>
        /// <param name="exceptionTypes">The types of exceptions to ignore.</param>
        void IgnoreExceptionTypes(params Type[] exceptionTypes);

        /// <summary>
        /// Waits until a condition is true or times out.
        /// </summary>
        /// <typeparam name="TResult">The type of result to expect from the condition.</typeparam>
        /// <param name="condition">A delegate taking a TSource as its parameter, and returning a TResult.</param>
        /// <returns>If TResult is a boolean, the method returns <see langword="true"/> when the condition is true, and <see langword="false"/> otherwise.
        /// If TResult is an object, the method returns the object when the condition evaluates to a value other than <see langword="null"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when TResult is not boolean or an object type.</exception>
        TResult Until<TResult>(Func<T, TResult> condition);
    }
    public interface IClock
    {
        /// <summary>
        /// Gets the current date and time values.
        /// </summary>
        DateTime Now { get; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> at a specified offset in the future.
        /// </summary>
        /// <param name="delay">The offset to use.</param>
        /// <returns>The <see cref="DateTime"/> at the specified offset in the future.</returns>
        DateTime LaterBy(TimeSpan delay);

        /// <summary>
        /// Gets a value indicating whether the current date and time is before the specified date and time.
        /// </summary>
        /// <param name="otherDateTime">The date and time values to compare the current date and time values to.</param>
        /// <returns><see langword="true"/> if the current date and time is before the specified date and time; otherwise, <see langword="false"/>.</returns>
        bool IsNowBefore(DateTime otherDateTime);
    }
        public class SystemClock : IClock
    {
        /// <summary>
        /// Gets the current date and time values.
        /// </summary>
        public DateTime Now
        {
            get { return DateTime.Now; }
        }

        /// <summary>
        /// Calculates the date and time values after a specific delay.
        /// </summary>
        /// <param name="delay">The delay after to calculate.</param>
        /// <returns>The future date and time values.</returns>
        public DateTime LaterBy(TimeSpan delay)
        {
            return DateTime.Now.Add(delay);
        }

        /// <summary>
        /// Gets a value indicating whether the current date and time is before the specified date and time.
        /// </summary>
        /// <param name="otherDateTime">The date and time values to compare the current date and time values to.</param>
        /// <returns><see langword="true"/> if the current date and time is before the specified date and time; otherwise, <see langword="false"/>.</returns>
        public bool IsNowBefore(DateTime otherDateTime)
        {
            return DateTime.Now < otherDateTime;
        }
    }
public class DefaultWait<T> : IWait<T>
    {
        private T input;
        private IClock clock;

        private TimeSpan timeout = DefaultSleepTimeout;
        private TimeSpan sleepInterval = DefaultSleepTimeout;
        private string message = string.Empty;

        private List<Type> ignoredExceptions = new List<Type>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultWait&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="input">The input value to pass to the evaluated conditions.</param>
        public DefaultWait(T input)
            : this(input, new SystemClock())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultWait&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="input">The input value to pass to the evaluated conditions.</param>
        /// <param name="clock">The clock to use when measuring the timeout.</param>
        public DefaultWait(T input, IClock clock)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input), "input cannot be null");
            }

            if (clock == null)
            {
                throw new ArgumentNullException(nameof(clock), "clock cannot be null");
            }

            this.input = input;
            this.clock = clock;
        }

        /// <summary>
        /// Gets or sets how long to wait for the evaluated condition to be true. The default timeout is 500 milliseconds.
        /// </summary>
        public TimeSpan Timeout
        {
            get { return this.timeout; }
            set { this.timeout = value; }
        }

        /// <summary>
        /// Gets or sets how often the condition should be evaluated. The default timeout is 500 milliseconds.
        /// </summary>
        public TimeSpan PollingInterval
        {
            get { return this.sleepInterval; }
            set { this.sleepInterval = value; }
        }

        /// <summary>
        /// Gets or sets the message to be displayed when time expires.
        /// </summary>
        public string Message
        {
            get { return this.message; }
            set { this.message = value; }
        }

        private static TimeSpan DefaultSleepTimeout
        {
            get { return TimeSpan.FromMilliseconds(500); }
        }

        /// <summary>
        /// Configures this instance to ignore specific types of exceptions while waiting for a condition.
        /// Any exceptions not whitelisted will be allowed to propagate, terminating the wait.
        /// </summary>
        /// <param name="exceptionTypes">The types of exceptions to ignore.</param>
        public void IgnoreExceptionTypes(params Type[] exceptionTypes)
        {
            if (exceptionTypes == null)
            {
                throw new ArgumentNullException(nameof(exceptionTypes), "exceptionTypes cannot be null");
            }

            foreach (Type exceptionType in exceptionTypes)
            {
                if (!typeof(Exception).IsAssignableFrom(exceptionType))
                {
                    throw new ArgumentException("All types to be ignored must derive from System.Exception", nameof(exceptionTypes));
                }
            }

            this.ignoredExceptions.AddRange(exceptionTypes);
        }

        /// <summary>
        /// Repeatedly applies this instance's input value to the given function until one of the following
        /// occurs:
        /// <para>
        /// <list type="bullet">
        /// <item>the function returns neither null nor false</item>
        /// <item>the function throws an exception that is not in the list of ignored exception types</item>
        /// <item>the timeout expires</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <typeparam name="TResult">The delegate's expected return type.</typeparam>
        /// <param name="condition">A delegate taking an object of type T as its parameter, and returning a TResult.</param>
        /// <returns>The delegate's return value.</returns>
        public virtual TResult Until<TResult>(Func<T, TResult> condition)
        {
            return Until(condition, CancellationToken.None);
        }

        /// <summary>
        /// Repeatedly applies this instance's input value to the given function until one of the following
        /// occurs:
        /// <para>
        /// <list type="bullet">
        /// <item>the function returns neither null nor false</item>
        /// <item>the function throws an exception that is not in the list of ignored exception types</item>
        /// <item>the timeout expires</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <typeparam name="TResult">The delegate's expected return type.</typeparam>
        /// <param name="condition">A delegate taking an object of type T as its parameter, and returning a TResult.</param>
        /// <param name="token">A cancellation token that can be used to cancel the wait.</param>
        /// <returns>The delegate's return value.</returns>
        public virtual TResult Until<TResult>(Func<T, TResult> condition, CancellationToken token)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition), "condition cannot be null");
            }

            var resultType = typeof(TResult);
            if ((resultType.IsValueType && resultType != typeof(bool)) || !typeof(object).IsAssignableFrom(resultType))
            {
                throw new ArgumentException("Can only wait on an object or boolean response, tried to use type: " + resultType.ToString(), nameof(condition));
            }

            Exception lastException = null;
            var endTime = this.clock.LaterBy(this.timeout);
            while (true)
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    var result = condition(this.input);
                    if (resultType == typeof(bool))
                    {
                        var boolResult = result as bool?;
                        if (boolResult.HasValue && boolResult.Value)
                        {
                            return result;
                        }
                    }
                    else
                    {
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!this.IsIgnoredException(ex))
                    {
                        throw;
                    }

                    lastException = ex;
                }

                // Check the timeout after evaluating the function to ensure conditions
                // with a zero timeout can succeed.
                if (!this.clock.IsNowBefore(endTime))
                {
                    string timeoutMessage = string.Format(CultureInfo.InvariantCulture, "Timed out after {0} seconds", this.timeout.TotalSeconds);
                    if (!string.IsNullOrEmpty(this.message))
                    {
                        timeoutMessage += ": " + this.message;
                    }

                    this.ThrowTimeoutException(timeoutMessage, lastException);
                }

                Thread.Sleep(this.sleepInterval);
            }
        }

        /// <summary>
        /// Throws a <see cref="WebDriverTimeoutException"/> with the given message.
        /// </summary>
        /// <param name="exceptionMessage">The message of the exception.</param>
        /// <param name="lastException">The last exception thrown by the condition.</param>
        /// <remarks>This method may be overridden to throw an exception that is
        /// idiomatic for a particular test infrastructure.</remarks>
        protected virtual void ThrowTimeoutException(string exceptionMessage, Exception lastException)
        {
            throw new WebDriverTimeoutException(exceptionMessage, lastException);
        }

        private bool IsIgnoredException(Exception exception)
        {
            return this.ignoredExceptions.Any(type => type.IsAssignableFrom(exception.GetType()));
        }
    }
public class WebDriverWait : DefaultWait<IWebDriver>
{

    public WebDriverWait(IWebDriver driver, TimeSpan timeout)
        : this(new SystemClock(), driver, timeout, DefaultSleepTimeout)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverWait"/> class.
    /// </summary>
    /// <param name="clock">An object implementing the <see cref="IClock"/> interface used to determine when time has passed.</param>
    /// <param name="driver">The WebDriver instance used to wait.</param>
    /// <param name="timeout">The timeout value indicating how long to wait for the condition.</param>
    /// <param name="sleepInterval">A <see cref="TimeSpan"/> value indicating how often to check for the condition to be true.</param>
    public WebDriverWait(IClock clock, IWebDriver driver, TimeSpan timeout, TimeSpan sleepInterval)
        : base(driver, clock)
    {
        this.Timeout = timeout;
        this.PollingInterval = sleepInterval;
        //this.IgnoreExceptionTypes(typeof(NotFoundException));
    }

    private static TimeSpan DefaultSleepTimeout
    {
        get { return TimeSpan.FromMilliseconds(500); }
    }
}

    public class WebDriverException : Exception{    
        public string message;
        public WebDriverException(string message)
        {
            this.message = message;
        }
        public WebDriverException()
        {
            this.message = "WebDriverException";
        }   
        public WebDriverException(string message, Exception innerException)
        {
            this.message = message;
        }
    }

    public class WebDriverTimeoutException : WebDriverException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDriverTimeoutException"/> class.
        /// </summary>
        public WebDriverTimeoutException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDriverTimeoutException"/> class with
        /// a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public WebDriverTimeoutException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDriverTimeoutException"/> class with
        /// a specified error message and a reference to the inner exception that is the
        /// cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception,
        /// or <see langword="null"/> if no inner exception is specified.</param>
        public WebDriverTimeoutException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

  
        
    }