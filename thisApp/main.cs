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
using System.IO;
using libzkfpcsharp;

namespace ReadAndVerify
{
    public partial class main : zkt
    {
        public main()
        {
            InitializeComponent();
        }

        void loadXmlProgressBar()
        {
            splitContainer1.Panel1.Controls.Clear();
            string pathToXml = "../../projectMain.xml";
            XmlDocument document = new XmlDocument();
            document.Load(pathToXml);

            XmlElement xRoot = document.DocumentElement;
            int pbY = 20;
            foreach (XmlNode xml in xRoot)
            {
                ProgressBar pb = new ProgressBar();
                pb.Width = this.Width - 60;
                pb.Height = 40;
                pb.Location = new Point(20, pbY);
                pb.Name = "pb" + pbY.ToString();
                pb.Parent = splitContainer1.Panel1;

                DateTime tempDeadLine = DateTime.Today;
                foreach (XmlNode childnode in xml.ChildNodes)
                {
                    if (childnode.Name == "title")
                    {
                        Label lb = new Label();
                        lb.Text = childnode.InnerText;
                        lb.Location = new Point(20, pbY + 42);
                        lb.Parent = splitContainer1.Panel1;
                        lb.Font = new Font(lb.Font.FontFamily, 14, FontStyle.Bold);
                    }
                    if (childnode.Name == "startDate")
                    {
                        TimeSpan min = DateTime.Today.Subtract(Convert.ToDateTime(childnode.InnerText));
                        TimeSpan max = tempDeadLine.Subtract(Convert.ToDateTime(childnode.InnerText));
                        double day = 0.0;
                        if (min.TotalDays != 0 && max.TotalDays != 0)
                            day = min.TotalDays * 100 / max.TotalDays;
                        else if (min.TotalDays == 0) day = 1 * 100 / max.TotalDays;
                        else if (max.TotalDays == 0) day = min.TotalDays * 100 / 1;
                        if (day > 0 && day < 1) day = 1.0;
                        if (day > 100) day = 100;
                        pb.Value = (int)day;
                        pb.MouseEnter += (o, v) =>
                        {
                            if (infoText != null) this.splitContainer1.Panel1.Controls.Remove(infoText);
                            infoText = new Label();
                            infoText.Text = pb.Value.ToString() + "%";
                            infoText.BorderStyle = BorderStyle.FixedSingle;
                            infoText.Size = new Size(50, 25);
                            infoText.Location = new Point(MousePosition.X + 1, MousePosition.Y - 70);
                            this.splitContainer1.Panel1.Controls.Add(infoText);
                            infoText.Font = new Font(infoText.Font.FontFamily, 10, FontStyle.Bold);
                            infoText.BringToFront();

                            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                            infoText.BackColor = Color.Transparent;
                        };
                        pb.MouseMove += (o, v) =>
                        {
                            if (infoText != null) infoText.Location = new Point(MousePosition.X - 51, MousePosition.Y - 70);
                        };

                        pb.MouseLeave += (o, v) =>
                        {
                            if (infoText != null) this.splitContainer1.Panel1.Controls.Remove(infoText);
                        };
                    }
                    if (childnode.Name == "deadline")
                    {
                        tempDeadLine = Convert.ToDateTime(childnode.InnerText);
                    }
                }
                pbY += 70;
            }
        }

        Label infoText = null;

        private void fAdmin(bool state)
        {
            menuStrip1.Items[0].Visible = !state;
            menuStrip1.Items[1].Visible = state;
            menuStrip1.Items[2].Visible = state;
            menuStrip1.Items[3].Visible = state;
            menuStrip1.Items[4].Visible = state;
            if (state)
            {
                Properties.Settings.Default.isAdmin = state;
                //free();
            }
            else
            {
                Properties.Settings.Default.isAdmin = state;
                //bnInit_Click();
                //bnOpen_Click();
            }
        }


