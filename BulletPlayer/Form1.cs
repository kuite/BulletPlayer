using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BulletPlayerBackend;
using BulletPlayerBackend.Utils;

namespace BulletPlayer
{
    public partial class Form1 : Form
    {
        private SessionManager _manager;

        public string SuggestedMove { get; set; }

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _manager = new SessionManager();
            label7.Text = "Logged as : " + _manager.Login();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var processEngine = _manager.EngineHandlerInstance.TurnEngineOn();

            _manager.MovesHandlerInstance.PlayerColor = "white";
            if (radioButton2.Checked)
                _manager.MovesHandlerInstance.PlayerColor = "black";

            _manager.StartScann(_manager.EngineHandlerInstance, processEngine);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //TODO: turn off scanning and playing
            _manager.IsScanning = false;
            _manager.IsPlaying = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //_manager.MovesHandlerInstance.PlayerColor = "white";
            //if (radioButton2.Checked)
            //    _manager.MovesHandlerInstance.PlayerColor = "black";
            //_manager.IsPlaying = true;
        }

        public void SetLabelSeven()
        {
            label7.Text = SuggestedMove;
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

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
