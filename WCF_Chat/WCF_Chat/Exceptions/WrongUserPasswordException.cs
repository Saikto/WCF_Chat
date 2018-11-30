using System;

namespace WCF_Chat.Exceptions
{
    public class WrongUserPasswordException : Exception
    {
        public WrongUserPasswordException()
        {
        }

        public WrongUserPasswordException(string message)
            : base(message)
        {
        }

        public WrongUserPasswordException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
