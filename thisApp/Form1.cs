using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using System.IO;
using System.Text.RegularExpressions;
using iTextSharp.text.pdf;
using iTextSharp.text;
using iTextSharp.text.pdf.parser;

namespace ReadAndVerify
{
    public partial class Form1 : Form
    {
        private bool DeviceExist = false;
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource = null;
        private List<string> camList = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void getCamList(List<string> list, ComboBox cBox)
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
                //comboBox1.SelectedIndex = 0; //make dafault to first cam
            }
            catch (ApplicationException)
            {
                DeviceExist = false;
                list.Add("null");
                cBox.Items.Add("Нет веб-камеры.");
            }
        }

        private void start()
        {
            //if (start.Text == "&Start")
            {
                if (DeviceExist)
                {
                    comboBox1.Text = comboBox1.Items[0].ToString();
                    videoSource = new VideoCaptureDevice(videoDevices[comboBox1.SelectedIndex].MonikerString);
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
        }

        private void CloseVideoSource()
        {
            if (!(videoSource == null))
                if (videoSource.IsRunning)
                {
                    videoSource.SignalToStop();
                    videoSource = null;
                }
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap img = (Bitmap)eventArgs.Frame.Clone();
            //do processing here
            pictureBox1.Image = img;
        }

        string PASSWORD = string.Empty;
        TabControl tabControl;

        private void Form1_Load(object sender, EventArgs e)
        {
            camList = new List<string>();
            getCamList(camList,comboBox1);
            start();
            Admin(false);
            string[] mySett = loadSett();
            PASSWORD = resByKey(find(mySett, "adminPass")); // считываем пароль админа
            string WD = resByKey(find(mySett, "workDir"));         // считываем рабочую диррикторию
            this.WindowState = FormWindowState.Maximized;
            textBox1.Text = WD;
            tabControl = new TabControl();
            tabControl.Location = new Point(10, 10);
            tabControl.Size = new Size(this.Size.Width - 280, 20);
            this.Controls.Add(tabControl);

            List<TabPage> listPage = new List<TabPage>();
            for (int i = 0; i < new DirectoryInfo(WD).GetFiles("*.pdf", System.IO.SearchOption.AllDirectories).Length; i++)
            {
                string headText = new DirectoryInfo(WD).GetFiles("*.pdf", System.IO.SearchOption.AllDirectories).GetValue(i).ToString();
                listPage.Add(new TabPage(headText));
                tabControl.Controls.Add(listPage.Last<TabPage>());

            }
            try
            {
                AxAcroPDFLib.AxAcroPDF AcroPDF = new AxAcroPDFLib.AxAcroPDF();
                AcroPDF.Size = new Size(this.Width - 280, this.Size.Height - 100);
                AcroPDF.Location = new Point(10, 40);
                this.Controls.Add(AcroPDF);
                AcroPDF.LoadFile(WD + "/" + listPage.First<TabPage>().Text);
                AcroPDF.Show();

                tabControl.Selected += (s, ed) =>
                {
                    AcroPDF.LoadFile(WD + "/" + ed.TabPage.Text);
                    AcroPDF.Show();
                };
            }

            catch
            {
                MessageBox.Show("В указанной рабочей папке нет *.pdf файлов.");
            }

            string path = Directory.GetCurrentDirectory().ToString() + @"\Log\Users.txt";
            if (File.Exists(path))
            {
                string[] readText = File.ReadAllLines(path);
                foreach (string s in readText)
                {
                    Char delimiter = ',';
                    comboBox2.Items.Add(s.Split(delimiter)[0]);
                }
            }
            else
            {
                MessageBox.Show("Создайте файл Users");
            }
            //AcroPDFLib.AcroPDFClass a = new AcroPDFLib.AcroPDFClass();


        }

        public int getNumberOfPdfPages(string fileName)
        {
            using (StreamReader sr = new StreamReader(File.OpenRead(fileName)))
            {
                Regex regex = new Regex(@"/Type\s*/Page[^s]");
                MatchCollection matches = regex.Matches(sr.ReadToEnd());

                return matches.Count;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            videoSource.DisplayPropertyPage(this.Handle);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            // При закрытии формы проверяем: если устройство (веб-камера) работает, то выключаем его.
            if (videoSource.IsRunning)
            {
                CloseVideoSource();
            }
        }

        // Сохранение фото
        private void button2_Click(object sender, EventArgs e)
        {
            //pictureBox1.Image.Save(@"D:\newIm.jpg");
            if (comboBox2.Text == string.Empty)
            {
                string FIO = Microsoft.VisualBasic.Interaction.InputBox("Введите Вашы ФИО :");
                if (FIO == "")
                {
                    MessageBox.Show("Введите имя и повоторите попытку.");
                    return;
                }
                foreach (string item in comboBox2.Items)
                {
                    if (item == FIO)
                    {
                        MessageBox.Show("Вы уже есть в списке. \nВыберите в нем себя и повторите попытку.");
                        return;
                    }
                }
                comboBox2.Items.Add(FIO);
                comboBox2.SelectedIndex = comboBox2.Items.Count - 1;
                using (StreamWriter sw = File.AppendText(Directory.GetCurrentDirectory() + @"\Log\Users.txt"))
                {
                    sw.WriteLine(comboBox2.Items[comboBox2.SelectedIndex]);
                }
            }
            int temper = 0;
            while(true)
            {
                temper = new Random().Next(100000000, 999999999);
                //temper = new Random().Next(0, 9);
                if (!File.Exists(Directory.GetCurrentDirectory() + @"\Log\Images\" + temper.ToString() + ".jpg")) break;
            }
            using (StreamWriter sw = File.AppendText(Directory.GetCurrentDirectory() + @"\Log\Log.txt"))
            {
                sw.WriteLine(comboBox2.Items[comboBox2.SelectedIndex] + ", " + DateTime.Now + ", " + temper + ".jpg, "+ tabControl.SelectedTab.Text);
            }
            comboBox2.Text = string.Empty;
            pictureBox1.Image.Save(Directory.GetCurrentDirectory() + @"\Log\Images\" + temper + ".jpg");
            //pictureBox1.Image.Save(Directory.GetCurrentDirectory() + @"\Log\Images\1.jpg");
        }

        private bool validation()
        {
            return Microsoft.VisualBasic.Interaction.InputBox("Введите пароль: ") == PASSWORD ? true : false;
        }

        private void Admin(bool state)
        {
            button1.Visible = state;
            comboBox1.Visible = state;
            label1.Visible = state;
            textBox1.Visible = state;
            button3.Visible = state;
            button5.Visible = state;
            button6.Visible = state;
        }

        bool adm = false;
        private void button4_Click_1(object sender, EventArgs e)
        {
            if (!adm)
            {
                if (!validation()) return;
                Admin(true);
                button4.Text = "Сохранить";
                adm = true;
            }
            else
            {
                Admin(false);
                button4.Text = "Включить админку";
                adm = false;
            }
        }

        // Читаем настройки
        string[] loadSett()
        {
            string path = Directory.GetCurrentDirectory().ToString() + @"\settings.txt";
            if (File.Exists(path))
            {
                string[] readText = File.ReadAllLines(path);
                string[] res = new string[2];
                int i = 0;
                foreach (string s in readText)
                {
                    res[i] = s;
                    i++;
                }
                return res;
            }
            return new string[1] { "false" };
        }

        // Читаем значение по строке
        string resByKey(string s)
        {
            char ch = '=';
            return s.Substring(s.IndexOf(ch) + 2);
        }

        string find(string[] strings,string finding)
        {
            foreach (string str in strings)
            {
                if (str.IndexOf(finding) == 0) return str;
            }
            return "null";
        }

        void setKeyByName(string name, string value)
        {
            string[] set = loadSett();
            for (int i = 0; i< set.Length;i++)
            {
                if (set[i].IndexOf(name) == 0)
                {
                    set[i] = name + " = " + value;
                }
            }
            string path = Directory.GetCurrentDirectory().ToString() + @"\settings.txt";
            using (StreamWriter sw = new StreamWriter(path))
            {
                foreach (var s in set)
                {
                    sw.WriteLine(s);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Выберите  рабочую папку:";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = dlg.SelectedPath;
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(textBox1.Text)) setKeyByName("workDir", textBox1.Text);
            else MessageBox.Show("Выбранной папки не существует, укажите корректное имя папки.");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (validation())
            {
                string temp = Microsoft.VisualBasic.Interaction.InputBox("Введите новый пароль: ");
                if ((temp == Microsoft.VisualBasic.Interaction.InputBox("Повторите новый пароль: ")))
                {
                    setKeyByName("adminPass", temp);
                }
                else MessageBox.Show("Пароли не совпадают. Повторите попытку.");
            }
            else MessageBox.Show("Не верный пароль.");
        }
    }
}
