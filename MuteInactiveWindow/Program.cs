using System.ComponentModel;

namespace MuteInactiveWindow
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Mutex名を決める（必ずアプリケーション固有の文字列に変更すること！）
            string mutexName = "MuteInactiveWindow";
            //Mutexオブジェクトを作成する
            System.Threading.Mutex mutex = new System.Threading.Mutex(false, mutexName);

            bool hasHandle = false;
            try
            {
                try
                {
                    //ミューテックスの所有権を要求する
                    hasHandle = mutex.WaitOne(0, false);
                }
                //.NET Framework 2.0以降の場合
                catch (System.Threading.AbandonedMutexException)
                {
                    //別のアプリケーションがミューテックスを解放しないで終了した時
                    hasHandle = true;
                }
                //ミューテックスを得られたか調べる
                if (hasHandle == false)
                {
                    //得られなかった場合は、すでに起動していると判断して終了
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
                    //ミューテックスを解放する
                    mutex.ReleaseMutex();
                }
                mutex.Close();
            }

        }
    }
}