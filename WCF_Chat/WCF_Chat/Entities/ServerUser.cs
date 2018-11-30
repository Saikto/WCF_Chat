using System.ServiceModel;

namespace WCF_Chat.Entities
{
    public class ServerUser: ClientUser
    {
        public OperationContext OperationContext { get; set; }
    }
}
