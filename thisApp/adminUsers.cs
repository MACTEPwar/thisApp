using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ReadAndVerify
{
    public partial class adminUsers : Form
    {
        public adminUsers()
        {
            InitializeComponent();
        }

        XmlNodeList userList;
        int currentIdUser;

        private void loadAllUsers()
        {
            //Users u = new Users();
            listBox1.Items.Clear();
            userList = new Users().getAllUsers();
            foreach(XmlNode user in userList)
            {
                listBox1.Items.Add(user.SelectSingleNode("username").InnerText);
            }
        }

        private void adminUsers_Load(object sender, EventArgs e)
        {
            loadAllUsers();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            MouseEventArgs ee = e as MouseEventArgs;
            int index = listBox1.IndexFromPoint(ee.Location);
            if (index > -1)
            {
                //MessageBox.Show(userList[index].SelectSingleNode("@id").Value);
                textBox1.Text = userList[index].SelectSingleNode("login").InnerText;
                textBox2.Text = userList[index].SelectSingleNode("password").InnerText;
                currentIdUser = Convert.ToInt32(userList[index].SelectSingleNode("@id").Value);
            }
            if (stateBut) ad.Close();
            {
                ad = new adminDocuments(currentIdUser);
                ad.Height = this.Height;
                ad.Show();
                ad.Location = new Point(this.Location.X + this.Width - 15, this.Location.Y);
            }
            stateBut = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new Users().updateLoginById(currentIdUser, textBox1.Text);
            new Users().updatePasswordById(currentIdUser, textBox2.Text);
        }

        adminDocuments ad;
        bool stateBut = false;
        private void button2_Click(object sender, EventArgs e)
        {
            
            if (!stateBut)
            {
                ad = new adminDocuments(currentIdUser);
                ad.Height = this.Height;
                ad.Show();
                ad.Location = new Point(this.Location.X + this.Width - 15, this.Location.Y);
            }
            else
            {
                ad.Close();
            }
            stateBut = !stateBut;
        }

        private void adminUsers_LocationChanged(object sender, EventArgs e)
        {
            
            if (ad!=null)
            {
                ad.Location = new Point(this.Location.X + this.Width - 15, this.Location.Y);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            createUser cu = new createUser();
            cu.Show();
            cu.FormClosed += (o, v) => {
                loadAllUsers();
            };
        }
    }
}
