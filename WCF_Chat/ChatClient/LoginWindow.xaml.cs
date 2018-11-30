using System;
using System.Windows;
using ChatClient.Exceptions;

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
            try
            {
                _clientService.ConnectUser(userName, password, registrationRequired);
                //StorageHandler.GetOrCreateUserDir(_clientService.CurrentUser.Id);
            }
            catch (WrongUserPasswordException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            catch (UserNotRegisteredException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            catch (UserAlreadyExistException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            MainWindow mainWindow = new MainWindow(_clientService, this);
            mainWindow.Show();
            Hide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
