using System;
using System.ServiceModel;
using System.Windows;
using ChatClient.Utility;

namespace ChatClient
{
    public partial class LoginWindow : Window
    {
        private ClientService _clientService;

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var userName = TbUserName.Text;
            var password = PbPassword.Password;
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
            _clientService = new ClientService();
            _clientService.FaultExceptionThrown += FaultExceptionOccured;
            _clientService.ConnectUser(userName, password, registrationRequired);
            _clientService.FaultExceptionThrown -= FaultExceptionOccured;

            if (_clientService.CurrentUser == null)
            {
                return;
            }
            //Creates main window instance and shows it
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
