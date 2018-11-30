using System.ServiceModel;
using WCF_Chat.Entities;

namespace WCF_Chat
{
    [ServiceContract(CallbackContract = typeof(IChatServerCallback))]
    public interface IChatService
    {
        [OperationContract]
        int LogIn(string userName, string password, bool registrationRequired);

        [OperationContract]
        void LogOff(int id);

        [OperationContract]
        ChatUser FindUserByName(string userName);

        [OperationContract(IsOneWay = true)]
        void SendMessage(Message message);
    }
}
