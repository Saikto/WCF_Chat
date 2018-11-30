using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using ChatClient.ChatService;
using ChatClient.Exceptions;
using ChatClient.Utility;

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
            bool registrationRequired;

            //Checks registration required checkbox
            if (CheckBoxRegistration.IsChecked == true)
            {
                registrationRequired = true;
            }
            else
            {
                registrationRequired = false;
            }

            //Validates if field with username and password are not empty
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("You have to type your user name and password to log in!");
                return;
            }

            //Validates correctness of username field input string
            try
            {
                ValidateUserName(userName);
            }
            catch (ArgumentException exception)
            {
                MessageBox.Show(exception.Message);
                return;
            }

            //Verifies correctness of new user password according to password requirements.
            if (registrationRequired)
            {
                try
                {
                    ValidateRegistrationPassword(password);
                }
                catch (ArgumentException exception)
                {
                    MessageBox.Show(exception.Message);
                    return;
                }
            }

            //Connects user if everything is OK
            try
            {
                int clientId = _clientService.ConnectUser(userName, password, registrationRequired);
                _clientService.CurrentUser = new ChatUser{Id = clientId, UserName = userName};
                StorageHandler.GetOrCreateUserDir(_clientService.CurrentUser.Id);
                MainWindow mainWindow = new MainWindow(_clientService, this);
                mainWindow.Show();
                Hide();
            }
            catch (WrongUserPasswordException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (UserNotRegisteredException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (UserAlreadyExistException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ValidateUserName(string userName)
        {
            Regex exp = new Regex(@"^[a-zA-Z0-9А-яЁё_\.]+$");

            if (userName.Length < 1)
                throw new ArgumentException("User name is too short. It has to contain at least one symbol.");
            if(userName.Length >= 20)
                throw new ArgumentException("User name is too long. It can't contain more than 20 symbols.");
            if (!exp.IsMatch(userName))
                throw new ArgumentException("User name contains restricted symbols. It can contain only numbers, symbols of Russian/English alphabet and symbols '_' and '.'");

        }

        private void ValidateRegistrationPassword(string password)
        {
            if (password.Length < 8)
                throw new ArgumentException("Password is too short. It has to contain at least 8 symbols.");
            if (password.Length >= 30)
                throw new ArgumentException("Password is too long. It can't contain more than 30 symbols.");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
