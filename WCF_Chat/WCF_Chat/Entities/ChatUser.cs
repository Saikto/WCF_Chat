using System.Runtime.Serialization;
using System.ServiceModel;

namespace WCF_Chat.Entities
{
    [DataContract]
    public class ChatUser
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string UserName { get; set; }

        public OperationContext OperationContext { get; set; }
    }
}