        // Обновляем новости в обееих панелях
        private void updateAllNews()
        {
            news.Controls.Clear();
            newsStrong.Controls.Clear();
            string path_lowNews = "../../lowNews.xml";
            LowNews l = new LowNews(path_lowNews);
            List<LowNews> newsList = l.getSortedNews("DESC");
            int startY = 10;
            foreach (LowNews lw in newsList)
            {
                int line = 1;
                addingOneNewsForForm(lw.id, lw.title, lw.content, lw.date, 0, startY, news, out line);
                startY += line + 10;
            }
            string path_strongNews = "../../strongNews.xml";
            LowNews lnews = new LowNews(path_strongNews);
            List<LowNews> newsListS = lnews.getSortedNews("DESC");
            startY = 10;
            foreach (LowNews lw in newsListS)
            {
                int line = 1;
                addingOneNewsForForm(lw.id, lw.title, lw.content, lw.date, 0, startY, newsStrong, out line);
                startY += line + 10;
            }
        }

        bool deleteNews = false;
        // Добавляет Одну новость в панель
        private void addingOneNewsForForm(int id, string title, string content, string date, int x, int y, Panel pan, out int lines)
        {
            Panel oneNew = new Panel();
            oneNew.BackColor = Color.FromArgb(211, 211, 211);
            oneNew.Location = new Point(x + 10, y);
            Label l1 = new Label();
            l1.Name = id.ToString();
            l1.BackColor = Color.FromArgb(47, 79, 79);
            l1.Text = title;
            l1.Location = new Point(5, 0);
            l1.Font = new Font(l1.Font.FontFamily, 14, FontStyle.Bold | FontStyle.Italic);
            l1.ForeColor = Color.FromArgb(211, 211, 211);
            l1.Click += (o,v) => {
                if (Properties.Settings.Default.isAdmin)
                {
                    //MessageBox.Show(pan.Name);
                    if (pan.Name == "pan1")
                    {
                        new LowNews().deleteNews(Convert.ToInt32(l1.Name));
                        updateAllNews();
                    }
                    else
                    {
                        new LowNews(@"../../strongNews.xml").deleteNews(Convert.ToInt32(l1.Name));
                        updateAllNews();
                    }
                }
            };
            oneNew.Controls.Add(l1);
            l1.AutoSize = true;
            l1.MaximumSize = new Size(700, 25);
            Label ct = new Label();
            ct.Text = content;
            ct.Location = new Point(6, 25);
            ct.Font = new Font(ct.Font.FontFamily, 14);
            oneNew.Controls.Add(ct);
            ct.AutoSize = true;
            ct.MaximumSize = new Size(pan.Width - 35, 999999999);
            lines = ct.Height + l1.Height + 10;
            Label l2 = new Label();
            l2.Text = date;
            l2.Width = 150;
            l2.Location = new Point(pan.Width - 170, 0);
            l2.Font = new Font(l2.Font.FontFamily, 10);
            oneNew.Controls.Add(l2);
            oneNew.Size = new Size(pan.Width - 30, lines);
            oneNew.BorderStyle = BorderStyle.FixedSingle;
            pan.Controls.Add(oneNew);
        }

        protected override void DefWndProc(ref Message m)
        {
            if (m.Msg == MESSAGE_CAPTURED_OK && !Properties.Settings.Default.isAdmin && FormHandle == this.Handle)
            {
                MemoryStream ms = new MemoryStream();
                BitmapFormat.GetBitmap(FPBuffer, mfpWidth, mfpHeight, ref ms);
                Bitmap bmp = new Bitmap(ms);
                String strShow = zkfp2.BlobToBase64(CapTmp, cbCapTmp);
                XmlNodeList _users = new Users().getAllUsers();
                foreach (XmlNode user in _users)
                {
                    string pass = user.SelectSingleNode("password").InnerText;
                    if (mach(strShow, pass) > 75)
                    {
                        //MessageBox.Show(user.SelectSingleNode("username").InnerText);
                        Registration reg = new Registration(new Users().getIdByLogin(user.SelectSingleNode("login").InnerText));
                        free();
                        //FormHandle = reg.Handle;
                        //MessageBox.Show(this.Handle.ToString());
                        reg.Show();
                        reg.FormClosed += (o, v) => {
                            FormHandle = this.Handle;
                            bnInit_Click();
                            bnOpen_Click();
                            //MessageBox.Show("");
                        };
                    }
                }
            }
            //else if (m.Msg == MESSAGE_CAPTURED_OK)
            //{
            //    MessageBox.Show("OK");
            //    MessageBox.Show(this.Handle.ToString());
            //    MessageBox.Show(FormHandle.ToString());
            //    MessageBox.Show(Properties.Settings.Default.isAdmin.ToString());
            //    base.DefWndProc(ref m);
            //}
            else
            {
                base.DefWndProc(ref m);
            }
        }


