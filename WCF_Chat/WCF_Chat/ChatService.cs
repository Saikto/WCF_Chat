using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using WCF_Chat.Entities;
using WCF_Chat.Exceptions;
using WCF_Chat.Interfaces;
using WCF_Chat.Utility;

namespace WCF_Chat
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ChatService : IChatService
    {
        private readonly List<ServerUser> _onlineUsersList = new List<ServerUser>();
        private readonly List<ClientUser> _registeredUsers;
        private readonly IStorageHandler _storageHandler;

        ChatService()
        {
            _storageHandler = new FileStorage();
            _registeredUsers = _storageHandler.GetRegisteredUsers();
        }

        public ClientUser LogIn(string userName, string password, bool registrationRequired)
        {
            ClientUser user;
            try
            {
                user = registrationRequired
                    ? RegisterNewUser(userName, password)
                    : ValidateUserLogin(userName, password);
            }
            catch (UserAlreadyExistException e)
            {
                throw new FaultException(e.Message);
            }
            catch (UserNotRegisteredException e)
            {
                throw new FaultException(e.Message);
            }
            catch (WrongUserPasswordException e)
            {
                throw new FaultException(e.Message);
            }


            var userToConnect = new ServerUser()
            {
                Id = user.Id,
                UserName = user.UserName,
                OperationContext = OperationContext.Current
            };

            if (_onlineUsersList.FirstOrDefault(u => u.Id == userToConnect.Id) == null)
            {
                _onlineUsersList.Add(userToConnect);
                userToConnect.OperationContext.Channel.Faulted += (sender, e) => LogOff(userToConnect.Id);
                userToConnect.OperationContext.Channel.Closed += (sender, e) => LogOff(userToConnect.Id);
            }

            Console.WriteLine($"{DateTime.Now}: User {userName} online.");
            Console.WriteLine($"{DateTime.Now}: Users online count: {_onlineUsersList.Count}.");

            return user;
        }

        public void LogOff(int id)
        {
            var userToDisconnect = _onlineUsersList.FirstOrDefault(u => u.Id == id);
            if (userToDisconnect != null)
            {
                Console.WriteLine($"{DateTime.Now}: User {userToDisconnect.UserName} offline.");
                _onlineUsersList.Remove(userToDisconnect);
                Console.WriteLine($"{DateTime.Now}: Users online count: {_onlineUsersList.Count}.");
            }
        }

        public void SendMessage(Message message)
        {
            foreach (var user in _registeredUsers)
            {
                if (user.Equals(message.Receiver))
                {
                    try
                    {
                        _storageHandler.AddChatContact(message.Receiver, message.Sender);
                        _storageHandler.AddMessageToHistory(message.Sender, message.Receiver, message);
                        _storageHandler.AddMessageToHistory(message.Receiver, message.Sender, message);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            var onlineUser = _onlineUsersList.FirstOrDefault(u => u.Id == message.Receiver.Id);
            if (onlineUser != null)
            {
                onlineUser.OperationContext.GetCallbackChannel<IChatServerCallback>().MessageCallback(message);
            }
            
        }

        public ClientUser AddToChatList(ClientUser forUser, string contactUserName)
        {
            ClientUser contactToAdd = _storageHandler.GetUserByUserName(contactUserName);

            if (contactToAdd == null)
            {
                throw new FaultException($"Account with user name '{contactUserName}' is not registered. Please try again.");
            }

            try
            {
                _storageHandler.AddChatContact(forUser, contactToAdd);
            }
            catch (Exception)
            {
                throw new FaultException("Unknown error. Please try again.");
            }

            return contactToAdd;
        }

        public int DeleteFromChatList(ClientUser forUser, string contactUserName)
        {
            ClientUser contactToDelete = _storageHandler.GetUserByUserName(contactUserName);

            try
            {
                _storageHandler.DeleteChatContact(forUser, contactToDelete);
            }
            catch (Exception)
            {
                return 0; // Error
            }

            return 1; // OK
        }

        public List<Message> GetMessagesHistory(ClientUser forUser, ClientUser withUser)
        {
            return _storageHandler.GetMessagesHistory(forUser, withUser);
        }

        public List<ClientUser> GetChatList(ClientUser forUser)
        {
            return _storageHandler.GetContactsList(forUser);
        }

        private ClientUser RegisterNewUser(string userName, string password)
        {
            if (_storageHandler.UserNameExists(userName))
            {
                throw new UserAlreadyExistException(
                    $"Account with user name '{userName}' already exists. Please enter another user name and try again.");
            }

            int userId = _storageHandler.GetNextUserId();

            ClientUser newUser = new ClientUser(userId, userName);
            _registeredUsers.Add(newUser);

            _storageHandler.AddToRegisteredUsers(newUser, password);

            return newUser;
        }

        private ClientUser ValidateUserLogin(string userName, string password)
        {
            if (!_storageHandler.UserNameExists(userName))
            {
                throw new UserNotRegisteredException($"Account with user name '{userName}' is not registered. Please check if you entered correct user name for your account or choose option 'Registration required' to create new account.");
            }

            ClientUser user = _storageHandler.GetUserByUserName(userName);

            if (_storageHandler.ValidatePassword(user, password))
            {
                return user;
            }

            throw new WrongUserPasswordException($"You entered wrong password for account with user name '{userName}'. Please try again.");
        }
    }
}
