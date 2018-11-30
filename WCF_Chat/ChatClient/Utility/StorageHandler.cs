using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using ChatClient.ChatService;

namespace ChatClient.Utility
{
    public static class StorageHandler
    {
        /// <summary>
        ///
        /// </summary>
        public static DirectoryInfo GetOrCreateAccountsDir()
        {
            return !Directory.Exists("UsersAccounts") ? Directory.CreateDirectory("UsersAccounts") : new DirectoryInfo("UsersAccounts");
        }

        /// <summary>
        ///
        /// </summary>
        public static DirectoryInfo GetOrCreateUserDir(int id)
        {
            var accountStorage = GetOrCreateAccountsDir();
            var path = Path.Combine(accountStorage.FullName, id.ToString());

            if (!Directory.Exists(path))
            {
                var directoryInfo = Directory.CreateDirectory(path);
                CreateContactsFile(id);
                return directoryInfo;
            }
            
            return new DirectoryInfo(path);
        }

        /// <summary>
        /// 
        /// </summary>
        private static FileInfo[] GetUserFiles(int id)
        {
            return GetOrCreateUserDir(id).GetFiles();
        }

        private static void CreateContactsFile(int id)
        {
            XDocument saveDoc = new XDocument();
            XElement contactsRoot = new XElement("ChatContactsList");
            saveDoc.Add(contactsRoot);
            saveDoc.Save(Path.Combine(GetOrCreateUserDir(id).FullName, "chatContacts.xml"));
        }

        public static void CreateMessagesHistoryFile(int forId, int withId)
        {
            XDocument saveDoc = new XDocument();
            XElement messagesRoot = new XElement("MessagesList");
            saveDoc.Add(messagesRoot);
            saveDoc.Save(Path.Combine(GetOrCreateUserDir(forId).FullName, $"messages_{withId}.xml"));
        }

        public static List<Message> GetMessagesHistoryFile(int forId, int withId)
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
                        Sender = new ChatUser{ UserName = messagesElement.LastAttribute.Value},
                        MessageText = messagesElement.Value
                });

                }
            }
            return messagesList;
        }

        public static void AddToMessagesHistoryFile(int forId, int withId, Message message)
        {
            FileInfo messagesHistoryFile = GetUserFiles(forId).FirstOrDefault(x => x.Name == $"messages_{withId}.xml");

            if (messagesHistoryFile != null)
            {
                XElement messagesDoc = XElement.Load(messagesHistoryFile.FullName);
                XElement messageElement = new XElement("message") {Value = message.MessageText};
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
        public static List<ChatUser> GetContactsListFromFilesStorage(int id)
        {
            List<ChatUser> contactsList = new List<ChatUser>();
            FileInfo contactsFile = GetUserFiles(id).FirstOrDefault(x => x.Name == "chatContacts.xml");

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
        public static void AddChatContactToFile(int id, int contactId, string contactUserName)
        {
            FileInfo contactsFile = GetUserFiles(id).FirstOrDefault(x => x.Name == "chatContacts.xml");
            if (contactsFile != null)
            {
                XElement contactsDoc = XElement.Load(contactsFile.FullName);
                XElement contactElement = new XElement("contact");
                XAttribute idAttribute = new XAttribute("id", contactId);
                XAttribute usernameAttribute = new XAttribute("username", contactUserName);
                contactElement.Add(idAttribute);
                contactElement.Add(usernameAttribute);
                contactsDoc.Add(contactElement);
                contactsDoc.Save(contactsFile.FullName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void DeleteChatContactFromFile(int id, string contactUserName)
        {
            if (contactUserName == null)
                return;
            FileInfo contactsFile = GetUserFiles(id).FirstOrDefault(x => x.Name == "chatContacts.xml");
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
