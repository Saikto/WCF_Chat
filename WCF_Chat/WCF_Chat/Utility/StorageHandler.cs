using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WCF_Chat.Utility
{
    public static class StorageHandler
    {
        public static byte[] ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                return bytes;
            }
        }

        /// <summary>
        /// Verifies if user with such user name has own directory in account storage (means registered)
        /// </summary>
        public static bool DoesUserExists(string userName)
        {
            return GetUserDir(userName) != null;
        }

        /// <summary>
        /// Creates directory named according to provided username (part of registration process)
        /// </summary>
        public static void CreateUserDir(string userName)
        {
            var accountStorage = GetAccountsDir();
            accountStorage.CreateSubdirectory($"{userName}");
        }

        /// <summary>
        /// Creates files to store user data (password an contact list) in his directory in account storage 
        /// </summary>
        public static void CreateUserFiles(string userName)
        {
            var passwordFilePath = Path.Combine(GetUserDir(userName).FullName, "password.txt");
            FileStream fs = File.Create(passwordFilePath);
            fs.Close();
        }

        /// <summary>
        /// Gets FileInfo instance that contain information about file with user password
        /// </summary>
        public static FileInfo GetPasswordFile(string userName)
        {
            return GetUserFiles(userName).FirstOrDefault(x => x.Name == "password.txt");
        }

        /// <summary>
        /// Reads list of all registered users (id username) from FileInfo of users.txt file from account storage
        /// </summary>
        public static Dictionary<int, string> GetRegisteredUsers()
        {
            Dictionary<int, string> usersWithIds = new Dictionary<int, string>();

            FileInfo registeredUsersFileInfo = GetRegisteredUsersFile();

            foreach (var userLine in File.ReadAllLines(registeredUsersFileInfo.FullName))
            {
                var array = userLine.Split(' ');
                usersWithIds.Add(Int32.Parse(array[0]), array[1]);
            }

            return usersWithIds;
        }

        /// <summary>
        /// Gets next available user id (reads last used ID from users.txt and returns next int number). First Id == 1
        /// </summary>
        public static int GenerateUserId()
        {
            int previousId;
            if (GetRegisteredUsers().Keys.Count > 0)
            {
                previousId = GetRegisteredUsers().Keys.Last();
                previousId++;
                return previousId;
            }
            return previousId = 1;
        }

        /// <summary>
        /// Writes new line with ID and username of newly registered user to users.txt file in account storage
        /// </summary>
        public static void AddToRegisteredUsersFile(int id, string userName)
        {
            FileInfo registeredUsersFileInfo = GetRegisteredUsersFile();

            if (string.IsNullOrWhiteSpace(File.ReadAllText(registeredUsersFileInfo.FullName)))
            {
                File.AppendAllText(registeredUsersFileInfo.FullName, $"{id} {userName}");
            }
            else
            {
                File.AppendAllText(registeredUsersFileInfo.FullName, $"\n{id} {userName}");
            }
        }

        /// <summary>
        /// Returns instance of DirectoryInfo for "UserAccounts" folder e.g. account storage
        /// </summary>
        public static DirectoryInfo GetAccountsDir()
        {
            return !Directory.Exists("UsersAccounts") ? Directory.CreateDirectory("UsersAccounts") : new DirectoryInfo("UsersAccounts");
        }

        /// <summary>
        /// Returns instance of FileInfo for users.txt file that contains strings (userID userName)
        /// </summary>
        private static FileInfo GetRegisteredUsersFile()
        {
            var accountsDir = GetAccountsDir();
            var usersFilePath = Path.Combine(accountsDir.FullName, "users.txt");

            if (!File.Exists(usersFilePath))
            {
                FileStream fs = File.Create(usersFilePath);
                fs.Close();
            }
            return new FileInfo(usersFilePath);
        }

        /// <summary>
        /// Returns array of FileInfos for files that store in particular user's directory in account storage
        /// </summary>
        private static FileInfo[] GetUserFiles(string userName)
        {
            return GetUserDir(userName).GetFiles();
        }

        /// <summary>
        /// Returns instance of DirectoryInfo for folder of particular user in account storage 
        /// </summary>
        private static DirectoryInfo GetUserDir(string userName)
        {
            var accountStorage = GetAccountsDir();
            var usersDirectoryInfos = accountStorage.GetDirectories();
            return usersDirectoryInfos.FirstOrDefault(d => d.Name == userName);
        }
    }
}
