using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using NAudio;
using NAudio.CoreAudioApi;

namespace MuteInactiveWindows
{
    internal class Monitoring
    {
        MMDevice device;
        Settings settings;

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
        [DllImport("USER32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int length);

        public Monitoring()
        {
            loadSettings();
            InitializeComponent();
            backgroundWorker.RunWorkerAsync();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            string currentTextWindow = null;
            while (true)
            {
                Thread.Sleep(settings.updateInterval);

                IntPtr hwnd = GetForegroundWindow();
                string windowText;
                if (GetWindowText(hwnd, sb, 256) != 0) windowText = sb.ToString();
                else windowText = "";

                //1回前のチェック時とアクティブなウィンドウが変わったら実行
                if (windowText == currentTextWindow) continue;
                currentTextWindow = windowText;

                //デバイスの取得は毎回やり直さないと、セッションが更新されない
                MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
                device = DevEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                SessionCollection sessions = device.AudioSessionManager.Sessions;
                for (int i = 0; i < sessions.Count; i++)
                {
                    AudioSessionControl session = sessions[i];
                    Process process = Process.GetProcessById((int)session.GetProcessID);
                    string processText = process.MainWindowTitle != "" ? process.MainWindowTitle : process.ProcessName;
                    //Debug.WriteLine(processText);
                    if (settings.monitoredApps.Contains(processText))
                    {
                        if (windowText == processText) session.SimpleAudioVolume.Mute = false;
                        else session.SimpleAudioVolume.Mute = true;
                    }
                }
            }
        }

        private System.ComponentModel.BackgroundWorker backgroundWorker;
        private ContextMenuStrip contextMenuStrip;
        private NotifyIcon notifyIcon;
        private void InitializeComponent()
        {
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(backgroundWorker_DoWork);

            contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Items.Add("Settings", null, (s, e) =>
            {
                FormSettings form = new FormSettings(settings);
                DialogResult dialogResult = form.ShowDialog();
                if (dialogResult == DialogResult.OK)
                {
                    settings = form.value;
                    saveSettings();
                }
            });
            contextMenuStrip.Items.Add(new ToolStripSeparator());
            contextMenuStrip.Items.Add("Restart", null, (s, e) =>
            {
                notifyIcon.Dispose();
                contextMenuStrip.Dispose();
                Application.Restart();
            });
            contextMenuStrip.Items.Add("Exit", null, (s, e) =>
            {
                notifyIcon.Dispose();
                contextMenuStrip.Dispose();
                Application.Exit();
            });

            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new Icon("MuteInactiveWindows.ico");
            notifyIcon.ContextMenuStrip = contextMenuStrip;
            notifyIcon.Text = "MuteInactiveWindows";
            notifyIcon.Visible = true;
        }

        private void saveSettings()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            FileStream fs = new FileStream(Directory.GetCurrentDirectory() + "\\" + "settings.xml", FileMode.Create);
            serializer.Serialize(fs, settings);
            fs.Close();
        }

        private void loadSettings()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                FileStream fs = new FileStream(Directory.GetCurrentDirectory() + "\\" + "settings.xml", FileMode.Open);
                settings = (Settings)serializer.Deserialize(fs);
                fs.Close();
            }
            catch
            {
                settings = new Settings();
            }
        }
    }



    public class Settings
    {
        public int updateInterval;
        public string[] monitoredApps;

        public Settings(string[] appNames, int updateInterval)
        {
            this.monitoredApps = appNames;
            this.updateInterval = updateInterval;
        }

        public Settings()
        {
            this.monitoredApps = new string[0];
            this.updateInterval = 1000;
        }
    }
}
