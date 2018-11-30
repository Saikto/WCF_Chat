using System.ServiceModel;
using WCF_Chat.Entities;

namespace WCF_Chat.Interfaces
{
    public interface IChatServerCallback
    {
        [OperationContract]
        void MessageCallback(Message message);
    }
}
