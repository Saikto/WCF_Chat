using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ChatClient.ChatService;
using ChatClient.Utility;

namespace ChatClient
{
    public partial class MainWindow : Window
    {
        private readonly LoginWindow _loginWindow;
        private readonly ClientService _clientService;
        private ChatUser _selectedChatContact;
        private List<Message> _selectedChatContactMessageHistory;
        public List<ChatUser> ChatContactsList;
        
        private bool _showContactSearch = false;

        public MainWindow(ClientService service, LoginWindow loginWindow)
        {
            _loginWindow = loginWindow;
            _clientService = service;
            _clientService.MessageReceived += _clientService_MessageReceived;

            InitializeComponent();

            HideSearch();
            ReloadContactList();
        }

        private void _clientService_MessageReceived(Message message)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart) delegate()
            {
                if (ChatContactsList.Find(u => u.Id == message.Sender.Id) == null)
                {
                    StorageHandler.AddChatContactToFile(_clientService.CurrentUser.Id, message.Sender.Id,
                        message.Sender.UserName);
                    StorageHandler.CreateMessagesHistoryFile(_clientService.CurrentUser.Id, message.Sender.Id);
                    ReloadContactList();
                }
                StorageHandler.AddToMessagesHistoryFile(_clientService.CurrentUser.Id, message.Sender.Id, message);
                if (_selectedChatContact != null && (message.Sender.Id == _selectedChatContact.Id))
                {
                    LoadMessageHistoryList(_selectedChatContact);
                }
            });
        }

        private void TbMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.IsKeyDown(Key.LeftShift))
            {
                TbMessage.Text = TbMessage.Text.Insert(TbMessage.Text.Length, Environment.NewLine);
                TbMessage.CaretIndex = TbMessage.Text.Length;
            }
            else if (e.Key == Key.Enter)
            {
                BtnSendMessage_Click(sender, e);
            }
        }

        private void BtnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedChatContact != null)
            {
                Message message = new Message
                {
                    Sender = _clientService.CurrentUser,
                    Receiver = _selectedChatContact,
                    SendTime = DateTime.Now,
                    MessageText = TbMessage.Text
                };
                _clientService.SendMessage(message);
                StorageHandler.AddToMessagesHistoryFile(_clientService.CurrentUser.Id, message.Receiver.Id, message);
                LoadMessageHistoryList(_selectedChatContact);
                TbMessage.Text = string.Empty;
            }
        }

        private void BtnLogOff_Click(object sender, RoutedEventArgs e)
        {
            _clientService.DisconnectUser();

            _loginWindow.TbPassword.Text = string.Empty;
            _loginWindow.CheckBoxRegistration.IsChecked = false;
            _loginWindow.Show();
            Hide();
        }

        private void LbContactsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _selectedChatContact = GetSelectedChatUserFromContactsLb();
            LoadMessageHistoryList(_selectedChatContact);
        }

        private void BtnAddNew_Click(object sender, RoutedEventArgs e)
        {
            if (!_showContactSearch)
            {
                ShowSearch();
            }

            if (_showContactSearch)
            {
                HideSearch();
            }
            _showContactSearch = !_showContactSearch;
        }

        private void BtnTryAdd_Click(object sender, RoutedEventArgs e)
        {
            var contactToAddName = TbFindContact.Text;
            if (ChatContactsList.FirstOrDefault(u => u.UserName == contactToAddName) == null)
            {
                var contact = _clientService.FindContact(contactToAddName);
                if (contact == null)
                {
                    MessageBox.Show("User with such username not found. Please try again.");
                    return;
                }
                else
                {
                    StorageHandler.AddChatContactToFile(_clientService.CurrentUser.Id, contact.Id, contact.UserName);
                    StorageHandler.CreateMessagesHistoryFile(_clientService.CurrentUser.Id, contact.Id);
                    _selectedChatContact = new ChatUser()
                    {
                        Id = contact.Id,
                        UserName = contact.UserName
                    };
                    ReloadContactList();
                    LoadMessageHistoryList(_selectedChatContact);
                }
            }
            TbMessage.Text = string.Empty;
            HideSearch();
            _showContactSearch = !_showContactSearch;
        }

        private void BtnDeleteContact_Click(object sender, RoutedEventArgs e)
        {
            ChatUser contactToDelete = _selectedChatContact;
            if (contactToDelete != null)
            {
                StorageHandler.DeleteChatContactFromFile(_clientService.CurrentUser.Id, contactToDelete.UserName);
                ReloadContactList();
            }
        }

        private ChatUser GetSelectedChatUserFromContactsLb()
        {
            var selectedContact = LbContactsList.SelectedItems.Cast<string>().ToList().FirstOrDefault();
            if (selectedContact != null)
            {
                var chatUser = ChatContactsList.Find(u => u.UserName.Equals(selectedContact));
                return chatUser;
            }
            return null;
        }

        private void HideSearch()
        {
            BtnAddNew.Content = "Add new";
            LblFindContact.Visibility = Visibility.Hidden;
            TbFindContact.Visibility = Visibility.Hidden;
            BtnTryAdd.Visibility = Visibility.Hidden;
        }

        private void ShowSearch()
        {
            BtnAddNew.Content = "Cancel";
            TbFindContact.Text = string.Empty;
            LblFindContact.Visibility = Visibility.Visible;
            TbFindContact.Visibility = Visibility.Visible;
            BtnTryAdd.Visibility = Visibility.Visible;
        }

        private void ReloadContactList()
        {
            LbContactsList.Items.Clear();

            ChatContactsList = StorageHandler.GetContactsListFromFilesStorage(_clientService.CurrentUser.Id);

            foreach (var contact in ChatContactsList)
            {
                LbContactsList.Items.Add(contact.UserName);
            }
        }

        private void LoadMessageHistoryList(ChatUser contact)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart) delegate()
            {
                LbChatMessages.Items.Clear();
                if (_selectedChatContact != null)
                {
                    _selectedChatContactMessageHistory =
                        StorageHandler.GetMessagesHistoryFile(_clientService.CurrentUser.Id, contact.Id);

                    foreach (var message in _selectedChatContactMessageHistory)
                    {
                        string messageString = $"{message.SendTime.ToShortTimeString()} {message.Sender.UserName}:  {message.MessageText}";
                        LbChatMessages.Items.Add(messageString);
                    }
                }
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _clientService.DisconnectUser();
            Environment.Exit(0);
        }
    }
}
