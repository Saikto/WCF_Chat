﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ChatClient.ChatService {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Message", Namespace="http://schemas.datacontract.org/2004/07/WCF_Chat.Entities")]
    [System.SerializableAttribute()]
    public partial class Message : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string MessageTextField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private ChatClient.ChatService.ChatUser ReceiverField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.DateTime SendTimeField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private ChatClient.ChatService.ChatUser SenderField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string MessageText {
            get {
                return this.MessageTextField;
            }
            set {
                if ((object.ReferenceEquals(this.MessageTextField, value) != true)) {
                    this.MessageTextField = value;
                    this.RaisePropertyChanged("MessageText");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public ChatClient.ChatService.ChatUser Receiver {
            get {
                return this.ReceiverField;
            }
            set {
                if ((object.ReferenceEquals(this.ReceiverField, value) != true)) {
                    this.ReceiverField = value;
                    this.RaisePropertyChanged("Receiver");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.DateTime SendTime {
            get {
                return this.SendTimeField;
            }
            set {
                if ((this.SendTimeField.Equals(value) != true)) {
                    this.SendTimeField = value;
                    this.RaisePropertyChanged("SendTime");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public ChatClient.ChatService.ChatUser Sender {
            get {
                return this.SenderField;
            }
            set {
                if ((object.ReferenceEquals(this.SenderField, value) != true)) {
                    this.SenderField = value;
                    this.RaisePropertyChanged("Sender");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ChatUser", Namespace="http://schemas.datacontract.org/2004/07/WCF_Chat.Entities")]
    [System.SerializableAttribute()]
    public partial class ChatUser : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int IdField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string UserNameField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int Id {
            get {
                return this.IdField;
            }
            set {
                if ((this.IdField.Equals(value) != true)) {
                    this.IdField = value;
                    this.RaisePropertyChanged("Id");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string UserName {
            get {
                return this.UserNameField;
            }
            set {
                if ((object.ReferenceEquals(this.UserNameField, value) != true)) {
                    this.UserNameField = value;
                    this.RaisePropertyChanged("UserName");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ChatService.IChatService", CallbackContract=typeof(ChatClient.ChatService.IChatServiceCallback))]
    public interface IChatService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IChatService/LogIn", ReplyAction="http://tempuri.org/IChatService/LogInResponse")]
        int LogIn(string userName, string password, bool registrationRequired);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IChatService/LogIn", ReplyAction="http://tempuri.org/IChatService/LogInResponse")]
        System.Threading.Tasks.Task<int> LogInAsync(string userName, string password, bool registrationRequired);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IChatService/LogOff", ReplyAction="http://tempuri.org/IChatService/LogOffResponse")]
        void LogOff(int id);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IChatService/LogOff", ReplyAction="http://tempuri.org/IChatService/LogOffResponse")]
        System.Threading.Tasks.Task LogOffAsync(int id);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IChatService/SendMessage")]
        void SendMessage(ChatClient.ChatService.Message message);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IChatService/SendMessage")]
        System.Threading.Tasks.Task SendMessageAsync(ChatClient.ChatService.Message message);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IChatService/AddToChatList", ReplyAction="http://tempuri.org/IChatService/AddToChatListResponse")]
        ChatClient.ChatService.ChatUser AddToChatList(int forId, string userName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IChatService/AddToChatList", ReplyAction="http://tempuri.org/IChatService/AddToChatListResponse")]
        System.Threading.Tasks.Task<ChatClient.ChatService.ChatUser> AddToChatListAsync(int forId, string userName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IChatService/DeleteFromChatList", ReplyAction="http://tempuri.org/IChatService/DeleteFromChatListResponse")]
        int DeleteFromChatList(int forId, string userName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IChatService/DeleteFromChatList", ReplyAction="http://tempuri.org/IChatService/DeleteFromChatListResponse")]
        System.Threading.Tasks.Task<int> DeleteFromChatListAsync(int forId, string userName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IChatService/GetMessagesHistory", ReplyAction="http://tempuri.org/IChatService/GetMessagesHistoryResponse")]
        ChatClient.ChatService.Message[] GetMessagesHistory(int forId, int withId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IChatService/GetMessagesHistory", ReplyAction="http://tempuri.org/IChatService/GetMessagesHistoryResponse")]
        System.Threading.Tasks.Task<ChatClient.ChatService.Message[]> GetMessagesHistoryAsync(int forId, int withId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IChatService/GetChatList", ReplyAction="http://tempuri.org/IChatService/GetChatListResponse")]
        ChatClient.ChatService.ChatUser[] GetChatList(int forId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IChatService/GetChatList", ReplyAction="http://tempuri.org/IChatService/GetChatListResponse")]
        System.Threading.Tasks.Task<ChatClient.ChatService.ChatUser[]> GetChatListAsync(int forId);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IChatServiceCallback {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IChatService/MessageCallback", ReplyAction="http://tempuri.org/IChatService/MessageCallbackResponse")]
        void MessageCallback(ChatClient.ChatService.Message message);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IChatServiceChannel : ChatClient.ChatService.IChatService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ChatServiceClient : System.ServiceModel.DuplexClientBase<ChatClient.ChatService.IChatService>, ChatClient.ChatService.IChatService {
        
        public ChatServiceClient(System.ServiceModel.InstanceContext callbackInstance) : 
                base(callbackInstance) {
        }
        
        public ChatServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName) : 
                base(callbackInstance, endpointConfigurationName) {
        }
        
        public ChatServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public ChatServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public ChatServiceClient(System.ServiceModel.InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, binding, remoteAddress) {
        }
        
        public int LogIn(string userName, string password, bool registrationRequired) {
            return base.Channel.LogIn(userName, password, registrationRequired);
        }
        
        public System.Threading.Tasks.Task<int> LogInAsync(string userName, string password, bool registrationRequired) {
            return base.Channel.LogInAsync(userName, password, registrationRequired);
        }
        
        public void LogOff(int id) {
            base.Channel.LogOff(id);
        }
        
        public System.Threading.Tasks.Task LogOffAsync(int id) {
            return base.Channel.LogOffAsync(id);
        }
        
        public void SendMessage(ChatClient.ChatService.Message message) {
            base.Channel.SendMessage(message);
        }
        
        public System.Threading.Tasks.Task SendMessageAsync(ChatClient.ChatService.Message message) {
            return base.Channel.SendMessageAsync(message);
        }
        
        public ChatClient.ChatService.ChatUser AddToChatList(int forId, string userName) {
            return base.Channel.AddToChatList(forId, userName);
        }
        
        public System.Threading.Tasks.Task<ChatClient.ChatService.ChatUser> AddToChatListAsync(int forId, string userName) {
            return base.Channel.AddToChatListAsync(forId, userName);
        }
        
        public int DeleteFromChatList(int forId, string userName) {
            return base.Channel.DeleteFromChatList(forId, userName);
        }
        
        public System.Threading.Tasks.Task<int> DeleteFromChatListAsync(int forId, string userName) {
            return base.Channel.DeleteFromChatListAsync(forId, userName);
        }
        
        public ChatClient.ChatService.Message[] GetMessagesHistory(int forId, int withId) {
            return base.Channel.GetMessagesHistory(forId, withId);
        }
        
        public System.Threading.Tasks.Task<ChatClient.ChatService.Message[]> GetMessagesHistoryAsync(int forId, int withId) {
            return base.Channel.GetMessagesHistoryAsync(forId, withId);
        }
        
        public ChatClient.ChatService.ChatUser[] GetChatList(int forId) {
            return base.Channel.GetChatList(forId);
        }
        
        public System.Threading.Tasks.Task<ChatClient.ChatService.ChatUser[]> GetChatListAsync(int forId) {
            return base.Channel.GetChatListAsync(forId);
        }
    }
}
