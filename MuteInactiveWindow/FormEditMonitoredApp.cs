﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MuteInactiveWindow
{
    public partial class FormEditMonitoredApp : Form
    {
        public string value { get; set; }
        public FormEditMonitoredApp(string currentText)
        {
            InitializeComponent();
            textBox1.Text = currentText;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            value = textBox1.Text;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text == "") button1.Enabled = false;
            else button1.Enabled = true;
        }
    }
}
