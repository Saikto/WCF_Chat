using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly List<ChatUser> _onlineUsers = new List<ChatUser>();
        private readonly Dictionary<int, string> _registeredUsers;

        ChatService()
        {
            StorageHandler.GetAccountsDir();
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
                _onlineUsers.Add(userToConnect);
                Console.WriteLine($"User {userName} is online now. Users online count: {_onlineUsers.Count}.");
                return resultCode;
            }
            return resultCode;
        }

        public void LogOff(int id)
        {
            var userToDisconnect = _onlineUsers.FirstOrDefault(u => u.Id == id);
            if (userToDisconnect != null)
            {
                Console.WriteLine($"User {userToDisconnect.UserName} offline now.");
                _onlineUsers.Remove(userToDisconnect);
                Console.WriteLine($"Users online count: {_onlineUsers.Count}.");
            }
        }

        public ChatUser FindUserByName(string userName)
        {
            try
            {
                var found = _registeredUsers.First(c => c.Value == userName);
                var id = found.Key;
                return new ChatUser { Id = id, UserName = userName};
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            
        }

        public void SendMessage(Message message)
        {
            foreach (var user in _onlineUsers)
            {
                if (user.Id.Equals(message.Receiver.Id))
                {
                    try
                    {
                        user.OperationContext.GetCallbackChannel<IChatServerCallback>().MessageCallback(message);
                    }
                    catch (TimeoutException)
                    {
                    }
                }
            }
        }

        private int ValidateUserLogin(string userName, string password)
        {
            if (!StorageHandler.DoesUserExists(userName))
                return -1;

            var passwordFile = StorageHandler.GetPasswordFile(userName);
            
            //bool passwordCorrect = byte.Equals(Encoding.UTF8.GetString(ComputeSha256Hash(password)), passwordHash);
            bool passwordCorrect = Equals(File.ReadAllText(passwordFile.FullName), password);

            if (passwordCorrect)
            {
                return _registeredUsers.First(u => u.Value == userName).Key;
            }

            return 0; 
        }

        private int RegisterNewUser(string userName, string password)
        {
            if (StorageHandler.DoesUserExists(userName))
                return -2;

            StorageHandler.CreateUserDir(userName);
            StorageHandler.CreateUserFiles(userName);
            var passwordFile = StorageHandler.GetPasswordFile(userName);

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
            int userId = StorageHandler.GenerateUserId();

            _registeredUsers.Add(userId, userName);
            StorageHandler.AddToRegisteredUsersFile(userId, userName);
            return userId;
        }
    }
}