        private void main_Load(object sender, EventArgs e)
        {
            //MessageBox.Show(DateTime.Now.ToString());
            FormHandle = this.Handle;
            bnInit_Click();
            bnOpen_Click();
            this.WindowState = FormWindowState.Normal;
            this.Bounds = Screen.PrimaryScreen.Bounds;
            //this.TopMost = true;
            addElementsOnForm();
            allXml = readingXml();
            loadXmlProgressBar();
            updateAllInfoInProject();
            updateAllNews();


            //zkt _zkt = new zkt(this.Handle);
            //_zkt.init();
            //_zkt.open();

            //updateAllNews();
            //LowNews l = new LowNews("../../lowNews.xml");
            //LowNews l = new LowNews("../../lowNews.xml",7,"title 7","MyContent", "28.04.2018 04:15:01");
            //l.updateNews(7, "title 7 NEW UPDATE", "MyContent UPDATE", "28.04.2018 04:15:01");
            //updateAllNews();
            //l.deleteNews(9);

            //Users us = new Users("../../xml/Users.xml");
            //MessageBox.Show(us.getPasswordByLogin("1"));
            //MessageBox.Show(us.getNewId().ToString());

            //if (us.getPasswordByLogin("login1") == null)
            //us.createUser("abra", "login1", "123qweASD", new int[] { 1, 2 });
            //us.updatePasswordById(3,"0987pr");


            //new Documents().createGroup("куку");
            //new Documents().updateGroup(5, "rere");

            //new Documents().replaceDocForGroup(3, "Важные");

            new Documents().fillStandartGroup();
            fAdmin(false);
        }
        Panel news;
        Panel newsStrong;
        private void addElementsOnForm()
        {
            Label lNewsLow = new Label();
            lNewsLow.AutoSize = true;
            lNewsLow.Text = "Новости :";
            lNewsLow.Font = new Font(lNewsLow.Font.FontFamily, 14, FontStyle.Bold);
            lNewsLow.Location = new Point(10, 10);
            news = new Panel();
            news.Name = "pan1";
            news.Size = new Size((splitContainer1.Panel2.Width / 2) - 30, splitContainer1.Panel2.Height - 130);
            news.Location = new Point(10, 40);
            //news.BackColor = Color.Yellow;
            news.BorderStyle = BorderStyle.FixedSingle;
            news.AutoScroll = true;
            this.splitContainer1.Panel2.Controls.Add(news);
            this.splitContainer1.Panel2.Controls.Add(lNewsLow);

            Label lNewsS = new Label();
            lNewsS.AutoSize = true;
            lNewsS.Text = "Выжные новости :";
            lNewsS.Font = new Font(lNewsS.Font.FontFamily, 14, FontStyle.Bold);
            lNewsS.Location = new Point(news.Width + 20, 10);
            newsStrong = new Panel();
            newsStrong.Name = "pan2";
            newsStrong.Size = new Size((splitContainer1.Panel2.Width / 2), splitContainer1.Panel2.Height - 130);
            newsStrong.Location = new Point(news.Width + 20, 40);
            //news.BackColor = Color.Yellow;
            newsStrong.BorderStyle = BorderStyle.FixedSingle;
            newsStrong.AutoScroll = true;
            this.splitContainer1.Panel2.Controls.Add(newsStrong);
            this.splitContainer1.Panel2.Controls.Add(lNewsS);

        }

        private void updateAllInfoInProject()
        {
            allXml = readingXml();
            addMenuUpdate(allXml);
            loadXmlProgressBar();
            addMenuDelete(allXml);
        }

