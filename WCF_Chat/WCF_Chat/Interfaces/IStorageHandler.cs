using System.Collections.Generic;
using WCF_Chat.Entities;

namespace WCF_Chat.Interfaces
{
    interface IStorageHandler
    {
        int GetNextUserId();
        bool UserNameExists(string userName);
        bool ValidatePassword(ClientUser forUser, string password);
        ClientUser GetUserByUserName(string userName);
        List<ClientUser> GetRegisteredUsers();
        List<Message> GetMessagesHistory(ClientUser forUser, ClientUser withUser);
        List<ClientUser> GetContactsList(ClientUser forUser);
        void AddToRegisteredUsers(ClientUser user, string password);
        void AddMessageToHistory(ClientUser forUser, ClientUser withUser, Message message);
        void AddChatContact(ClientUser forUser, ClientUser contactToAdd);
        void DeleteChatContact(ClientUser forUser, ClientUser contactToDelete);
    }
}
