﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using NAudio;
using NAudio.CoreAudioApi;

namespace MuteInactiveWindow
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
            MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
            device = DevEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            StringBuilder sb = new StringBuilder();
            //string currentTextWindow = null;
            while (true)
            {
                IntPtr hwnd = GetForegroundWindow();
                GetWindowText(hwnd, sb, 256);
                string windowText = sb.ToString();

                /*
                if (windowText == currentTextWindow) continue;
                //1回前のチェック時とアクティブなウィンドウが変わったら実行
                currentTextWindow = windowText;
                */

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
                        //monitoredApps.Add(processText, session);
                    }
                }

                Thread.Sleep(settings.updateInterval);
            }
        }

        private System.ComponentModel.BackgroundWorker backgroundWorker;
        private ContextMenuStrip contextMenuStrip;
        private NotifyIcon notifyIcon;
        private void InitializeComponent()
        {
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(backgroundWorker_DoWork);

            //コンテキストメニューを作成
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
            contextMenuStrip.Items.Add("Exit", null, (s, e) =>
            {
                notifyIcon.Dispose();
                contextMenuStrip.Dispose();
                Application.Exit();
            });

            // 常駐アプリ（タスクトレイのアイコン）を作成
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new Icon("MuteInactiveWindow.ico");
            notifyIcon.ContextMenuStrip = contextMenuStrip;
            notifyIcon.Text = "MuteInactiveWindows";
            notifyIcon.Visible = true;
        }

        private void saveSettings()
        {
            // XmlSerializerを使ってファイルに保存（TwitSettingオブジェクトの内容を書き込む）
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));

            // カレントディレクトリに"settings.xml"というファイルで書き出す
            FileStream fs = new FileStream(Directory.GetCurrentDirectory() + "\\" + "settings.xml", FileMode.Create);

            // オブジェクトをシリアル化してXMLファイルに書き込む
            serializer.Serialize(fs, settings);
            fs.Close();
        }

        private void loadSettings()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));

                // XMLをTwitSettingsオブジェクトに読み込む
                FileStream fs = new FileStream(Directory.GetCurrentDirectory() + "\\" + "settings.xml", FileMode.Open);

                // XMLファイルを読み込み、逆シリアル化（復元）する
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
            this.updateInterval = 100;
        }
    }
}
