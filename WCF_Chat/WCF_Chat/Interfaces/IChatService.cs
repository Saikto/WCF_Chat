using System.Collections.Generic;
using System.ServiceModel;
using WCF_Chat.Entities;

namespace WCF_Chat.Interfaces
{
    [ServiceContract(CallbackContract = typeof(IChatServerCallback))]
    public interface IChatService
    {
        [OperationContract]
        ClientUser LogIn(string userName, string password, bool registrationRequired);

        [OperationContract]
        void LogOff(int id);

        [OperationContract(IsOneWay = true)]
        void SendMessage(Message message);

        [OperationContract]
        ClientUser AddToChatList(ClientUser forUser, string contactUserName);

        [OperationContract]
        int DeleteFromChatList(ClientUser forUser, string contactUserName);

        [OperationContract]
        List<Message> GetMessagesHistory(ClientUser forUser, ClientUser withUser);

        [OperationContract]
        List<ClientUser> GetChatList(ClientUser forUser);
    }
}
