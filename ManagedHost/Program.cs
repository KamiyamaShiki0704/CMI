using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CMI.Host
{
    internal static class Program
    {
        private static int exiting;

        [STAThread]
        private static void Main()
        {
            ConfigureUnhandledExceptionHandlers();

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
                LogAndExit("CMI Host startup failed", ex);
            }
        }

        private static void ConfigureUnhandledExceptionHandlers()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (sender, args) => LogAndExit("CMI Host UI thread failed", args.Exception);
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                LogAndExit("CMI Host app domain failed", args.ExceptionObject as Exception);
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                args.SetObserved();
                LogAndExit("CMI Host task failed", args.Exception);
            };
        }

        private static void LogAndExit(string source, Exception ex)
        {
            if (Interlocked.Exchange(ref exiting, 1) == 0)
            {
                try
                {
                    string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cmi_host_error.log");
                    File.AppendAllText(logPath,
                        $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {source}{Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}");
                }
                catch
                {
                    // Last-resort handler; avoid surfacing WinForms/.NET dialogs from the packaged host.
                }
            }
            Environment.Exit(0);
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
