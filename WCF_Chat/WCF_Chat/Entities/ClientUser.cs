using System.Runtime.Serialization;

namespace WCF_Chat.Entities
{
    [DataContract]
    public class ClientUser
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string UserName { get; set; }

        public ClientUser()
        {
        }

        public ClientUser(int id, string userName)
        {
            Id = id;
            UserName = userName;
        }

        public bool Equals(ClientUser other)
        {
            return Id == other.Id && string.Equals(UserName, other.UserName);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id * 397) ^ (UserName != null ? UserName.GetHashCode() : 0);
            }
        }
    }
}
