using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using WCF_Chat.Entities;
using WCF_Chat.Interfaces;

namespace WCF_Chat.Utility
{
    public class FileStorage: IStorageHandler
    {
        private readonly DirectoryInfo _accountStorageDirInfo;
        private readonly FileInfo _registeredUsersFileInfo;

        public FileStorage()
        {
            _accountStorageDirInfo = !Directory.Exists("UsersAccounts") ? Directory.CreateDirectory("UsersAccounts") : new DirectoryInfo("UsersAccounts");
            _registeredUsersFileInfo = GetRegisteredUsersFileInfo();
        }

        #region private

        /// <summary>
        /// Returns instance of DirectoryInfo for folder of particular user in account storage 
        /// </summary>
        private DirectoryInfo GetUserDir(int forId)
        {
            var usersDirectoryInfos = _accountStorageDirInfo.GetDirectories();
            return usersDirectoryInfos.FirstOrDefault(d => Int32.Parse(d.Name) == forId);
        }

        /// <summary>
        /// Returns array of FileInfos for files that store in particular user's directory in account storage
        /// </summary>
        private FileInfo[] GetUserFiles(int forId)
        {
            return GetUserDir(forId).GetFiles();
        }

        /// <summary>
        /// Gets FileInfo instance that contain information about file with user password
        /// </summary>
        private FileInfo GetPasswordFile(int forId)
        {
            return GetUserFiles(forId).FirstOrDefault(x => x.Name == "password.txt");
        }

        /// <summary>
        /// Creates directory named according to provided ID and password.txt, chatContacts.xml inside this folder (part of registration process)
        /// </summary>
        private void CreateUserFilesInStorage(int forId)
        {
            _accountStorageDirInfo.CreateSubdirectory($"{forId}");

            var passwordFilePath = Path.Combine(GetUserDir(forId).FullName, "password.txt");
            FileStream fs = File.Create(passwordFilePath);
            fs.Close();

            CreateContactsFile(forId);
        }

        /// <summary>
        /// Creates chatContacts.xml inside user directory
        /// </summary>
        private void CreateContactsFile(int forId)
        {
            XDocument saveDoc = new XDocument();
            XElement contactsRoot = new XElement("ChatContactsList");
            saveDoc.Add(contactsRoot);
            saveDoc.Save(Path.Combine(GetUserDir(forId).FullName, "chatContacts.xml"));
        }

        /// <summary>
        /// Returns instance of FileInfo for users.txt file that contains strings (userID userName)
        /// </summary>
        private FileInfo GetRegisteredUsersFileInfo()
        {
            var usersFilePath = Path.Combine(_accountStorageDirInfo.FullName, "users.txt");

            if (!File.Exists(usersFilePath))
            {
                FileStream fs = File.Create(usersFilePath);
                fs.Close();
            }
            return new FileInfo(usersFilePath);
        }

        /// <summary>
        /// Creates message history file with particular contact inside user folder
        /// </summary>
        private void CreateMessagesHistoryFile(int forId, int withId)
        {
            FileInfo messagesHistoryFile = GetUserFiles(forId).FirstOrDefault(x => x.Name == $"messages_{withId}.xml");

            if (messagesHistoryFile == null)
            {
                XDocument saveDoc = new XDocument();
                XElement messagesRoot = new XElement("MessagesList");
                saveDoc.Add(messagesRoot);
                saveDoc.Save(Path.Combine(GetUserDir(forId).FullName, $"messages_{withId}.xml"));
            }
        }

