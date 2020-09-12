using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Timers;

namespace WindowWaker
{
    public partial class Form1 : Form
    {

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
            // Legacy flag, should not be used.
            // ES_USER_PRESENT = 0x00000004
        }

        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hWnd);

        private const int SW_RESTORE = 9;
        private System.Timers.Timer myTimer;

        [DllImport("user32.dll")]
        private static extern IntPtr ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        public Form1()
        {
            InitializeComponent();
        }

        private void ButtonGo_Click(object sender, EventArgs e)
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
            toolStripStatusLabel1.Text = "App Started: " + (DateTime.Now).ToString("h:mm:ss tt");

            // Create a timer
            myTimer = new System.Timers.Timer();

            if (checkBox1.Checked)
            {
                // Tell the timer what to do when it elapses
                myTimer.Elapsed += new ElapsedEventHandler(myEvent);
                // Set it to go off every ten seconds
                myTimer.Interval = 10000;
                // And start it        
                myTimer.Enabled = true;
            }
        }

        private void ButtonStop_Click(object sender, EventArgs e)
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
            myTimer.Enabled = false;
            toolStripStatusLabel1.Text = "App Stopped." + (DateTime.Now).ToString("h:mm:ss tt");
        }

        private void CaptureScreen(string procName)
        {
            Process proc;

            try
            {
                proc = Process.GetProcessesByName(procName)[0];

                // You need to focus on the application
                SetForegroundWindow(proc.MainWindowHandle);
                ShowWindow(proc.MainWindowHandle, SW_RESTORE);

                // You need some amount of delay, but 1 second may be overkill
                System.Threading.Thread.Sleep(1000);

                Rect rect = new Rect();
                IntPtr error = GetWindowRect(proc.MainWindowHandle, ref rect);

                // sometimes it gives error.
                while (error == (IntPtr)0)
                {
                    error = GetWindowRect(proc.MainWindowHandle, ref rect);
                }

                int width = rect.right - rect.left;
                int height = rect.bottom - rect.top;

                Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                Graphics.FromImage(bmp).CopyFromScreen(rect.left,
                                                       rect.top,
                                                       0,
                                                       0,
                                                       new Size(width, height),
                                                       CopyPixelOperation.SourceCopy);


                bmp.Save("c:\\tmp\\ScreenCapture_" + (DateTime.Now).ToString("mmddyy_hhmmsstt") + ".png", ImageFormat.Png);
                //bmp.Save("c:\\tmp\\ScreenCapture" + ".png", ImageFormat.Png);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
            }
            
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            CaptureScreen(textBox1.Text);
        }

        // Implement a call with the right signature for events going off
        private void myEvent(object source, ElapsedEventArgs e) {

            if (checkBox1.Checked)
            {
                CaptureScreen(textBox1.Text);
            }

        }
    }
}
