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
    public partial class adminDocuments : Form
    {
        int id;

        public adminDocuments(int id)
        {
            InitializeComponent();
            this.id = id;
        }

        void loadDocInList(string state,ListBox l)
        {
            l.Items.Clear();
            switch(state)
            {
                case "Не прочитанные":
                    {
                        foreach(int item in new Users().getReadOffDocuments(id))
                        {
                            try
                            {
                                l.Items.Add(new Documents().getDocumentById(item).Attributes["name"].Value);
                            }
                            catch
                            {
                                continue;
                            }
                        }
                        break;
                    }
                case "Прочитанные":
                    {
                        foreach (int item in new Users().getReadOnDocuments(id))
                        {
                            l.Items.Add(new Documents().getDocumentById(item).Attributes["name"].Value);
                        }
                        break;
                    }
            }
        }
        void loadChkList(CheckedListBox clb)
        {
            foreach(XmlNode item in new Documents().getAllGroups())
            {
                clb.Items.Add(item.Attributes["name"].Value);
            }
        }

        private void adminDocuments_Load(object sender, EventArgs e)
        {
            loadDocInList(comboBox1.Text,listBox1);
            loadChkList(checkedListBox1);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            loadDocInList(comboBox1.Text, listBox1);
        }
    }
}
