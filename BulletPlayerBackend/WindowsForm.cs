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

        public WindowsForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _manager = new SessionManager(this);
            label4.Text = "Logged as : " + _manager.Login(textBox1.Text, textBox2.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var processEngine = _manager.EngineHandlerInstance.TurnEngineOn();

            _manager.MovesHandlerInstance.PlayerColor = "white";
            if (radioButton2.Checked)
                _manager.MovesHandlerInstance.PlayerColor = "black";

            _manager.StartPlaying(processEngine);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _manager.StopPlaying();
            _manager.IsPlaying = false;
        }
    }
}
