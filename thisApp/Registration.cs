using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Xml;
using libzkfpcsharp;

namespace ReadAndVerify
{
    public partial class Registration : zkt
    {
        Color col;
        Point loc;
        bool isAdmin = false;
        public int id;
        private bool DeviceExist = false;
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource = null;
        private List<string> camList = null;
        private IntPtr fh;

        //public Registration(int id,IntPtr fh)
        //{
        //    this.fh = fh;
        //    this.id = id;
        //    InitializeComponent();
        //}

        public Registration(int id)
        {
            this.id = id;
            InitializeComponent();
        }

        /// <summary>
        /// 01 - Запускаем часы в лайбл
        /// 02 - Загрузить все не прочитанные файлы в листбокс
        /// 03 - Вход/Выход в режим админа
        /// 04 - Начать показ видео
        /// 05 - Передать dидео в пикчу
        /// 06 - Освобождение ресурсов и остановка показа
        /// 07 - Получить список камер
        /// 08 - Поулчить количество страниц в документи по пути
        /// 09 - Добавление записи в лог
        /// </summary>
        /// <param></param>
        void setTimeLabel(Label l)
        {
            //while (true)
            //{
                var a = new Action(() => { l.Text = DateTime.Now.ToString(); });
                this.Invoke(a);
            //}
        }//1
        void uploadReadOffDoc(ListBox l)
        {
            var ac = new Action(() =>
            {
                l.Items.Clear();
            });
            this.Invoke(ac);
            Users user = new Users();
            List<int> documents = user.getReadOffDocuments(id);
            foreach (int i in documents)
            {
                try
                {
                    string pathDoc = new Documents().getDocumentById(i).Attributes.Item(2).Value;
                    var a = new Action(() =>
                    {
                        l.Items.Add(pathDoc);
                    });
                    this.Invoke(a);
                }
                catch
                {
                }
            } 
        }//2
        void admin(bool state)
        {
            toolStripComboBox1.Visible = state;
            настройкиКамерыToolStripMenuItem.Visible = state;
        }//3
        private void start()
        {
            //if (start.Text == "&Start")
            {
                if (DeviceExist)
                {
                    toolStripComboBox1.Text = toolStripComboBox1.Items[0].ToString();
                    videoSource = new VideoCaptureDevice(videoDevices[toolStripComboBox1.SelectedIndex].MonikerString);
                    videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
                    CloseVideoSource();
                    videoSource.DesiredFrameSize = new Size(222, 222);
                    videoSource.Start();
                }
                else
                {
                    MessageBox.Show("Ошибка. Выберите устройство.");
                }
            }
        }//4
        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap img = (Bitmap)eventArgs.Frame.Clone();
            pictureBox1.Image = img;
        }//5
        private void CloseVideoSource()
        {
            if (!(videoSource == null))
                if (videoSource.IsRunning)
                {
                    videoSource.SignalToStop();
                    videoSource = null;
                }
        }//6
        private void getCamList(List<string> list, ToolStripComboBox cBox)
        {
            try
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                list.Clear(); cBox.Items.Clear();
                if (videoDevices.Count == 0)
                    throw new ApplicationException();

                DeviceExist = true;
                foreach (FilterInfo device in videoDevices)
                {
                    list.Add(device.Name);
                    cBox.Items.Add(device.Name);
                }
            }
            catch (ApplicationException)
            {
                DeviceExist = false;
                list.Add("null");
                cBox.Items.Add("Нет веб-камеры.");
            }
        }//7
        public int getNumberOfPdfPages(string fileName)
        {
            using (StreamReader sr = new StreamReader(File.OpenRead(fileName)))
            {
                Regex regex = new Regex(@"/Type\s*/Page[^s]");
                MatchCollection matches = regex.Matches(sr.ReadToEnd());

                return matches.Count;
            }
        }//8
        private void addLogRecord(string path,string comment, string photo)
        {
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(new Users().getUsernameById(id) + ", "+ photo +", " +DateTime.Now.ToString()+", "+comment);
            }
        }//9

        Point axLoc = new Point();

        private void Registration_Load(object sender, EventArgs e)
        {
            FormHandle = this.Handle;
            axLoc = axAcroPDF1.Location;
            bnInit_Click();
            bnOpen_Click();
            //MessageBox.Show(FormHandle.ToString());
            new Documents().deleatExistingItems(this.id);
            new Documents().deleatExistingItems(this.id);
            //new Users().deleteDoc(this.id, 14);
            new Documents().fillStandartGroup();
            //MessageBox.Show(this.id.ToString());
            this.WindowState = FormWindowState.Maximized;
            admin(isAdmin);
            timer2.Interval = 1000;
            timer2.Start();
            uploadReadOffDoc(listBox1);
            uploadReadOnDoc(listBox2);
            // Для кнопок приход-уход
            if (listBox1.Items.Count != 0)
            {
                button1.Enabled = false; button2.Enabled = false;
            }
            else
            {
                if (DateTime.Now.Hour >= 7 && DateTime.Now.Hour <= 13)
                {
                        button1.Enabled = true; button2.Enabled = false;
                }
                else
                {
                        button1.Enabled = false; button2.Enabled = true;
                }
            }
            // Для кнопок приход-уход
            this.FormClosed += (o, ev) => {
                CloseVideoSource(); timer2.Stop();timer1.Stop();
            };
            this.SizeChanged += (o, ev) => {
                axAcroPDF1.Height = listBox1.Height + listBox1.Location.Y - 25;
            };
            camList = new List<string>();
            getCamList(camList, toolStripComboBox1);
            if (camList.Count > 0 && camList[0] != "null")
                start();
            else
                MessageBox.Show("Камера не обнаружена");
            label2.Text = new Users().getUsernameById(this.id);
            findNewDocuments();
            uploadReadOffDoc(listBox1);
        }

        private void войтиВАдминкуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!isAdmin)
            {
                if (Microsoft.VisualBasic.Interaction.InputBox("Введите пароль :") == Properties.Settings.Default.adminPassword)
                {
                    isAdmin = true;
                    admin(true);
                    войтиВАдминкуToolStripMenuItem.Text = "Выйти с админки";
                }
            }
            else
            {
                isAdmin = false;
                admin(false);
                войтиВАдминкуToolStripMenuItem.Text = "Войти в админки";
            }
            
        }

        private void настройкиКамерыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            videoSource.DisplayPropertyPage(this.Handle);
        }

        int ird = -10;
        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                bool copy = false;
                MouseEventArgs ee = e as MouseEventArgs;
                int index = listBox1.IndexFromPoint(ee.Location);
                ird = index;
                //axAcroPDF1.LoadFile(Properties.Settings.Default.workDir + listBox1.Items[index].ToString());
                axAcroPDF1.LoadFile(listBox1.Items[index].ToString());
                //timer1.Interval = getNumberOfPdfPages(Properties.Settings.Default.workDir + listBox1.Items[index].ToString()) * 30000;
                timer1.Interval = 1;
                timer1.Start();
                //col = this.BackColor;
                loc = axAcroPDF1.Location;
                //menuStrip1.Visible = label1.Visible = listBox2.Visible = listBox1.Visible = button2.Visible = button1.Visible = pictureBox1.Visible = false;
                this.BackColor = Color.Gray;
                axAcroPDF1.Height = this.Height - 40;
                axAcroPDF1.Location = new Point(axAcroPDF1.Location.X, 0);
            }
            catch
            {

            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            //try
            //{
            //    Users user = new Users();
            //    List<int> documents = user.getReadOffDocuments(id);
            //    string path = Directory.GetCurrentDirectory() + @"\Log\Log.txt";
            //    string titleDoc = new Documents().getDocumentById(documents[ird]).SelectSingleNode("@name").Value;
            //    if (camList.Count > 0 && camList[0] != "null")
            //    {
            //    CloseVideoSource();
            //    user.docFromReadOffToReadOn(id, documents[ird]);
            //    uploadReadOffDoc(listBox1);
            //    using (Graphics g = Graphics.FromImage(pictureBox1.Image))
            //    {
            //        Pen p = new Pen(Color.White);
            //        Font drawFont = new Font("Arial", 20);
            //        SolidBrush drawBrush = new SolidBrush(Color.Red);
            //        g.DrawString(label1.Text, drawFont, drawBrush, new Point(390, 450));
            //        Users u = new Users();
            //        g.DrawString(u.getUsernameById(id), drawFont, drawBrush, new Point(10, 450));
            //    }
            //    string namePhoto = DateTime.Now.Day.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Year.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
            //    pictureBox1.Image.Save(Directory.GetCurrentDirectory() + @"\Log\Images\" + namePhoto + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                
            //    addLogRecord(path, "ПРОЧИТАЛ ДОКУМЕНТ "+ titleDoc, namePhoto);
            //    start();
            //    }
            //    else
            //    {
            //        addLogRecord(path, "ПРОЧИТАЛ ДОКУМЕНТ " + titleDoc, "ФОТО НЕ ПРОИЗВЕДЕНО");
            //        user.docFromReadOffToReadOn(id, documents[ird]);
            //        uploadReadOffDoc(listBox1);
            //    }
            //}
            //catch
            //{
                
            //}
            //menuStrip1.Visible = label1.Visible = listBox2.Visible = listBox1.Visible = button2.Visible = button1.Visible = pictureBox1.Visible = true;
            //axAcroPDF1.Height = listBox1.Height + listBox1.Location.Y - 25;
            //axAcroPDF1.Location = loc;
            //this.BackColor = col;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            setTimeLabel(label1);
            if (listBox1.Items.Count != 0)
            {
                button1.Enabled = false; button2.Enabled = false;
            }
            else
            {
                if (DateTime.Now.Hour >= 7 && DateTime.Now.Hour <= 13)
                {
                        button1.Enabled = true; button2.Enabled = false;
                }
                else
                {
                        button1.Enabled = false; button2.Enabled = true;
                }
            }
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            string path = Directory.GetCurrentDirectory() + @"\Log\Log.txt";
            if (camList.Count > 0 && camList[0] != "null")
            {
                CloseVideoSource();
                using (Graphics g = Graphics.FromImage(pictureBox1.Image))
                {
                    Pen p = new Pen(Color.White);
                    Font drawFont = new Font("Arial", 20);
                    SolidBrush drawBrush = new SolidBrush(Color.Red);
                    g.DrawString(label1.Text, drawFont, drawBrush, new Point(390, 450));
                    Users u = new Users();
                    g.DrawString(u.getUsernameById(id), drawFont, drawBrush, new Point(10, 450));
                }
                string namePhoto = DateTime.Now.Day.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Year.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
                pictureBox1.Image.Save(Directory.GetCurrentDirectory() + @"\Log\Images\" + namePhoto + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                addLogRecord(path, "ПРИХОД", namePhoto);
                start();
            }
            else
            {
                addLogRecord(path, "ПРИХОД", "ФОТО НЕ РПОИЗВЕДЕНО");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string path = Directory.GetCurrentDirectory() + @"\Log\Log.txt";
            if (camList.Count > 0 && camList[0] != "null")
            {
                CloseVideoSource();
                using (Graphics g = Graphics.FromImage(pictureBox1.Image))
                {
                    Pen p = new Pen(Color.White);
                    Font drawFont = new Font("Arial", 20);
                    SolidBrush drawBrush = new SolidBrush(Color.Red);
                    g.DrawString(label1.Text, drawFont, drawBrush, new Point(390, 450));
                    Users u = new Users();
                    g.DrawString(u.getUsernameById(id), drawFont, drawBrush, new Point(10, 450));
                }
                string namePhoto = DateTime.Now.Day.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Year.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
                pictureBox1.Image.Save(Directory.GetCurrentDirectory() + @"\Log\Images\" + namePhoto + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                addLogRecord(path, "УХОД", namePhoto);
                start();
            }
            else
            {
                addLogRecord(path, "УХОД", "ФОТО НЕ РПОИЗВЕДЕНО");
            }
        }

        private void findNewDocuments()
        {
            XmlNodeList readOnDoc = new Users().getReadOnDocuments(this.id,true);
            XmlNodeList readOffDoc = new Users().getReadOffDocuments(this.id, true);
            XmlNodeList allDoc = new Documents().getDocumentsByGroupId(new Documents().getIdGroupByTitle("all"));

            // Сравниваем документы все с папки и прочитанные, и лишние добавляем в не прочитанные. (остальные удалить)



            foreach (XmlNode xmlAllDoc in allDoc) //foreach (XmlNode xmlReadOnDoc in readOnDoc)
            {
                bool docIsExist = false;
                foreach (XmlNode xmlReadOnDoc in readOnDoc) //foreach (XmlNode xmlAllDoc in allDoc)
                {
                    //MessageBox.Show(xmlReadOnDoc.SelectSingleNode("@id").Value + " ;" + xmlAllDoc.SelectSingleNode("@id").Value);
                    if (xmlReadOnDoc.SelectSingleNode("@id").Value == xmlAllDoc.SelectSingleNode("@id").Value)
                    {
                        docIsExist = true;
                    }
                }
                if (!docIsExist)
                {
                    // Проверяем в непрочитанных

                    foreach (XmlNode xmlReadOffDoc in readOffDoc)
                    {
                        if (xmlReadOffDoc.SelectSingleNode("@id").Value == xmlAllDoc.SelectSingleNode("@id").Value)
                        {
                            docIsExist = true;
                        }
                    }
                    if (!docIsExist)
                    {
                        // добавляем id в не прочитанные
                        new Users().addDocToReadOff(this.id, xmlAllDoc.SelectSingleNode("@id").Value);
                    }
                   
                }
            }



            //FileInfo[] fia = new DirectoryInfo(Properties.Settings.Default.workDir).GetFiles();
            //foreach (FileInfo fi in fia)
            //{
            //    bool flag = false;
            //    foreach (XmlNode xn in readOnDoc)
            //    {
            //        if (fi.Name == xn.SelectSingleNode("@id").Value)
            //        {
            //            flag = true;
            //        }
            //    }
            //    if (!flag)
            //    {
            //        // добавить его в непрочитанные документы
                    
            //    }
            //}

            //foreach (XmlNode xc in readOnDoc)
            //{
            //    MessageBox.Show(xc.SelectSingleNode("@id").Value);
            //}
        }

        private void Registration_FormClosed(object sender, FormClosedEventArgs e)
        {
            free();
            //FormHandle = this.fh;
        }

        //private void deleatExistingItems()
        //{
        //    XmlNodeList readOnDoc = new Users().getReadOnDocuments(this.id, true);
        //    XmlNodeList readOffDoc = new Users().getReadOffDocuments(this.id, true);
        //    var List = readOnDoc.Cast<XmlNode>().Concat<XmlNode>(readOffDoc.Cast<XmlNode>());
        //    FileInfo[] fiAll = new DirectoryInfo(Properties.Settings.Default.workDir).GetFiles();
        //    foreach (XmlNode doc in List) //foreach (FileInfo fi in fiAll)
        //    {
        //        bool fileEx = false;
        //        foreach (FileInfo fi in fiAll) //foreach (XmlNode doc in List)
        //        {
        //            string name =  new Documents().getDocumentById(Convert.ToInt32(doc.SelectSingleNode("@id").Value)).SelectSingleNode("@name").Value;
        //            if (name == fi.Name)
        //            {
        //                fileEx = true;
        //                break;
        //            }
        //        }
        //        if (!fileEx)
        //        {
        //            new Documents().deleteDocument(Convert.ToInt32(doc.SelectSingleNode("@id").Value));
        //        }
        //    }
        //}

        void uploadReadOnDoc(ListBox list)
        {
            var ac = new Action(() =>
            {
                list.Items.Clear();
            });
            this.Invoke(ac);
            Users user = new Users();
            List<int> documents = user.getReadOnDocuments(id);
            foreach (int i in documents)
            {
                try
                {
                    string pathDoc = new Documents().getDocumentById(i).Attributes.Item(2).Value;
                    var a = new Action(() =>
                    {
                        list.Items.Add(pathDoc);
                    });
                    this.Invoke(a);
                }
                catch
                {
                }
            } 
        }

        protected override void DefWndProc(ref Message m)
        {
            if (m.Msg == MESSAGE_CAPTURED_OK && !Properties.Settings.Default.isAdmin && FormHandle == this.Handle)
            {
                MemoryStream ms1 = new MemoryStream();
                BitmapFormat.GetBitmap(FPBuffer, mfpWidth, mfpHeight, ref ms1);
                Bitmap bmp = new Bitmap(ms1);
                String strShow = zkfp2.BlobToBase64(CapTmp, cbCapTmp);


                //XmlNodeList _users = new Users().getAllUsers();
                //foreach (XmlNode user in _users)
                //{
                //    string pass = user.SelectSingleNode("password").InnerText;
                //    if (mach(strShow, pass) > 75)
                //    {
                //        //MessageBox.Show(user.SelectSingleNode("username").InnerText);
                //        Registration reg = new Registration(new Users().getIdByLogin(user.SelectSingleNode("login").InnerText));
                //        //free();
                //        //FormHandle = reg.Handle;
                //        MessageBox.Show(this.Handle.ToString());
                //        reg.Show();
                //    }
                //}


                XmlNodeList _users = new Users().getAllUsers();
                foreach (XmlNode user in _users)
                {
                    if (this.id == Convert.ToInt32(user.Attributes["id"].Value))
                    {
                        string pass = user.SelectSingleNode("password").InnerText;
                        //MessageBox.Show(pass);
                        //MessageBox.Show(strShow);
                        //MessageBox.Show(mach(strShow, pass).ToString());
                        if (mach(strShow, pass) > 75)
                        {

                            //MessageBox.Show("Read!!");
                            //прочитал

                            try
                            {
                                Users user1 = new Users();
                                List<int> documents = user1.getReadOffDocuments(id);
                                string path = Directory.GetCurrentDirectory() + @"\Log\Log.txt";
                                string titleDoc = new Documents().getDocumentById(documents[ird]).SelectSingleNode("@name").Value;
                                if (camList.Count > 0 && camList[0] != "null")
                                {
                                    CloseVideoSource();
                                    user1.docFromReadOffToReadOn(id, documents[ird]);
                                    uploadReadOffDoc(listBox1);
                                    uploadReadOnDoc(listBox2);
                                    using (Graphics g = Graphics.FromImage(pictureBox1.Image))
                                    {
                                        Pen p = new Pen(Color.White);
                                        Font drawFont = new Font("Arial", 20);
                                        SolidBrush drawBrush = new SolidBrush(Color.Red);
                                        g.DrawString(label1.Text, drawFont, drawBrush, new Point(390, 450));
                                        Users u = new Users();
                                        g.DrawString(u.getUsernameById(id), drawFont, drawBrush, new Point(10, 450));
                                    }
                                    string namePhoto = DateTime.Now.Day.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Year.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
                                    pictureBox1.Image.Save(Directory.GetCurrentDirectory() + @"\Log\Images\" + namePhoto + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                                    addLogRecord(path, "ПРОЧИТАЛ ДОКУМЕНТ " + titleDoc, namePhoto);
                                    start();
                                }
                                else
                                {
                                    addLogRecord(path, "ПРОЧИТАЛ ДОКУМЕНТ " + titleDoc, "ФОТО НЕ ПРОИЗВЕДЕНО");
                                    user1.docFromReadOffToReadOn(id, documents[ird]);
                                    uploadReadOffDoc(listBox1);
                                    uploadReadOnDoc(listBox2);
                                }

                            }
                            catch
                            {

                            }
                            menuStrip1.Visible = label1.Visible = listBox2.Visible = listBox1.Visible = button2.Visible = button1.Visible = pictureBox1.Visible = true;
                            axAcroPDF1.Height = listBox1.Height + listBox1.Location.Y - 25;
                            axAcroPDF1.Location = loc;
                            axAcroPDF1.Location = axLoc;
                            this.BackColor = col;
                            axAcroPDF1.LoadFile("none");
                            //axAcroPDF1.Dispose();
                        }
                    }
                }

            }
            else
            {
                base.DefWndProc(ref m);
            }
        }
    }
}
