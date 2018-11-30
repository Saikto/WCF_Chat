using System;

namespace ChatClient.Exceptions
{
    public class UserNotRegisteredException : Exception
    {
        public UserNotRegisteredException()
        {
        }

        public UserNotRegisteredException(string message)
            : base(message)
        {
        }

        public UserNotRegisteredException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
