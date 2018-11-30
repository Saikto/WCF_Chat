using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using WCF_Chat.Entities;

namespace WCF_Chat.Utility
{
    public class StorageHandler
    {
        private static DirectoryInfo _accountStorageDirInfo;
        private static FileInfo _registeredUsersFileInfo;

        public StorageHandler()
        {
            _accountStorageDirInfo = !Directory.Exists("UsersAccounts") ? Directory.CreateDirectory("UsersAccounts") : new DirectoryInfo("UsersAccounts");
            _registeredUsersFileInfo = GetRegisteredUsersFileInfo();
        }

        public static byte[] ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                return bytes;
            }
        }

        /// <summary>
        /// Gets next available user id (reads last used ID from users.txt and returns next int number). First Id == 1
        /// </summary>
        public static int GenerateUserId()
        {
            int previousId;
            if (GetRegisteredUsers().Keys.Count > 0)
            {
                previousId = GetRegisteredUsers().Keys.Last();
                previousId++;
                return previousId;
            }
            return previousId = 1;
        }

        /// <summary>
        /// Verifies if user with such user name present in users.txt (means registered)
        /// </summary>
        public static bool DoesUserNameExists(string userName)
        {
            return GetRegisteredUsers().Values.Contains(userName);
        }

        public static int GetUserIdByUserName(string userName)
        {
            return GetRegisteredUsers().FirstOrDefault(u => u.Value == userName).Key;
        }

        /// <summary>
        /// Returns instance of DirectoryInfo for folder of particular user in account storage 
        /// </summary>
        private static DirectoryInfo GetUserDir(int forId)
        {
            var usersDirectoryInfos = _accountStorageDirInfo.GetDirectories();
            return usersDirectoryInfos.FirstOrDefault(d => Int32.Parse(d.Name) == forId);
        }

        /// <summary>
        /// Returns array of FileInfos for files that store in particular user's directory in account storage
        /// </summary>
        private static FileInfo[] GetUserFiles(int forId)
        {
            return GetUserDir(forId).GetFiles();
        }

        /// <summary>
        /// Gets FileInfo instance that contain information about file with user password
        /// </summary>
        public static FileInfo GetPasswordFile(int forId)
        {
            return GetUserFiles(forId).FirstOrDefault(x => x.Name == "password.txt");
        }

        /// <summary>
        /// Reads list of all registered users (id username) from FileInfo of users.txt file from account storage
        /// </summary>
        public static Dictionary<int, string> GetRegisteredUsers()
        {
            Dictionary<int, string> usersWithIds = new Dictionary<int, string>();

            foreach (var userLine in File.ReadAllLines(_registeredUsersFileInfo.FullName))
            {
                var array = userLine.Split(' ');
                usersWithIds.Add(Int32.Parse(array[0]), array[1]);
            }

            return usersWithIds;
        }

        /// <summary>
        /// Creates directory named according to provided username (part of registration process)
        /// </summary>
        public static void CreateUserFilesInStorage(int forId)
        {
            _accountStorageDirInfo.CreateSubdirectory($"{forId}");

            var passwordFilePath = Path.Combine(GetUserDir(forId).FullName, "password.txt");
            FileStream fs = File.Create(passwordFilePath);
            fs.Close();

            CreateContactsFile(forId);
        }

        /// <summary>
        /// Writes new line with ID and username of newly registered user to users.txt file in account storage
        /// </summary>
        public static void AddToRegisteredUsersFile(int id, string userName)
        {
            if (string.IsNullOrWhiteSpace(File.ReadAllText(_registeredUsersFileInfo.FullName)))
            {
                File.AppendAllText(_registeredUsersFileInfo.FullName, $"{id} {userName}");
            }
            else
            {
                File.AppendAllText(_registeredUsersFileInfo.FullName, $"\n{id} {userName}");
            }
        }

        /// <summary>
        /// Returns instance of FileInfo for users.txt file that contains strings (userID userName)
        /// </summary>
        private static FileInfo GetRegisteredUsersFileInfo()
        {
            var usersFilePath = Path.Combine(_accountStorageDirInfo.FullName, "users.txt");

            if (!File.Exists(usersFilePath))
            {
                FileStream fs = File.Create(usersFilePath);
                fs.Close();
            }
            return new FileInfo(usersFilePath);
        }

        private static void CreateContactsFile(int forId)
        {
            XDocument saveDoc = new XDocument();
            XElement contactsRoot = new XElement("ChatContactsList");
            saveDoc.Add(contactsRoot);
            saveDoc.Save(Path.Combine(GetUserDir(forId).FullName, "chatContacts.xml"));
        }

        private static void CreateMessagesHistoryFile(int forId, int withId)
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

        public static List<Message> GetMessagesHistory(int forId, int withId)
        {
            List<Message> messagesList = new List<Message>();
            FileInfo messagesHistoryFile = GetUserFiles(forId).FirstOrDefault(x => x.Name == $"messages_{withId}.xml");

            if (messagesHistoryFile != null)
            {
                XElement messagesDoc = XElement.Load(messagesHistoryFile.FullName);
                List<XElement> messagesElements = messagesDoc.Descendants("message").ToList();
                foreach (var messagesElement in messagesElements)
                {
                    messagesList.Add(new Message()
                    {
                        SendTime = DateTime.Parse(messagesElement.FirstAttribute.Value),
                        Sender = new ChatUser { UserName = messagesElement.LastAttribute.Value },
                        MessageText = messagesElement.Value
                    });

                }
            }
            return messagesList;
        }

        public static void AddToMessagesHistoryFile(int forId, int withId, Message message)
        {
            FileInfo messagesHistoryFile = GetUserFiles(forId).FirstOrDefault(x => x.Name == $"messages_{withId}.xml");
            if (messagesHistoryFile == null)
            {
                CreateMessagesHistoryFile(forId, withId);
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
        /// 
        /// </summary>
        public static List<ChatUser> GetContactsListFromFile(int forId)
        {
            List<ChatUser> contactsList = new List<ChatUser>();
            FileInfo contactsFile = GetUserFiles(forId).FirstOrDefault(x => x.Name == "chatContacts.xml");

            if (contactsFile != null)
            {
                XElement contactsDoc = XElement.Load(contactsFile.FullName);
                List<XElement> contactElements = contactsDoc.Descendants("contact").ToList();
                foreach (var contactElement in contactElements)
                {
                    contactsList.Add(new ChatUser
                    {
                        Id = Int32.Parse(contactElement.FirstAttribute.Value),
                        UserName = contactElement.LastAttribute.Value
                    });

                }
            }
            return contactsList;
        }

        /// <summary>
        /// 
        /// </summary>
        public static void AddChatContactToFile(int forId, int contactId, string contactUserName)
        {
            FileInfo contactsFile = GetUserFiles(forId).FirstOrDefault(x => x.Name == "chatContacts.xml");
            if (contactsFile != null)
            {
                CreateMessagesHistoryFile(forId, contactId);
            }

            if(GetContactsListFromFile(forId).FirstOrDefault(u => u.Id == contactId) != null)
                return;

            XElement contactsDoc = XElement.Load(contactsFile.FullName);
            XElement contactElement = new XElement("contact");
            XAttribute idAttribute = new XAttribute("id", contactId);
            XAttribute usernameAttribute = new XAttribute("username", contactUserName);
            contactElement.Add(idAttribute);
            contactElement.Add(usernameAttribute);
            contactsDoc.Add(contactElement);
            contactsDoc.Save(contactsFile.FullName);

        }

        /// <summary>
        /// 
        /// </summary>
        public static void DeleteChatContactFromFile(int forId, string contactUserName)
        {
            if (contactUserName == null)
                return;
            FileInfo contactsFile = GetUserFiles(forId).FirstOrDefault(x => x.Name == "chatContacts.xml");
            if (contactsFile != null)
            {
                XElement contactsDoc = XElement.Load(contactsFile.FullName);
                List<XElement> contactElements = contactsDoc.Descendants("contact").ToList();
                int indexToDel = contactElements.FindIndex(c => c.LastAttribute.Value == contactUserName);
                contactElements.RemoveAt(indexToDel);
                contactsDoc.ReplaceAll(contactElements);
                contactsDoc.Save(contactsFile.FullName);
            }
        }
    }
}
