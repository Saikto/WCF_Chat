using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using ChatClient.Exceptions;
using WCF_Chat.Entities;
using WCF_Chat.Utility;

namespace WCF_Chat
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ChatService : IChatService
    {
        private readonly List<ServerUser> _onlineUsersList = new List<ServerUser>();
        private readonly List<ClientUser> _registeredUsers;
        private StorageHandler _storageHandler;

        ChatService()
        {
            _storageHandler = new StorageHandler();
            _registeredUsers = StorageHandler.GetRegisteredUsers();
        }

        /// <summary>
        /// Logs in or registers new user on server side. 
        /// </summary>
        /// <returns> Connected user ID if success. Else returns error code.</returns>
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
                        StorageHandler.AddChatContactToFile(message.Receiver.Id, message.Sender.Id, message.Sender.UserName);
                        StorageHandler.AddToMessagesHistoryFile(message.Sender.Id, message.Receiver.Id, message);
                        StorageHandler.AddToMessagesHistoryFile(message.Receiver.Id, message.Sender.Id, message);
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

        public ClientUser AddToChatList(int forId, string userName)
        {
            ClientUser contact = FindUserByName(userName);

            if (contact == null)
            {
                throw new FaultException($"Account with user name '{userName}' is not registered. Please try again.");
            }

            try
            {
                StorageHandler.AddChatContactToFile(forId, contact.Id, contact.UserName);
            }
            catch (Exception)
            {
                throw new FaultException("Unknown error. Please try again.");
            }

            return contact;
        }

        public int DeleteFromChatList(int forId, string userName)
        {
            try
            {
                StorageHandler.DeleteChatContactFromFile(forId, userName);
            }
            catch (Exception)
            {
                return 0; // Error
            }

            return 1; // OK
        }

        public List<Message> GetMessagesHistory(int forId, int withId)
        {
            return StorageHandler.GetMessagesHistory(forId, withId);
        }

        public List<ClientUser> GetChatList(int forId)
        {
            return StorageHandler.GetContactsListFromFile(forId);
        }

        private ClientUser FindUserByName(string userName)
        {
            return _registeredUsers.FirstOrDefault(u => u.UserName == userName);
        }

        private ClientUser RegisterNewUser(string userName, string password)
        {
            if (StorageHandler.UserNameExists(userName))
            {
                throw new UserAlreadyExistException(
                    $"Account with user name '{userName}' already exists. Please enter another user name and try again.");
            }

            int userId = StorageHandler.GetNextUserId();

            ClientUser newUser = new ClientUser(userId, userName);
            _registeredUsers.Add(newUser);

            StorageHandler.AddToRegisteredUsersFile(userId, userName);
            StorageHandler.CreateUserFilesInStorage(userId);
            var passwordFile = StorageHandler.GetPasswordFile(userId);

            File.AppendAllText(passwordFile.FullName, password);
            //using (var fStream = File.Create(userFilePath))
            //{
            //    var passwordHash = ComputeSha256Hash(password);
            //    foreach (var byteSymbol in passwordHash)
            //    {
            //        fStream.WriteByte(byteSymbol);
            //    }

            //    fStream.Flush();
            //}
            return newUser;
        }

        private ClientUser ValidateUserLogin(string userName, string password)
        {
            if (!StorageHandler.UserNameExists(userName))
            {
                throw new UserNotRegisteredException($"Account with user name '{userName}' is not registered. Please check if you entered correct user name for your account or choose option 'Registration required' to create new account.");
            }

            int userId = StorageHandler.GetUserIdByUserName(userName);

            var passwordFile = StorageHandler.GetPasswordFile(userId);

            //bool passwordCorrect = byte.Equals(Encoding.UTF8.GetString(ComputeSha256Hash(password)), passwordHash);
            bool passwordCorrect = Equals(File.ReadAllText(passwordFile.FullName), password);

            if (passwordCorrect)
            {
                return _registeredUsers.First(u => u.UserName == userName);
            }

            throw new WrongUserPasswordException($"You entered wrong password for account with user name '{userName}'. Please try again.");
        }
    }
}
