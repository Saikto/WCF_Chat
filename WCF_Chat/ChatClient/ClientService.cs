using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using ChatClient.ChatService;
using ChatClient.Exceptions;
using ChatClient.Utility;

namespace ChatClient
{
    public class ClientService: IChatServiceCallback
    {
        private ChatServiceClient _serverService;
        public ChatUser CurrentUser;
        public Dictionary<ChatUser, List<Message>> ContactsMessageHistory;

        public delegate void MessageReceivedHandler(Message message);
        public event MessageReceivedHandler MessageReceived;

        public ClientService()
        {
            StorageHandler.GetOrCreateAccountsDir();
        }

        public void ConnectUser(string userName, string password, bool registrationRequired)
        {
            int resultCode = -666;
            _serverService = new ChatServiceClient(new InstanceContext(this));
            Thread t = new Thread(delegate()
            {
                try
                {
                    resultCode = _serverService.LogIn(userName, password, registrationRequired);
                }
                catch (EndpointNotFoundException)
                {
                    resultCode = -666;
                }
                 
            });
            t.Start();
            t.Join(60000);

            if (resultCode > 0)
            {
                CurrentUser = new ChatUser { Id = resultCode, UserName = userName };
                ContactsMessageHistory = GetAllMessageHistoryFromServer();
            }

            switch (resultCode)
            {
                case 0:
                    throw new WrongUserPasswordException($"You entered wrong password for account with user name '{userName}'. Please try again.");
                case -1:
                    throw new UserNotRegisteredException($"Account with user name '{userName}' is not registered. Please check if you entered correct user name for your account or choose option 'Registration required' to create new account.");
                case -2:
                    throw new UserAlreadyExistException($"Account with user name '{userName}' already exists. Please enter another user name and try again.");
                case -666:
                    throw new ServerDidNotRespondException("Sorry, server did not respond. Try again later.");
            }
        }

        public void DisconnectUser()
        {
            if (_serverService != null)
            {
                _serverService.LogOff(CurrentUser.Id);
                _serverService = null;
            }
        }

        public void SendMessage(Message message)
        {
            _serverService.SendMessage(message);
            ContactsMessageHistory.FirstOrDefault(u => u.Key.UserName == message.Receiver.UserName).Value.Add(message);
        }

        public void AddToChatList(string contactToAdd)
        {
            ChatUser user = _serverService.AddToChatList(CurrentUser.Id, contactToAdd);
            if (user != null)
            {
                ContactsMessageHistory.Add(user, _serverService.GetMessagesHistory(CurrentUser.Id, user.Id).ToList());
            }
            else
            {
                throw new UserNotRegisteredException(
                    $"Account with user name '{contactToAdd}' is not registered. Please try again.");
            }
        }

        public void DeleteFromChatList(ChatUser contactDelete)
        {
            int resultCode = _serverService.DeleteFromChatList(CurrentUser.Id, contactDelete.UserName);
            switch (resultCode)
            {
                case 0:
                    throw new Exception("Unknown error occured. Please try again.");
                case 1:
                    ContactsMessageHistory.Remove(contactDelete);
                    return;
            }

        }

        public void MessageCallback(Message message)
        {
            if (MessageReceived != null)
            {
                if (ContactsMessageHistory.Keys.FirstOrDefault(u => u.Id == message.Sender.Id) == null)
                {
                    List<Message> newHistory = new List<Message>();
                    newHistory.Add(message);
                    ContactsMessageHistory.Add(message.Sender, newHistory);
                }
                else
                {
                    ContactsMessageHistory.FirstOrDefault(s => s.Key.Id == message.Sender.Id).Value.Add(message);
                }
                MessageReceived(message);
            }
        }

        private Dictionary<ChatUser, List<Message>> GetAllMessageHistoryFromServer()
        {
            Dictionary<ChatUser, List<Message>> messageHistoryDict = new Dictionary<ChatUser, List<Message>>();
            List<ChatUser> contactsList = _serverService.GetChatList(CurrentUser.Id).ToList();

            foreach (var contact in contactsList)
            {
                List<Message> messageHistory =
                    _serverService.GetMessagesHistory(CurrentUser.Id, contact.Id).ToList();
                messageHistoryDict.Add(contact, messageHistory);
            }

            return messageHistoryDict;
        }
    }
}
