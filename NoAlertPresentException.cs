using System.Runtime.Serialization;

namespace pw1
{
    [Serializable]
    internal class NoAlertPresentException : Exception
    {
        public NoAlertPresentException()
        {
        }

        public NoAlertPresentException(string? message) : base(message)
        {
        }

        public NoAlertPresentException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected NoAlertPresentException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}