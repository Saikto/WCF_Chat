﻿using System.Collections.Generic;
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

        [OperationContract(IsOneWay = true)]
        void SendMessage(Message message);

        [OperationContract]
        ChatUser AddToChatList(int forId, string userName);

        [OperationContract]
        int DeleteFromChatList(int forId, string userName);

        [OperationContract]
        List<Message> GetMessagesHistory(int forId, int withId);

        [OperationContract]
        List<ChatUser> GetChatList(int forId);
    }
}