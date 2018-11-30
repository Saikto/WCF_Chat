using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Threading;
using System.Windows.Documents;
using ChatClient.ChatService;
using ChatClient.Exceptions;
using ChatClient.Utility;

namespace ChatClient
{
    public class ClientService: IChatServiceCallback
    {
        private ChatServiceClient _clientChatService;
        public ChatUser CurrentUser;

        public delegate void MessageReceivedHandler(Message message);
        public event MessageReceivedHandler MessageReceived;

        public ClientService()
        {
            StorageHandler.GetOrCreateAccountsDir();
        }

        public ChatUser FindContact(string userName)
        {
            return _clientChatService.FindUserByName(userName);
        }

        public int ConnectUser(string userName, string password, bool registrationRequired)
        {
            int resultCode = -666;
            _clientChatService = new ChatServiceClient(new InstanceContext(this));
            Thread t = new Thread(delegate()
            {
                resultCode = _clientChatService.LogIn(userName, password, registrationRequired); 

            });
            t.Start();
            t.Join(60000);
            
            if (resultCode > 0)
                return resultCode;
            switch (resultCode)
            {
                case 0:
                    throw new WrongUserPasswordException($"You entered wrong password for account with user name '{userName}'. Please try again.");
                case -1:
                    throw new UserNotRegisteredException($"Account with user name '{userName}' is not registered. Please check if you entered correct user name for your account or choose option 'Registration required' to create new account.");
                case -2:
                    throw new UserAlreadyExistException($"Account with user name '{userName}' already exists. Please enter another user name and try again.");
                case -666:
                    throw new ServerDidNotRespondException($"Sorry, server did not respond. Try again later.");
            }
            return resultCode;
        }

        public void DisconnectUser()
        {
            if (_clientChatService != null)
            {
                _clientChatService.LogOff(CurrentUser.Id);
                _clientChatService = null;
            }
        }

        public void SendMessage(Message message)
        {
            _clientChatService.SendMessage(message);
        }

        public void MessageCallback(Message message)
        {
            if (MessageReceived != null)
                MessageReceived(message);
        }
    }
}
