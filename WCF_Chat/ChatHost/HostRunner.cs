using System;
using System.ServiceModel;
using WCF_Chat;

namespace ChatHost
{
    class HostRunner
    {
        static void Main()
        {
            using (var host = new ServiceHost(typeof(ChatService)))
            {
                host.Open();
                Console.WriteLine("Host started.");
                Console.ReadLine();
            }
        }
    }
}
