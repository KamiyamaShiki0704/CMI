using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CMI.Host
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            using (new Mutex(true, BuildMutexName(), out bool createdNew))
            {
                if (!createdNew) return;

                try
                {
                    global::CMI.CMI.Main();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        ex.ToString(),
                        "CMI Nightreign Host",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private static string BuildMutexName()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory ?? string.Empty;
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(baseDirectory.ToLowerInvariant()));
                return "Global\\CMI_Host_" + BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }
    }
}
