using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace wf_run_exe
{
    public partial class Form1 : Form
    {

           private const int GWL_STYLE = (-16);
        private const int WS_VISIBLE = 0x10000000;
        EventHandler appIdleEvent = null;
        Action<object, EventArgs> appIdleAction = null;
        Process m_AppProcess;


        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowLongPtr32(HandleRef hWnd, int nIndex, int dwNewLong);


        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, int dwNewLong);


        [DllImport("user32.dll", SetLastError = true)]
        private static extern long SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        //SetParent(IntPtr hWndChild, IntPtr hWndNewParent);这个方法很重要，就是将hWndChild指向开启exe的窗体嵌入到hWndNewParent窗体的某个控件上，或者是窗体本 身的容器
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);
        // MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);这个方法是windows的api,见名知意，是移动hwnd所指的窗体到指定的位置，并且指定是否需要重绘

        public static IntPtr SetWindowLong(HandleRef hWnd, int nIndex, int dwNewLong)
        {


            if (IntPtr.Size == 4)
            {
                return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
            }
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        }
        public Form1()
        {
            InitializeComponent();
            appIdleAction = new Action<object, EventArgs>(Application_Idle);
            appIdleEvent = new EventHandler(appIdleAction);


        }
        /// <summary>
        /// 确保应用程序嵌入此容器，再次确认exe嵌入，如果不调用这个方法，程序不一定能嵌入外部exe
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Application_Idle(object sender, EventArgs e)
        {
            if (this.m_AppProcess == null || this.m_AppProcess.HasExited)
            {
                this.m_AppProcess = null;
                Application.Idle -= appIdleEvent;//这一步一直不知道有什么用，但是不用这行代码程序有时候能嵌入有时候又不行
                return;
            }
            if (m_AppProcess.MainWindowHandle == IntPtr.Zero) return;
            Application.Idle -= appIdleEvent;
            EmbedProcess(m_AppProcess, this);


        }
        /// <summary>
        /// 将指定的程序嵌入指定的控件
        /// </summary>
        private void EmbedProcess(Process app, Control control)
        {


            // Get the main handle
            if (app == null || app.MainWindowHandle == IntPtr.Zero || control == null) return;
            try
            {
                // Put it into this form
                SetParent(app.MainWindowHandle, control.Handle);
            }
            catch (Exception ex1)
            {
                Console.WriteLine(ex1.Message);
            }
            try
            {
                // Remove border and whatnot               
                SetWindowLong(new HandleRef(this, app.MainWindowHandle), GWL_STYLE, WS_VISIBLE);
            }
            catch (Exception ex2)
            {
                Console.WriteLine(ex2.Message);
            }
            try
            {
                MoveWindow(app.MainWindowHandle, 0, 0, control.Width, control.Height, true);
            }
            catch (Exception ex3)
            {
                Console.WriteLine(ex3.Message);
            }
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {            //以下这段代码是通过命令行方式调起一个exe程序，获取这个程序的句柄然后嵌入主的winform窗体中
                     // ProcessStartInfo info = new ProcessStartInfo(@"C:\Program Files (x86)\Microsoft Office\Office12\EXCEL.EXE");
            ProcessStartInfo info = new ProcessStartInfo(System.Environment.CurrentDirectory + @"\FPGA更新旧版本0716\tftpd32.exe");


            info.WindowStyle = ProcessWindowStyle.Minimized;
            info.UseShellExecute = false;
            info.CreateNoWindow = false;
            m_AppProcess = System.Diagnostics.Process.Start(info);
            Application.Idle += appIdleEvent;
            try
            {
                EmbedProcess(m_AppProcess, this);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
}
