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
            _manager.Login(textBox1.Text, textBox2.Text, comboBox1.SelectedItem.ToString());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _manager.PlayerColor = "white";
            if (radioButton2.Checked)
                _manager.PlayerColor = "black";

            _manager.StartPlaying();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _manager.StopPlaying();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _manager.SetBoardCoordinates();
        }

        public void SetNotification(string notification)
        {
            label5.Text = notification;
            if (notification.StartsWith("B"))
                label5.ForeColor = Color.OrangeRed;
            if (notification.StartsWith("F"))
                label5.ForeColor = Color.ForestGreen;
        }
    }
}
