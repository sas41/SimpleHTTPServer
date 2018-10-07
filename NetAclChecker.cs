using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SimpleHTTPServer
{
    public static class NetAclChecker
    {
        public static void AddAddresses(List<string> addresses)
        {
            AddAddresses(addresses, Environment.UserDomainName, Environment.UserName);
        }

        private static void AddAddresses(List<string> addresses, string domain, string user)
        {
            foreach (string address in addresses)
            {
                string args = string.Format(@"http add urlacl url={0} user={1}\{2}", address, domain, user);

                ProcessStartInfo psi = new ProcessStartInfo("netsh", args);
                psi.Verb = "runas";
                psi.CreateNoWindow = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.UseShellExecute = true;

                Process.Start(psi).WaitForExit();
            }
        }
    }
}
