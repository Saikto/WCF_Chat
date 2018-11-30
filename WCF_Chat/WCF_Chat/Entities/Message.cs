using System;
using System.Runtime.Serialization;

namespace WCF_Chat.Entities
{
    [DataContract]
    public class Message
    {
        [DataMember]
        public ChatUser Sender { get; set; }

        [DataMember]
        public ChatUser Receiver { get; set; }

        [DataMember]
        public DateTime SendTime { get; set; }

        [DataMember]
        public string MessageText { get; set; }
    }
}
