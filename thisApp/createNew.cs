using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ReadAndVerify
{
    public partial class createNew : Form
    {
        private bool isL = false; 
        public createNew(bool isLow)
        {
            InitializeComponent();
            this.isL = isLow;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (isL)
            {
                LowNews lw = new LowNews(textBox1.Text, textBox2.Text, DateTime.Now.ToString());
                this.Close();
            }
            else
            {
                LowNews lw = new LowNews(textBox1.Text, textBox2.Text, DateTime.Now.ToString(), @"../../strongNews.xml");
                this.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
