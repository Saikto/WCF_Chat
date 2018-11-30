using System;
using ChatClient.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChatUnitTests
{
    [TestClass]
    public class LoginValidatorTests
    {
        private string _exMessageEmptyInput = "You have to type your user name and password to log in!";
        private string _exMessageShortUsername = "User name is too short. It has to contain at least two symbols.";
        private string _exMessageLongUsername = "User name is too long. It can't contain more than 20 symbols.";
        private string _exMessageRestrictedSymbol = "User name contains restricted symbols. It can contain only numbers, symbols of Russian/English alphabet and symbols '_' and '.'";
        private string _exMessageShortPassword = "Password is too short. It has to contain at least 8 symbols.";
        private string _exMessageLongPassword = "Password is too long. It can't contain more than 30 symbols.";

        [TestMethod]
        public void EmptyUsernameStr()
        {
            string userName = "";
            string password = "somePassword";

            try
            {
                LoginValidator.ValidateLogin(userName, password, false);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(_exMessageEmptyInput, e.Message);
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public void EmptyPasswordStr()
        {
            string userName = "someUsername";
            string password = "";

            try
            {
                LoginValidator.ValidateLogin(userName, password, false);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(_exMessageEmptyInput, e.Message);
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public void ShortUsernameStr()
        {
            string userName = "U";
            string password = "somePassword";

            try
            {
                LoginValidator.ValidateLogin(userName, password, false);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(_exMessageShortUsername, e.Message);
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public void LongUsernameStr()
        {
            string userName = "012345678901234567890";
            string password = "somePassword";

            try
            {
                LoginValidator.ValidateLogin(userName, password, false);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(_exMessageLongUsername, e.Message);
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public void RestrictedUsernameStr()
        {
            string[] badUserNames =
            {
                "someUsername`",
                "someUsername~",
                "someUsername!",
                "someUsername@",
                "someUsername#",
                "someUsername$",
                "someUsername%",
                "someUsername^",
                "someUsername|",
                "someUsername&",
                "someUsername*",
                "someUsername)",
                "someUsername-",
                "someUsername+",
                "someUsername[",
                "someUsername]",
                "someUsername\"",
                "someUsername/",
                "someUsername№",
                "someUsername;",
                "someUsername:",
                "someUsername?",
                "someUsername=",
                "someUsername<",
                "someUsername>",
                "someUsername,",
                "someUsername'",
                "someUsername{",
                "someUsername}",
                "someUsername ",
            };

            string password = "somePassword";

            foreach (var username in badUserNames)
            {
                string actualExMessage = null;
                try
                {
                    LoginValidator.ValidateLogin(username, password, false);
                }
                catch (ArgumentException e)
                {
                    actualExMessage = e.Message;
                    
                }

                if (actualExMessage == null)
                {
                    Assert.Fail();
                }
                Assert.AreEqual(_exMessageRestrictedSymbol, actualExMessage);
            }
        }

        [TestMethod]
        public void ValidUsernameSymbols()
        {
            string[] goodUserNames =
            {
                "someUsername.",
                "someUsername_",
            };

            string password = "somePassword";

            foreach (var username in goodUserNames)
            {
                LoginValidator.ValidateLogin(username, password, false);
            }

        }

        [TestMethod]
        public void ShortPasswordStr()
        {
            string userName = "someUsername";
            string password = "1234567";

            try
            {
                LoginValidator.ValidateLogin(userName, password, true);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(_exMessageShortPassword, e.Message);
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public void LongPasswordStr()
        {
            string userName = "someUsername";
            string password = "0123456789012345678901234567890";

            try
            {
                LoginValidator.ValidateLogin(userName, password, true);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(_exMessageLongPassword, e.Message);
                return;
            }
            Assert.Fail();
        }
    }
}
