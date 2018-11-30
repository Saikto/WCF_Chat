using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Windows.Documents;
using ChatClient.ChatService;
using ChatClient.Utility;

namespace ChatClient
{
    public class ClientService: IChatServiceCallback
    {
        private ChatServiceClient _serverService;
        public ClientUser CurrentUser;
        public Dictionary<ClientUser, List<Message>> ContactsMessageHistory;

        public delegate void MessageReceivedHandler(Message message);
        public event MessageReceivedHandler MessageReceived;

        public delegate void FaultExceptionHandler(FaultException ex);
        public event FaultExceptionHandler FaultExceptionThrown;

        public ClientService()
        {
            StorageHandler.GetOrCreateAccountsDir();
        }

        public void ConnectUser(string userName, string password, bool registrationRequired)
        {
            _serverService = new ChatServiceClient(new InstanceContext(this));

            Thread t = new Thread(delegate()
            {
                try
                {
                    CurrentUser = _serverService.LogIn(userName, password, registrationRequired);
                }
                catch (EndpointNotFoundException)
                {
                    FaultExceptionThrown?.Invoke(new FaultException("Sorry, server did not respond. Try again later."));
                }
                catch (FaultException e)
                {
                    FaultExceptionThrown?.Invoke(new FaultException(e.Message));
                }

            });
            t.Start();
            t.Join(60000);

            if (CurrentUser != null)
            {
                ContactsMessageHistory = GetAllMessageHistoryFromServer();
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
            ClientUser user;
            try
            {
                user = _serverService.AddToChatList(CurrentUser.Id, contactToAdd);
            }
            catch (FaultException e)
            {
                throw new FaultException(e.Message);
            }

            ContactsMessageHistory.Add(user, _serverService.GetMessagesHistory(CurrentUser.Id, user.Id).ToList());
        }

        public void DeleteFromChatList(ClientUser contactDelete)
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
                    List<Message> newHistory = new List<Message> {message};
                    ContactsMessageHistory.Add(message.Sender, newHistory);
                }
                else
                {
                    ContactsMessageHistory.FirstOrDefault(s => s.Key.Id == message.Sender.Id).Value.Add(message);
                }
                MessageReceived(message);
            }
        }

        private Dictionary<ClientUser, List<Message>> GetAllMessageHistoryFromServer()
        {
            Dictionary<ClientUser, List<Message>> messageHistoryDict = new Dictionary<ClientUser, List<Message>>();
            List<ClientUser> contactsList = _serverService.GetChatList(CurrentUser.Id).ToList();

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
