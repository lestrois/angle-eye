using System;
using System.Windows.Forms;
using org.richardqiao.angleeye.screener;
using System.IO;
using System.Drawing.Imaging;
using System.Threading;

namespace ScreenAgent
{
    public class TaskTrayApplicationContext : ApplicationContext
    {
        NotifyIcon notifyIcon = new NotifyIcon();
        Configuration configWindow = new Configuration();
        MenuItem startMenuItem;
        volatile bool flag = true;
        Thread thr;
        public TaskTrayApplicationContext()
        {
            startMenuItem = new MenuItem("Stop", new EventHandler(StartProcess));
            MenuItem configMenuItem = new MenuItem("Configuration", new EventHandler(ShowConfig));
            MenuItem exitMenuItem = new MenuItem("Exit", new EventHandler(Exit));

            notifyIcon.Icon = ScreenAgent.Properties.Resources.AppIcon;
            notifyIcon.DoubleClick += new EventHandler(ShowMessage);
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[] { startMenuItem, configMenuItem, exitMenuItem });
            notifyIcon.Visible = true;
            thr = new Thread(ScreenCap);
            thr.Start();
        }

        void ShowMessage(object sender, EventArgs e)
        {
            // Only show the message if the settings say we can.
            if (ScreenAgent.Properties.Settings.Default.ShowMessage)
                MessageBox.Show("Hello World");
        }

        void StartProcess(object sender, EventArgs e)
        {
            startMenuItem.Enabled = false;
            if (startMenuItem.Text == "Start")
            {
                startMenuItem.Text = "Stop";
                flag = true;
            }
            else
            {
                startMenuItem.Text = "Start";
                flag = false;
            }
            startMenuItem.Enabled = true;
        }

        void ShowConfig(object sender, EventArgs e)
        {
            // If we are already showing the window meerly focus it.
            if (configWindow.Visible)
                configWindow.Focus();
            else
                configWindow.ShowDialog();
        }

        void Exit(object sender, EventArgs e)
        {
            // We must manually tidy up and remove the icon before we exit.
            // Otherwise it will be left behind until the user mouses over.
            notifyIcon.Visible = false;
            flag = false;
            thr.Abort();
            Application.Exit();
        }

        void ScreenCap()
        {
            while (true)
            {
                if (flag)
                {
                    DateTime dt = DateTime.Now;
                    string month = dt.Month.ToString();
                    if (month.Length == 1) month = "0" + month;
                    string day = dt.Day.ToString();
                    if (day.Length == 1) day = "0" + day;
                    string date = dt.Year.ToString() + month + day;
                    string hour = dt.Hour.ToString();
                    if (hour.Length == 1) hour = "0" + hour;
                    string min = dt.Minute.ToString();
                    if (min.Length == 1) min = "0" + min;
                    string folder = Path.Combine(Environment.CurrentDirectory, date);
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    using (var screen = ScreenshotCapture.TakeScreenshot())
                    {
                        screen.Save(folder + "/sc-" + hour + min + ".jpg", ImageFormat.Jpeg);
                    }
                }
                Thread.Sleep(60000);
            }
        }
    }
}
