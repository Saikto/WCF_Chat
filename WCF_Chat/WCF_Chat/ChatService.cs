using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using WCF_Chat.Entities;
using WCF_Chat.Utility;

namespace WCF_Chat
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ChatService : IChatService
    {
        private readonly List<ChatUser> _onlineUsersList = new List<ChatUser>();
        private readonly Dictionary<int, string> _registeredUsers;
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
        public int LogIn(string userName, string password, bool registrationRequired)
        {
            var resultCode = registrationRequired ? RegisterNewUser(userName, password) : ValidateUserLogin(userName, password);

            if (resultCode > 0)
            {
                var userToConnect = new ChatUser()
                {
                    Id = resultCode,
                    UserName = userName,
                    OperationContext = OperationContext.Current
                };

                if (_onlineUsersList.FirstOrDefault(u => u.Id == userToConnect.Id) == null)
                {
                    _onlineUsersList.Add(userToConnect);
                    userToConnect.OperationContext.Channel.Faulted += (sender, e) => LogOff(userToConnect.Id);
                }

                Console.WriteLine($"{DateTime.Now}: User {userName} online.");
                Console.WriteLine($"{DateTime.Now}: Users online count: {_onlineUsersList.Count}.");
                return resultCode;
            }
            return resultCode;
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
                if (user.Key.Equals(message.Receiver.Id))
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

        public ChatUser AddToChatList(int forId, string userName)
        {
            ChatUser contact = FindUserByName(userName);

            if (contact != null)
            {
                try
                {
                    StorageHandler.AddChatContactToFile(forId, contact.Id, contact.UserName);
                }
                catch (Exception)
                {
                    return null;
                }
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

        public List<ChatUser> GetChatList(int forId)
        {
            return StorageHandler.GetContactsListFromFile(forId);
        }

        private ChatUser FindUserByName(string userName)
        {
            try
            {
                var found = _registeredUsers.First(c => c.Value == userName);
                var id = found.Key;
                return new ChatUser { Id = id, UserName = userName };
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        private int RegisterNewUser(string userName, string password)
        {
            if (StorageHandler.DoesUserNameExists(userName))
                return -2;

            int userId = StorageHandler.GenerateUserId();
            _registeredUsers.Add(userId, userName);
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
            return userId;
        }

        private int ValidateUserLogin(string userName, string password)
        {
            if (!StorageHandler.DoesUserNameExists(userName))
            {
                return -1;
            }

            int userId = StorageHandler.GetUserIdByUserName(userName);

            var passwordFile = StorageHandler.GetPasswordFile(userId);

            //bool passwordCorrect = byte.Equals(Encoding.UTF8.GetString(ComputeSha256Hash(password)), passwordHash);
            bool passwordCorrect = Equals(File.ReadAllText(passwordFile.FullName), password);

            if (passwordCorrect)
            {
                return _registeredUsers.First(u => u.Value == userName).Key;
            }

            return 0;
        }
    }
}