        void addMenuUpdate(XmlElement xRoot)
        {
            this.изменитьToolStripMenuItem.DropDownItems.Clear();
            foreach (XmlNode xml in xRoot)
            {
                foreach (XmlNode childnode in xml.ChildNodes)
                {
                    if (childnode.Name == "title")
                    {
                        ToolStripMenuItem newItemMenu = new ToolStripMenuItem();
                        newItemMenu.Text = childnode.InnerText;
                        this.изменитьToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { newItemMenu });
                        ToolStripMenuItem newItemMenuUpdateTitle = new ToolStripMenuItem();
                        newItemMenuUpdateTitle.Text = "Изменить название";
                        ToolStripMenuItem newItemMenuUpdateStartDate = new ToolStripMenuItem();
                        newItemMenuUpdateStartDate.Text = "Изменить дату начала";
                        ToolStripMenuItem newItemMenuUpdateDeadLine = new ToolStripMenuItem();
                        newItemMenuUpdateDeadLine.Text = "Изменить дату окончания";
                        newItemMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { newItemMenuUpdateTitle, newItemMenuUpdateStartDate, newItemMenuUpdateDeadLine });
                        newItemMenuUpdateTitle.Click += (e, ov) =>
                        {
                            foreach (XmlNode thisXml in allXml)
                            {
                                foreach (XmlNode thisChildnode in thisXml.ChildNodes)
                                {
                                    if (thisChildnode.Name == "title" && thisChildnode.InnerText == newItemMenuUpdateTitle.OwnerItem.Text)
                                    {
                                        string newTitle = Microsoft.VisualBasic.Interaction.InputBox("Введите новое имя проекта :");
                                        foreach (ToolStripMenuItem menuItem in this.изменитьToolStripMenuItem.DropDownItems)
                                        {
                                            if (newTitle == menuItem.Text)
                                            {
                                                MessageBox.Show("Проект с таким именем уже существует. Измените имя и повторите попытку.");
                                                return;
                                            }
                                        }
                                        if (newTitle == "")
                                        {
                                            MessageBox.Show("Имя проекта не может быть пустым");
                                            return;
                                        }
                                        thisChildnode.InnerText = newTitle;
                                        dc.Save("../../projectMain.xml");
                                        MessageBox.Show("Название проекта успешно изменено.");
                                        allXml = readingXml();
                                        addMenuUpdate(allXml);
                                        loadXmlProgressBar();
                                    }
                                }
                            }
                        };
                        newItemMenuUpdateDeadLine.Click += (e, ov) =>
                        {
                            bool b = false;
                            foreach (XmlNode thisXml in allXml)
                            {
                                foreach (XmlNode thisChildnode in thisXml.ChildNodes)
                                {
                                    if (b)
                                    {
                                        thisChildnode.InnerText = Microsoft.VisualBasic.Interaction.InputBox("Введите дату окончания проекта :");
                                        dc.Save("../../projectMain.xml");
                                        MessageBox.Show("Дата окончания проекта успешно изменена.");
                                        allXml = readingXml();
                                        addMenuUpdate(allXml);
                                        b = false;
                                    }
                                    if (thisChildnode.Name == "title" && thisChildnode.InnerText == newItemMenuUpdateTitle.OwnerItem.Text)
                                    {
                                        b = true;
                                        continue;
                                    }
                                }
                            }
                        };
                        newItemMenuUpdateStartDate.Click += (e, ov) =>
                        {
                            int b = 0;
                            foreach (XmlNode thisXml in allXml)
                            {
                                foreach (XmlNode thisChildnode in thisXml.ChildNodes)
                                {
                                    if (b == 1)
                                    {
                                        b++;
                                        continue;
                                    }
                                    if (b >= 2)
                                    {
                                        thisChildnode.InnerText = Microsoft.VisualBasic.Interaction.InputBox("Введите дату начала проекта :");
                                        dc.Save("../../projectMain.xml");
                                        MessageBox.Show("Дата начала проекта успешно изменена.");
                                        allXml = readingXml();
                                        addMenuUpdate(allXml);
                                        b = 0;
                                    }
                                    if (thisChildnode.Name == "title" && thisChildnode.InnerText == newItemMenuUpdateTitle.OwnerItem.Text)
                                    {
                                        b++; continue;
                                    }
                                }
                            }
                        };
                    }
                }
            }
        }

        void addMenuDelete(XmlElement xRoot)
        {
            this.удалитьToolStripMenuItem.DropDownItems.Clear();
            foreach (XmlNode xml in xRoot)
            {
                foreach (XmlNode childnode in xml.ChildNodes)
                {
                    if (childnode.Name == "title")
                    {
                        ToolStripMenuItem newItemMenu = new ToolStripMenuItem();
                        newItemMenu.Text = childnode.InnerText;
                        this.удалитьToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { newItemMenu });
                        newItemMenu.Click += (o, v) =>
                        {
                            foreach (XmlNode thisXml in allXml)
                            {
                                foreach (XmlNode thisChildnode in thisXml.ChildNodes)
                                {
                                    if (thisChildnode.Name == "title" && thisChildnode.InnerText == newItemMenu.Text)
                                    {
                                        thisChildnode.ParentNode.ParentNode.RemoveChild(thisChildnode.ParentNode);
                                        MessageBox.Show("Проект был удален.");
                                        dc.Save("../../projectMain.xml");
                                        allXml = readingXml();
                                        updateAllInfoInProject();
                                    }
                                }
                            }
                        };
                    }
                }
            }
        }

        private void NewItemMenuUpdateTitle_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        XmlElement readingXml()
        {
            if (allXml != null) allXml.RemoveAll();
            string pathToXml = "../../projectMain.xml";
            dc = new XmlDocument();
            dc.Load(pathToXml);
            return dc.DocumentElement;
        }

        XmlElement allXml;
        XmlDocument dc;

        void newElementInXml(XmlDocument d, string title, string startDate, string deadline)
        {
            DateTime tm;
            if (!DateTime.TryParse(startDate, out tm) || !DateTime.TryParse(deadline, out tm))
            {
                MessageBox.Show("Не удалось добавить новый элемент. \nНе верный формат даты.");
                return;
            }
            XmlNode element = d.CreateElement("Project");
            d.DocumentElement.AppendChild(element); // указываем родителя

            addElementInXml(d, element, "title", title);
            addElementInXml(d, element, "deadline", deadline);
            addElementInXml(d, element, "startDate", startDate);
        }

        void addElementInXml(XmlDocument d, XmlNode parent, string name, string value)
        {
            XmlNode subElement = d.CreateElement(name); // даём имя
            subElement.InnerText = value; // и значение
            parent.AppendChild(subElement); // и указываем кому принадлежит
        }

        private void button1_Click(object sender, EventArgs e)
        {
            loadXmlProgressBar();
        }

        private void button2_Click(object sender, EventArgs e)
        {
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fAdmin(false);
        }

        private void добавитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string pathToXml = "../../projectMain.xml";
            XmlDocument document = new XmlDocument();
            document.Load(pathToXml);
            string name = Microsoft.VisualBasic.Interaction.InputBox("Введите название проекта");
            foreach (ToolStripMenuItem menuItem in this.изменитьToolStripMenuItem.DropDownItems)
            {
                if (name == menuItem.Text)
                {
                    MessageBox.Show("Проект с таким именем уже существует. Измените имя и повторите попытку.");
                    return;
                }
            }
            if (name == "")
            {
                MessageBox.Show("Имя проекта не может быть пустым");
                return;
            }
            string dateStr = Microsoft.VisualBasic.Interaction.InputBox("Дату начала проекта");
            string dateFns = Microsoft.VisualBasic.Interaction.InputBox("Дату окончания проекта");
            newElementInXml(document, name, dateStr, dateFns);
            document.Save(pathToXml);
            updateAllInfoInProject();
        }

        private void войтиВАдминкуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Microsoft.VisualBasic.Interaction.InputBox("") == Properties.Settings.Default.adminPassword)
            {
                fAdmin(true);
            }
        }

        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void button3_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Authentication auth = new Authentication();
            auth.Show();
        }

        private void пользователиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void добавитьToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //LowNews ln = new LowNews("",
            createNew cn = new createNew(true);
            cn.Show();
            cn.FormClosed += (o, v) => {
                updateAllNews();
            };
        }

        private void удалитьToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            createNew cn = new createNew(false);
            cn.Show();
            cn.FormClosed += (o, v) =>
            {
                updateAllNews();
            };
        }

        private void удалитьToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            new LowNews(@"../../strongNews.xml").deleteNews(1); 
        }

        private void изменитьToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void пользователиToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            free();
            //MessageBox.Show(FormHandle.ToString());
            adminUsers adm = new adminUsers();

            adm.FormClosed += (o, v) =>
            {
                try
                {
                    free();
                }
                catch
                {
                }

                FormHandle = this.Handle;
                bnInit_Click();
                bnOpen_Click();
                //MessageBox.Show(FormHandle.ToString());
            };
            adm.Show();
        }

        private void поменятьПарольАдминаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.adminPassword = Microsoft.VisualBasic.Interaction.InputBox("Введите пароль, на который хотите поменять");
            Properties.Settings.Default.Save();
        }
    }
}
