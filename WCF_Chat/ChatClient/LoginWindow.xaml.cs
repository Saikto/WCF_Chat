using System;
using System.ServiceModel;
using System.Windows;

namespace ChatClient
{
    public partial class LoginWindow : Window
    {
        private readonly ClientService _clientService = new ClientService();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var userName = TbUserName.Text;
            var password = TbPassword.Text;
            var registrationRequired = CheckBoxRegistration.IsChecked == true;

            //Validate correctness of login credentials
            try
            {
                LoginValidator.ValidateLogin(userName, password, registrationRequired);
            }
            catch (ArgumentException exception)
            {
                MessageBox.Show(exception.Message);
                return;
            }

            //Connects user if everything is OK
            _clientService.FaultExceptionThrown += FaultExceptionOccured;
            _clientService.ConnectUser(userName, password, registrationRequired);

            if (_clientService.CurrentUser == null)
            {
                _clientService.FaultExceptionThrown -= FaultExceptionOccured;
                return;
            }
            //StorageHandler.GetOrCreateUserDir(_clientService.CurrentUser.Id);

            MainWindow mainWindow = new MainWindow(_clientService, this);
            mainWindow.Title += $" - {userName}";
            mainWindow.Show();
            Hide();
        }

        private void FaultExceptionOccured(FaultException ex)
        {
            MessageBox.Show(ex.Message);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
