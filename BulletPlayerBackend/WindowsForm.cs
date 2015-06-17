using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BulletPlayerBackend
{
    public partial class WindowsForm : Form
    {
        private SessionManager _manager;

        public string SuggestedMove { get; set; }

        public WindowsForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _manager = new SessionManager(this);
            label4.Text = "Logged as : " + _manager.Login();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var processEngine = _manager.EngineHandlerInstance.TurnEngineOn();

            _manager.MovesHandlerInstance.PlayerColor = "white";
            if (radioButton2.Checked)
                _manager.MovesHandlerInstance.PlayerColor = "black";

            _manager.StartScann(_manager.EngineHandlerInstance, processEngine);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //TODO: turn off scanning and playing
            _manager.IsScanning = false;
            _manager.IsPlaying = false;
        }

        public void ShowMove(string move)
        {
            label3.Text = "Suggested move: " + move;
        }

        public static void ShowConsoleWindow()
        {
            var handle = GetConsoleWindow();

            if (handle == IntPtr.Zero)
            {
                AllocConsole();
            }
            else
            {
                ShowWindow(handle, SW_SHOW);
            }
        }

        public static void HideConsoleWindow()
        {
            var handle = GetConsoleWindow();

            ShowWindow(handle, SW_HIDE);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
    }
}
