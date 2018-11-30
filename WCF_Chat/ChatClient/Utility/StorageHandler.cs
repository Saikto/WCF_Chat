using System.IO;

namespace ChatClient.Utility
{
    public static class StorageHandler
    {
        /// <summary>
        ///
        /// </summary>
        public static DirectoryInfo GetOrCreateAccountsDir()
        {
            return !Directory.Exists("UsersAccounts") ? Directory.CreateDirectory("UsersAccounts") : new DirectoryInfo("UsersAccounts");
        }

        /// <summary>
        ///
        /// </summary>
        public static DirectoryInfo GetOrCreateUserDir(int id)
        {
            var accountStorage = GetOrCreateAccountsDir();
            var path = Path.Combine(accountStorage.FullName, id.ToString());

            if (!Directory.Exists(path))
            {
                var directoryInfo = Directory.CreateDirectory(path);
                return directoryInfo;
            }
            
            return new DirectoryInfo(path);
        }

        /// <summary>
        /// 
        /// </summary>
        private static FileInfo[] GetUserFiles(int id)
        {
            return GetOrCreateUserDir(id).GetFiles();
        }
    }
}
