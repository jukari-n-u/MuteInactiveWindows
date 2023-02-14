using System.ComponentModel;

namespace MuteInactiveWindows
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string mutexName = "MuteInactiveWindows";
            System.Threading.Mutex mutex = new System.Threading.Mutex(false, mutexName);

            bool hasHandle = false;
            try
            {
                try
                {
                    hasHandle = mutex.WaitOne(0, false);
                }
                catch (System.Threading.AbandonedMutexException)
                {
                    hasHandle = true;
                }
                if (hasHandle == false)
                {
                    MessageBox.Show("This application already running.");
                    return;
                }


                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();
                new Monitoring();
                Application.Run();
            }
            finally
            {
                if (hasHandle)
                {
                    mutex.ReleaseMutex();
                }
                mutex.Close();
            }

        }
    }
}