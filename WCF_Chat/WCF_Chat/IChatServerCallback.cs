using System.Collections.Generic;
using System.ServiceModel;
using WCF_Chat.Entities;

namespace WCF_Chat
{
    public interface IChatServerCallback
    {
        [OperationContract]
        void MessageCallback(Message message);
    }
}
