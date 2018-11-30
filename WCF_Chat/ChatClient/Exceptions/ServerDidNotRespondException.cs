using System;

namespace ChatClient.Exceptions
{
    public class ServerDidNotRespondException : Exception
    {
        public ServerDidNotRespondException()
        {
        }

        public ServerDidNotRespondException(string message)
            : base(message)
        {
        }

        public ServerDidNotRespondException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
