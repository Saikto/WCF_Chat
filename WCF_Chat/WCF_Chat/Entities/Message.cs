using System;
using System.Runtime.Serialization;

namespace WCF_Chat.Entities
{
    [DataContract]
    public class Message
    {
        [DataMember]
        public ClientUser Sender { get; set; }

        [DataMember]
        public ClientUser Receiver { get; set; }

        [DataMember]
        public DateTime SendTime { get; set; }

        [DataMember]
        public string MessageText { get; set; }
    }
}
