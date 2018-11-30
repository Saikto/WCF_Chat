using System;
using System.Text.RegularExpressions;

namespace ChatClient.Utility
{
    public static class LoginValidator
    {
        public static void ValidateLogin(string userName, string password, bool registrationRequired)
        {
            //Validates if field with username and password are not empty
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("You have to type your user name and password to log in!");
            }

            //Validates correctness of username field input string
            ValidateUserName(userName);

            //Verifies correctness of new user password according to password requirements.
            if (registrationRequired)
            {
                  ValidateRegistrationPassword(password);
            }
        }

        private static void ValidateUserName(string userName)
        {
            Regex exp = new Regex(@"^[a-zA-Z0-9А-яЁё_\.]+$");

            if (userName.Length < 1)
                throw new ArgumentException("User name is too short. It has to contain at least one symbol.");
            if (userName.Length >= 20)
                throw new ArgumentException("User name is too long. It can't contain more than 20 symbols.");
            if (!exp.IsMatch(userName))
                throw new ArgumentException("User name contains restricted symbols. It can contain only numbers, symbols of Russian/English alphabet and symbols '_' and '.'");

        }

        private static void ValidateRegistrationPassword(string password)
        {
            if (password.Length < 8)
                throw new ArgumentException("Password is too short. It has to contain at least 8 symbols.");
            if (password.Length >= 30)
                throw new ArgumentException("Password is too long. It can't contain more than 30 symbols.");
        }
    }
}
