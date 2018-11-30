using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ChatClient.ChatService;

namespace ChatClient
{
    public partial class MainWindow : Window
    {
        private readonly LoginWindow _loginWindow;
        private readonly ClientService _clientService;
        private ClientUser _selectedChatContact;
        public List<ClientUser> ChatContactsList;
        
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
            if (_selectedChatContact != null && !string.IsNullOrEmpty(TbMessage.Text))
            {
                Message message = new Message
                {
                    Sender = _clientService.CurrentUser,
                    Receiver = _selectedChatContact,
                    SendTime = DateTime.Now,
                    MessageText = TbMessage.Text
                };
                _clientService.SendMessage(message);
                ReloadMessageHistoryListForSelectedContact();
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
            if (_selectedChatContact != null)
            {
                ReloadMessageHistoryListForSelectedContact();
            }
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
            var contactToAdd = TbFindContact.Text;
            if (ChatContactsList.FirstOrDefault(u => u.UserName == contactToAdd) == null)
            {
                try
                {
                    _clientService.AddToChatList(contactToAdd);

                }
                catch (FaultException exception)
                {
                    MessageBox.Show(exception.Message);
                    return;
                }

                //_selectedChatContact = _clientService.ContactsMessageHistory.FirstOrDefault().Key;
                ReloadContactList();
                //ReloadMessageHistoryListForSelectedContact();
            }
            TbMessage.Text = string.Empty;
            HideSearch();
            _showContactSearch = !_showContactSearch;
        }

        private void BtnDeleteContact_Click(object sender, RoutedEventArgs e)
        {
            ClientUser contactToDelete = _selectedChatContact;
            if (contactToDelete != null)
            {
                try
                {
                    _clientService.DeleteFromChatList(contactToDelete);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                    return;
                }
                //TODO
                LbChatMessages.Items.Clear();
                ReloadContactList();
            }
        }

        private void _clientService_MessageReceived(Message message)
        {
            //if (ChatContactsList.FirstOrDefault(u => u.UserName == message.Sender.UserName) == null)
            //{
            //    Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart) delegate() { ReloadContactList(); });
            //}

            if (_selectedChatContact != null && (message.Sender.UserName == _selectedChatContact.UserName))
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart) delegate()
                {
                    string messageString =
                        $"{message.SendTime.ToShortTimeString()} {message.Sender.UserName}:  {message.MessageText}";
                    LbChatMessages.Items.Add(messageString);
                    LbChatMessages.Items.MoveCurrentToLast();
                    LbChatMessages.ScrollIntoView(LbChatMessages.Items.CurrentItem);
                });
            }
        }

        private ClientUser GetSelectedChatUserFromContactsLb()
        {
            var selectedContact = LbContactsList.SelectedItems.Cast<string>().ToList().FirstOrDefault();
            if (selectedContact != null)
            {
                var chatUser = ChatContactsList.Find(u => u.UserName.Equals(selectedContact));
                return chatUser;
            }
            return null;
        }

        private void ReloadContactList()
        {
            LbContactsList.Items.Clear();

            ChatContactsList = _clientService.ContactsMessageHistory.Keys.ToList();

            foreach (var contact in ChatContactsList)
            {
                LbContactsList.Items.Add(contact.UserName);
            }
        }

        private void ReloadMessageHistoryListForSelectedContact()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart) delegate()
            {
                LbChatMessages.Items.Clear();
                List<Message> messageHistory =
                    _clientService.ContactsMessageHistory.FirstOrDefault(u => u.Key.Id == _selectedChatContact.Id)
                        .Value;

                foreach (var message in messageHistory)
                {
                    string messageString =
                        $"{message.SendTime.ToShortTimeString()} {message.Sender.UserName}:  {message.MessageText}";
                    LbChatMessages.Items.Add(messageString);
                }
                LbChatMessages.Items.MoveCurrentToLast();
                LbChatMessages.ScrollIntoView(LbChatMessages.Items.CurrentItem);
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _clientService.DisconnectUser();
            Environment.Exit(0);
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
    }
}