        /// <summary>
        /// Computes SHA256 hash of string
        /// </summary>
        private byte[] ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                return bytes;
            }
        }

        #endregion

        #region public

        /// <summary>
        /// Gets next available user id (reads last used ID from users.txt and returns next int number). First Id == 1
        /// </summary>
        public int GetNextUserId()
        {
            int previousId;
            if (GetRegisteredUsers().Count > 0)
            {
                previousId = GetRegisteredUsers().Last().Id;
                previousId++;
                return previousId;
            }
            return previousId = 1;
        }

        /// <summary>
        /// Returns ClientUser instance from registered users for user with provided username 
        /// </summary>
        public ClientUser GetUserByUserName(string userName)
        {
            return GetRegisteredUsers().FirstOrDefault(u => u.UserName == userName);
        }

        /// <summary>
        /// Verifies if user with such user name present in users.txt (means registered)
        /// </summary>
        public bool UserNameExists(string userName)
        {
            return GetRegisteredUsers().FirstOrDefault(u => u.UserName == userName) != null;
        }

        /// <summary>
        /// Compares provided password for particular user with it's value in password.txt
        /// </summary>
        public bool ValidatePassword(ClientUser forUser, string password)
        {
            FileInfo passwordFile = GetPasswordFile(forUser.Id);
            string expectedPassword = File.ReadAllText(passwordFile.FullName);

            return string.Equals(expectedPassword, password);
        }

        /// <summary>
        /// Returns list of all registered users. Reads users.txt file from account storage
        /// </summary>
        public List<ClientUser> GetRegisteredUsers()
        {
            List<ClientUser> users = new List<ClientUser>();

            foreach (var userLine in File.ReadAllLines(_registeredUsersFileInfo.FullName))
            {
                var array = userLine.Split(' ');
                users.Add(new ClientUser(int.Parse(array[0]), array[1]));
            }

            return users;
        }

        /// <summary>
        /// Returns list of messages between two provided users
        /// </summary>
        public List<Message> GetMessagesHistory(ClientUser forUser, ClientUser withUser)
        {
            List<Message> messagesList = new List<Message>();
            FileInfo messagesHistoryFile = GetUserFiles(forUser.Id).FirstOrDefault(x => x.Name == $"messages_{withUser.Id}.xml");

            if (messagesHistoryFile != null)
            {
                XElement messagesDoc = XElement.Load(messagesHistoryFile.FullName);
                List<XElement> messagesElements = messagesDoc.Descendants("message").ToList();
                foreach (var messagesElement in messagesElements)
                {
                    messagesList.Add(new Message()
                    {
                        SendTime = DateTime.Parse(messagesElement.FirstAttribute.Value),
                        Sender = new ClientUser { UserName = messagesElement.LastAttribute.Value },
                        MessageText = messagesElement.Value
                    });

                }
            }
            return messagesList;
        }

        /// <summary>
        /// Returns list of contacts for provided user. Reads from chatContacts.xml
        /// </summary>
        public List<ClientUser> GetContactsList(ClientUser forUser)
        {
            List<ClientUser> contactsList = new List<ClientUser>();
            FileInfo contactsFile = GetUserFiles(forUser.Id).FirstOrDefault(x => x.Name == "chatContacts.xml");

            if (contactsFile != null)
            {
                XElement contactsDoc = XElement.Load(contactsFile.FullName);
                List<XElement> contactElements = contactsDoc.Descendants("contact").ToList();
                foreach (var contactElement in contactElements)
                {
                    contactsList.Add(new ClientUser
                    {
                        Id = Int32.Parse(contactElement.FirstAttribute.Value),
                        UserName = contactElement.LastAttribute.Value
                    });

                }
            }
            return contactsList;
        }

        /// <summary>
        /// Adds provided user to file storage. Creates folder, password file, adds to users.txt
        /// </summary>
        public void AddToRegisteredUsers(ClientUser user, string password)
        {
            CreateUserFilesInStorage(user.Id);
            var passwordFile = GetPasswordFile(user.Id);
            File.AppendAllText(passwordFile.FullName, password);

            if (string.IsNullOrWhiteSpace(File.ReadAllText(_registeredUsersFileInfo.FullName)))
            {
                File.AppendAllText(_registeredUsersFileInfo.FullName, $"{user.Id} {user.UserName}");
            }
            else
            {
                File.AppendAllText(_registeredUsersFileInfo.FullName, $"\n{user.Id} {user.UserName}");
            }
        }

        /// <summary>
        /// Adds message to messages history file for provided user
        /// </summary>
        public void AddMessageToHistory(ClientUser forUser, ClientUser withUser, Message message)
        {
            FileInfo messagesHistoryFile = GetUserFiles(forUser.Id).FirstOrDefault(x => x.Name == $"messages_{withUser.Id}.xml");
            if (messagesHistoryFile == null)
            {
                CreateMessagesHistoryFile(forUser.Id, withUser.Id);
            }

            if (messagesHistoryFile != null)
            {
                XElement messagesDoc = XElement.Load(messagesHistoryFile.FullName);
                XElement messageElement = new XElement("message") { Value = message.MessageText };
                XAttribute timeAttribute = new XAttribute("time", message.SendTime.ToShortTimeString());
                XAttribute senderAttribute = new XAttribute("username", message.Sender.UserName);
                messageElement.Add(timeAttribute);
                messageElement.Add(senderAttribute);
                messagesDoc.Add(messageElement);
                messagesDoc.Save(messagesHistoryFile.FullName);
            }
        }

        /// <summary>
        /// Adds chat contact into chatContacts.xml for provided user
        /// </summary>
        public void AddChatContact(ClientUser forUser, ClientUser contactToAdd)
        {
            FileInfo contactsFile = GetUserFiles(forUser.Id).FirstOrDefault(x => x.Name == "chatContacts.xml");
            if (contactsFile != null)
            {
                CreateMessagesHistoryFile(forUser.Id, contactToAdd.Id);
            }

            if (!GetContactsList(forUser).Contains(contactToAdd))
                return;

            XElement contactsDoc = XElement.Load(contactsFile.FullName);
            XElement contactElement = new XElement("contact");
            XAttribute idAttribute = new XAttribute("id", contactToAdd.Id);
            XAttribute usernameAttribute = new XAttribute("username", contactToAdd.UserName);
            contactElement.Add(idAttribute);
            contactElement.Add(usernameAttribute);
            contactsDoc.Add(contactElement);
            contactsDoc.Save(contactsFile.FullName);
        }

        /// <summary>
        /// Removes chat contact from chatContacts.xml for provided user
        /// </summary>
        public void DeleteChatContact(ClientUser forUser, ClientUser contactToDelete)
        {
            if (contactToDelete.UserName == null)
                return;
            FileInfo contactsFile = GetUserFiles(forUser.Id).FirstOrDefault(x => x.Name == "chatContacts.xml");
            if (contactsFile != null)
            {
                XElement contactsDoc = XElement.Load(contactsFile.FullName);
                List<XElement> contactElements = contactsDoc.Descendants("contact").ToList();
                int indexToDel = contactElements.FindIndex(c => c.LastAttribute.Value == contactToDelete.UserName);
                contactElements.RemoveAt(indexToDel);
                contactsDoc.ReplaceAll(contactElements);
                contactsDoc.Save(contactsFile.FullName);
            }
        }

        #endregion
    }
}
