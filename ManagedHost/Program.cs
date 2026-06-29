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
            try
            {
                using (Mutex mutex = new Mutex(true, BuildMutexName(), out bool createdNew))
                {
                    if (!createdNew) return;

                    global::CMI.CMI.Main();
                }
            }
            catch (UnauthorizedAccessException)
            {
                // A previous elevated/session-mismatched host can leave a named mutex
                // that this process cannot open. Treat it as "already running".
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString(),
                    "CMI Host",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static string BuildMutexName()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory ?? string.Empty;
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(baseDirectory.ToLowerInvariant()));
                return "Local\\CMI_Host_" + BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }
    }
}
