using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using libzkfpcsharp;
//using Sample;
using System.Drawing.Imaging;
using System.Xml;

namespace ReadAndVerify
{
    public partial class zkt : Form
    {
        public static string id;
        public static bool zktFree = true;
        public IntPtr mDevHandle = IntPtr.Zero;
        public IntPtr mDBHandle = IntPtr.Zero;
        public static IntPtr FormHandle = IntPtr.Zero;
        public bool bIsTimeToDie = false;
        public bool IsRegister = false;
        public bool bIdentify = true;
        public byte[] FPBuffer;
        public int RegisterCount = 0;
        public const int REGISTER_FINGER_COUNT = 3;

        public byte[][] RegTmps = new byte[3][];
        public byte[] RegTmp = new byte[2048];
        public byte[] CapTmp = new byte[2048];

        public int cbCapTmp = 2048;
        public int cbRegTmp = 0;
        public int iFid = 1;

        public int mfpWidth = 0;
        public int mfpHeight = 0;
        public int mfpDpi = 0;

        public const int MESSAGE_CAPTURED_OK = 0x0400 + 6;

        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        TextBox textRes = new TextBox();
        Bitmap picFPImg;
        ComboBox cmbIdx = new ComboBox();

        protected void DoCapture()
        {
            while (!bIsTimeToDie)
            {
                cbCapTmp = 2048;
                int ret = 0;
                try
                {
                    ret = zkfp2.AcquireFingerprint(mDevHandle, FPBuffer, CapTmp, ref cbCapTmp);
                }
                catch
                {
                    return;
                }
                if (ret == zkfp.ZKFP_ERR_OK)
                {
                    SendMessage(FormHandle, MESSAGE_CAPTURED_OK, IntPtr.Zero, IntPtr.Zero);
                }
                Thread.Sleep(200);
            }
        }

        protected void free()
        {
            if (zktFree) return;
            zktFree = true;
            zkfp2.Terminate();
            cbRegTmp = 0;
        }

        protected void bnOpen_Click()
        {
            zktFree = false;
            int ret = zkfp.ZKFP_ERR_OK;
            if (IntPtr.Zero == (mDevHandle = zkfp2.OpenDevice(cmbIdx.SelectedIndex)))
            {
                MessageBox.Show("OpenDevice fail");
                return;
            }
            if (IntPtr.Zero == (mDBHandle = zkfp2.DBInit()))
            {
                MessageBox.Show("Init DB fail");
                zkfp2.CloseDevice(mDevHandle);
                mDevHandle = IntPtr.Zero;
                return;
            }
            RegisterCount = 0;
            cbRegTmp = 0;
            iFid = 1;
            for (int i = 0; i < 3; i++)
            {
                RegTmps[i] = new byte[2048];
            }
            byte[] paramValue = new byte[4];
            int size = 4;
            zkfp2.GetParameters(mDevHandle, 1, paramValue, ref size);
            zkfp2.ByteArray2Int(paramValue, ref mfpWidth);

            size = 4;
            zkfp2.GetParameters(mDevHandle, 2, paramValue, ref size);
            zkfp2.ByteArray2Int(paramValue, ref mfpHeight);

            FPBuffer = new byte[mfpWidth * mfpHeight];

            size = 4;
            zkfp2.GetParameters(mDevHandle, 3, paramValue, ref size);
            zkfp2.ByteArray2Int(paramValue, ref mfpDpi);

            textRes.AppendText("reader parameter, image width:" + mfpWidth + ", height:" + mfpHeight + ", dpi:" + mfpDpi + "\n");

            Thread captureThread = new Thread(new ThreadStart(DoCapture));
            captureThread.IsBackground = true;
            captureThread.Start();
            bIsTimeToDie = false;
            textRes.AppendText("Open succ\n");

        }

        protected void bnInit_Click()
        {
            cmbIdx.Items.Clear();
            int ret = zkfperrdef.ZKFP_ERR_OK;
            if ((ret = zkfp2.Init()) == zkfperrdef.ZKFP_ERR_OK)
            {
                int nCount = zkfp2.GetDeviceCount();
                if (nCount > 0)
                {
                    for (int i = 0; i < nCount; i++)
                    {
                        cmbIdx.Items.Add(i.ToString());
                    }
                    cmbIdx.SelectedIndex = 0;
                }
                else
                {
                    zkfp2.Terminate();
                    MessageBox.Show("No device connected!");
                }
            }
            else
            {
                MessageBox.Show("Initialize fail, ret=" + ret + " !");
            }
        }
        protected string stri = string.Empty;
        //protected override void DefWndProc(ref Message m)
        //{
        //    switch (m.Msg)
        //    {
        //        case MESSAGE_CAPTURED_OK:
        //            {
        //                MemoryStream ms = new MemoryStream();
        //                BitmapFormat.GetBitmap(FPBuffer, mfpWidth, mfpHeight, ref ms);
        //                Bitmap bmp = new Bitmap(ms);
        //                this.picFPImg = new Bitmap(bmp);


        //                String strShow = zkfp2.BlobToBase64(CapTmp, cbCapTmp);
        //                textRes.AppendText("capture template data:" + strShow + "\n");
        //                //MessageBox.Show("capture template data:" + strShow + "\n");




        //                //XmlNodeList _users = new Users().getAllUsers();
        //                //foreach (XmlNode user in _users)
        //                //{
        //                //    string pass = user.SelectSingleNode("password").InnerText;
        //                //    if (mach(strShow, pass) > 75)
        //                //    {
        //                //        MessageBox.Show(user.SelectSingleNode("username").InnerText);
        //                //        break;
        //                //    }
                            
        //                //}




                        
        //                // сравнивать 

        //                if (IsRegister)
        //                {
        //                    int ret = zkfp.ZKFP_ERR_OK;
        //                    int fid = 0, score = 0;
        //                    ret = zkfp2.DBIdentify(mDBHandle, CapTmp, ref fid, ref score);
        //                    if (zkfp.ZKFP_ERR_OK == ret)
        //                    {
        //                        textRes.AppendText("This finger was already register by " + fid + "!\n");
        //                        return;
        //                    }

        //                    if (RegisterCount > 0 && zkfp2.DBMatch(mDBHandle, CapTmp, RegTmps[RegisterCount - 1]) <= 0)
        //                    {
        //                        textRes.AppendText("Please press the same finger 3 times for the enrollment.\n");
        //                        return;
        //                    }

        //                    Array.Copy(CapTmp, RegTmps[RegisterCount], cbCapTmp);
        //                    String strBase64 = zkfp2.BlobToBase64(CapTmp, cbCapTmp);
        //                    byte[] blob = zkfp2.Base64ToBlob(strBase64);
        //                    RegisterCount++;
        //                    if (RegisterCount >= REGISTER_FINGER_COUNT)
        //                    {
        //                        RegisterCount = 0;
        //                        if (zkfp.ZKFP_ERR_OK == (ret = zkfp2.DBMerge(mDBHandle, RegTmps[0], RegTmps[1], RegTmps[2], RegTmp, ref cbRegTmp)) &&
        //                               zkfp.ZKFP_ERR_OK == (ret = zkfp2.DBAdd(mDBHandle, iFid, RegTmp)))
        //                        {
        //                            iFid++;
        //                            stri = strBase64;
        //                            textRes.AppendText("enroll succ\n");
        //                        }
        //                        else
        //                        {
        //                            textRes.AppendText("enroll fail, error code=" + ret + "\n");
        //                        }
        //                        IsRegister = false;
        //                        return;
        //                    }
        //                    else
        //                    {
        //                        textRes.AppendText("You need to press the " + (REGISTER_FINGER_COUNT - RegisterCount) + " times fingerprint\n");
        //                    }
        //                }
        //                else
        //                {
        //                    if (cbRegTmp <= 0)
        //                    {
        //                        textRes.AppendText("Please register your finger first!\n");
        //                        return;
        //                    }
        //                    if (bIdentify)
        //                    {
        //                        int ret = zkfp.ZKFP_ERR_OK;
        //                        int fid = 0, score = 0;
        //                        ret = zkfp2.DBIdentify(mDBHandle, CapTmp, ref fid, ref score);
        //                        if (zkfp.ZKFP_ERR_OK == ret)
        //                        {
        //                            textRes.AppendText("Identify succ, fid= " + fid + ",score=" + score + "!\n");
        //                            return;
        //                        }
        //                        else
        //                        {
        //                            textRes.AppendText("Identify fail, ret= " + ret + "\n");
        //                            return;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        int ret = zkfp2.DBMatch(mDBHandle, CapTmp, RegTmp);
        //                        if (0 < ret)
        //                        {
        //                            textRes.AppendText("Match finger succ, score=" + ret + "!\n");
        //                            return;
        //                        }
        //                        else
        //                        {
        //                            textRes.AppendText("Match finger fail, ret= " + ret + "\n");
        //                            return;
        //                        }
        //                    }
        //                }
        //            }
        //            break;

        //        default:
        //            base.DefWndProc(ref m);
        //            break;
        //    }
        //}
        protected int mach(string str1, string str2)
        {
            byte[] blob1 = Convert.FromBase64String(str1.Trim());
            byte[] blob2 = Convert.FromBase64String(str2.Trim());

            return zkfp2.DBMatch(mDBHandle, blob1, blob2);
        }

        protected void bnEnroll_Click()
        {
            if (!IsRegister)
            {
                IsRegister = true;
                RegisterCount = 0;
                cbRegTmp = 0;
                MessageBox.Show("Please press your finger 3 times!\n");
            }
        }
    }
}


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Windows.Forms;
//using libzkfpcsharp;
//using System.Runtime.InteropServices;
//using System.Threading;
//using System.IO;
//using System.Drawing;
//using System.Xml;

//namespace ReadAndVerify
//{
//    public partial class zkt : Form
//    {

//        public ComboBox cmbIdx;
//        public TextBox textRes;
//        public PictureBox picFPImg;
//        public TextBox txtTemplate1;
//        public TextBox txtTemplate2;


//        IntPtr mDevHandle = IntPtr.Zero;
//        IntPtr mDBHandle = IntPtr.Zero;
//        IntPtr FormHandle = IntPtr.Zero;
//        bool bIsTimeToDie = false;
//        bool IsRegister = false;
//        bool bIdentify = true;
//        byte[] FPBuffer;
//        int RegisterCount = 0;
//        const int REGISTER_FINGER_COUNT = 3;

//        byte[][] RegTmps = new byte[3][];
//        byte[] RegTmp = new byte[2048];
//        byte[] CapTmp = new byte[2048];

//        int cbCapTmp = 2048;
//        int cbRegTmp = 0;
//        int iFid = 1;

//        private int mfpWidth = 0;
//        private int mfpHeight = 0;
//        private int mfpDpi = 0;

//        const int MESSAGE_CAPTURED_OK = 0x0400 + 6;

//        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
//        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

//        //public zkt()
//        //{
//        //}

//        public zkt(IntPtr FormHandle)
//        {
//            this.FormHandle = FormHandle;
//        }

//        public void init()
//        {
//            cmbIdx = new ComboBox();
//            cmbIdx.Items.Clear();
//            int ret = zkfperrdef.ZKFP_ERR_OK;
//            if ((ret = zkfp2.Init()) == zkfperrdef.ZKFP_ERR_OK)
//            {
//                int nCount = zkfp2.GetDeviceCount();
//                if (nCount > 0)
//                {
//                    for (int i = 0; i < nCount; i++)
//                    {
//                        cmbIdx.Items.Add(i.ToString());
//                    }
//                    cmbIdx.SelectedIndex = 0;
//                    //bnInit.Enabled = false;
//                    //bnFree.Enabled = true;
//                    //bnOpen.Enabled = true;
//                }
//                else
//                {
//                    zkfp2.Terminate();
//                    MessageBox.Show("No device connected!");
//                }
//            }
//            else
//            {
//                MessageBox.Show("Initialize fail, ret=" + ret + " !");
//            }
//        }

//        public void free()
//        {
//            zkfp2.Terminate();
//            cbRegTmp = 0;
//            //bnInit.Enabled = true;
//            //bnFree.Enabled = false;
//            //bnOpen.Enabled = false;
//            //bnClose.Enabled = false;
//            //bnEnroll.Enabled = false;
//            //bnVerify.Enabled = false;
//            //bnIdentify.Enabled = false;
//            //btMatch.Enabled = false;
//        }

//        public void open()
//        {
//            textRes = new TextBox();
//            int ret = zkfp.ZKFP_ERR_OK;
//            if (IntPtr.Zero == (mDevHandle = zkfp2.OpenDevice(cmbIdx.SelectedIndex)))
//            {
//                MessageBox.Show("OpenDevice fail");
//                return;
//            }
//            if (IntPtr.Zero == (mDBHandle = zkfp2.DBInit()))
//            {
//                MessageBox.Show("Init DB fail");
//                zkfp2.CloseDevice(mDevHandle);
//                mDevHandle = IntPtr.Zero;
//                return;
//            }
//            //bnInit.Enabled = false;
//            //bnFree.Enabled = true;
//            //bnOpen.Enabled = false;
//            //bnClose.Enabled = true;
//            //bnEnroll.Enabled = true;
//            //bnVerify.Enabled = true;
//            //bnIdentify.Enabled = true;
//            //btnOutput.Enabled = true;
//            //btMatch.Enabled = true;
//            //btnImport.Enabled = true;
//            RegisterCount = 0;
//            cbRegTmp = 0;
//            iFid = 1;
//            for (int i = 0; i < 3; i++)
//            {
//                RegTmps[i] = new byte[2048];
//            }
//            byte[] paramValue = new byte[4];
//            int size = 4;
//            zkfp2.GetParameters(mDevHandle, 1, paramValue, ref size);
//            zkfp2.ByteArray2Int(paramValue, ref mfpWidth);

//            size = 4;
//            zkfp2.GetParameters(mDevHandle, 2, paramValue, ref size);
//            zkfp2.ByteArray2Int(paramValue, ref mfpHeight);

//            FPBuffer = new byte[mfpWidth * mfpHeight];

//            size = 4;
//            zkfp2.GetParameters(mDevHandle, 3, paramValue, ref size);
//            zkfp2.ByteArray2Int(paramValue, ref mfpDpi);

//            textRes.AppendText("reader parameter, image width:" + mfpWidth + ", height:" + mfpHeight + ", dpi:" + mfpDpi + "\n");

//            Thread captureThread = new Thread(new ThreadStart(DoCapture));
//            captureThread.IsBackground = true;
//            captureThread.Start();
//            bIsTimeToDie = false;
//            textRes.AppendText("Open succ\n");

//        }

//        public void DoCapture()
//        {
//            while (!bIsTimeToDie)
//            {
//                cbCapTmp = 2048;
//                int ret = zkfp2.AcquireFingerprint(mDevHandle, FPBuffer, CapTmp, ref cbCapTmp);
//                if (ret == zkfp.ZKFP_ERR_OK)
//                {
//                    SendMessage(FormHandle, MESSAGE_CAPTURED_OK, IntPtr.Zero, IntPtr.Zero);
//                }
//                Thread.Sleep(200);
//            }
//        }

//        protected override void DefWndProc(ref Message m)
//        {
//            if (m.Msg == MESSAGE_CAPTURED_OK) MessageBox.Show("yes");
//            switch (m.Msg)
//            {
//                case MESSAGE_CAPTURED_OK:
//                    {
//                        picFPImg = new PictureBox();
//                        MemoryStream ms = new MemoryStream();
//                        BitmapFormat.GetBitmap(FPBuffer, mfpWidth, mfpHeight, ref ms);
//                        Bitmap bmp = new Bitmap(ms);
//                        this.picFPImg.Image = bmp;


//                        String strShow = zkfp2.BlobToBase64(CapTmp, cbCapTmp);
//                        textRes.AppendText("capture template data:" + strShow + "\n");

//                        // ищем strShow для идентификации

//                        //XmlNodeList u = new Users().getAllUsers();
//                        //foreach(XmlNode user in u)
//                        //{

//                        //    MessageBox.Show(user.SelectSingleNode("User/password").Value);
//                        //}
//                        //mach(new Users().)
//                        MessageBox.Show(strShow);
//                        if (IsRegister)
//                        {
//                            int ret = zkfp.ZKFP_ERR_OK;
//                            int fid = 0, score = 0;
//                            ret = zkfp2.DBIdentify(mDBHandle, CapTmp, ref fid, ref score);
//                            if (zkfp.ZKFP_ERR_OK == ret)
//                            {
//                                textRes.AppendText("This finger was already register by " + fid + "!\n");
//                                return;
//                            }

//                            if (RegisterCount > 0 && zkfp2.DBMatch(mDBHandle, CapTmp, RegTmps[RegisterCount - 1]) <= 0)
//                            {
//                                textRes.AppendText("Please press the same finger 3 times for the enrollment.\n");
//                                return;
//                            }

//                            Array.Copy(CapTmp, RegTmps[RegisterCount], cbCapTmp);
//                            String strBase64 = zkfp2.BlobToBase64(CapTmp, cbCapTmp);
//                            byte[] blob = zkfp2.Base64ToBlob(strBase64);
//                            RegisterCount++;
//                            if (RegisterCount >= REGISTER_FINGER_COUNT)
//                            {
//                                RegisterCount = 0;
//                                if (zkfp.ZKFP_ERR_OK == (ret = zkfp2.DBMerge(mDBHandle, RegTmps[0], RegTmps[1], RegTmps[2], RegTmp, ref cbRegTmp)) &&
//                                       zkfp.ZKFP_ERR_OK == (ret = zkfp2.DBAdd(mDBHandle, iFid, RegTmp)))
//                                {
//                                    iFid++;
//                                    textRes.AppendText("enroll succ\n");
//                                }
//                                else
//                                {
//                                    textRes.AppendText("enroll fail, error code=" + ret + "\n");
//                                }
//                                IsRegister = false;
//                                return;
//                            }
//                            else
//                            {
//                                textRes.AppendText("You need to press the " + (REGISTER_FINGER_COUNT - RegisterCount) + " times fingerprint\n");
//                            }
//                        }
//                        else
//                        {
//                            if (cbRegTmp <= 0)
//                            {
//                                textRes.AppendText("Please register your finger first!\n");
//                                return;
//                            }
//                            if (bIdentify)
//                            {
//                                int ret = zkfp.ZKFP_ERR_OK;
//                                int fid = 0, score = 0;
//                                ret = zkfp2.DBIdentify(mDBHandle, CapTmp, ref fid, ref score);
//                                if (zkfp.ZKFP_ERR_OK == ret)
//                                {
//                                    textRes.AppendText("Identify succ, fid= " + fid + ",score=" + score + "!\n");
//                                    return;
//                                }
//                                else
//                                {
//                                    textRes.AppendText("Identify fail, ret= " + ret + "\n");
//                                    return;
//                                }
//                            }
//                            else
//                            {
//                                int ret = zkfp2.DBMatch(mDBHandle, CapTmp, RegTmp);
//                                if (0 < ret)
//                                {
//                                    textRes.AppendText("Match finger succ, score=" + ret + "!\n");
//                                    return;
//                                }
//                                else
//                                {
//                                    textRes.AppendText("Match finger fail, ret= " + ret + "\n");
//                                    return;
//                                }
//                            }
//                        }
//                    }
//                    break;

//                default:
//                    base.DefWndProc(ref m);
//                    break;
//            }
//        }

//        public void identify()
//        {
//            if (!bIdentify)
//            {
//                bIdentify = true;
//                textRes.AppendText("Please press your finger!\n");
//            }
//        }

//        public void verify()
//        {
//            if (bIdentify)
//            {
//                bIdentify = false;
//                textRes.AppendText("Please press your finger!\n");
//            }
//        }

//        public void mach(string str1, string str2)
//        {
//            txtTemplate1 = new TextBox();
//            txtTemplate2 = new TextBox();
//            txtTemplate1.Text = str1;
//            txtTemplate2.Text = str2;
//            //txtTemplate1.Text = "TElTUzIxAAAFCgoECAUHCc7QAAAlC3YBAAAAhDc1tgrLABQHygBnAAIHiQCWAOgNawD9CkwLiACGACIObArLANANYwB8AFYH+QCSABEPJAAECz8KZgDzABUPcAoKAdINbwDKAVYHRACUANsLXABGCmwPdwBHAKMPago7AOoP9gD7AfsFmgAbAPENeACuCgwN2ACkALgN4QqyACINcAB8AFUHmwB6AGgNIgDhCqMPwQBwAL8P6Ar0ACkOhQDOAVYHAgHVAKINjwDHCtkP3wBeAMkP+AoNAagJ+gDSAZMDZQAlAdQNwgEvC38LpgAlADwNqwoSAHINnwAdAPQNvACKAHMNHQDnCicPfgDjABcP0Qr0ADgP2wBEAIEFjAB0AO0NxgHGCiMNCgG8AOYG3AoQAbgHqwCTAGoFEAHpACALvABXCu0PdgAnARYOUwpUAOUPgwCBAdMFuQBPAd0PECY/AofzgYzh/OCXXApzgGL94l33Fqf12ABl/YGCoH2Eiz/4LAdhhWR/A4fflcbyJRQkCZCD2AOZA+V/3YKoiuwDFIIlAkSC5g7KiMeONQMjdJcDVX/FfBb5BITMCfcFYYNKao5ZqPKAgdKGXQisk8fyiHgpXXlzwKXzFnMBsP+BgW96rHaAgnKAZXzUeURkTAse+KoNfOwMg9jyrYClAkeAmIsEhqmDnYdzCKeA45W6if+d4ReEiWtstPMRGk9YtAuAgoYEqeqkg7PSJAhZidqMpHl4i2IFed4CIXul0PE4bwLwYYRg8Xz1GAdSBKLhrP0UH0gSlgxyENd1oI7gCdoFQANkgfcLvQOpg+oCWH2EidMltQc29aMKDIjX+rZ6TIuABxcDMAFhgm0DX/mLi3v9WgROB3L6KgCa43sn9vy7f3j1HiBGAQJp2TwDCnkAZsBkCMWOAHqA/4cQAN/NhsXIiWR7wVsJxZAPY2PBwGwEAd8RRsvABQC0E/fsBAWcHm1wBgDr5YmDywQAoSdwc8IArCL7REYLAP3xiX1giggAnUH0hVQGCnJGacEJAFJHaMtwwHEJAHaOYFdm/hIBC1CPBMLFcn1+wcBbD8WqUX7DwmZiwMIEfQ0Ks1X3/z4/wABXUGPC/8YMAG9aaMqBYsB5BAAnXAYwBgDaXYOMrAUF6GMPSggAx7QGNcg3DACQdfGK/vvKQv4IAIl4pWn6XQYA2X2GhFIFBeuCFsI8BwAchIbKwMR9DAC8TXqWyIbAfAUAhE5exGcJAMSLBsA7/lABAbyOdMLBBJTEysEEAPyUGgb9BgqEmlrBAwCEm1LICwDWoYONB3zFDgHepxMvCMXArAP//v7/hArFuKh7woDAagsABbAJ9PxFUv4EAC21GzIHAF+7WnsFBwVuvFDAwFQDxA3FKv8EAEjHUKMEBA7GKX4ZAR8PpILKg8GXwcJpBf/E9EcDALbLCTkIBW3OUMH/RsLNAJvc3f38+/w+wACn0+r3//wEAF7eVmYJANflK8AE/cHP/g0AtfFAOv479f7/QAUAZDNQOw0BrfxMTD3NEOcONv76wEAU1XkJ3cPA/////jv7+Pb/wP7Bwf8EwAAadRFMKAYQHxZVNMEHEQQqgwbH+kwFEHMtWkTNERE+h59bEhCCh9PFXP7A+/D7/zr9+vVHBBB/SWKRBRQDVYzGw8EY1Zxd7sDAiMLBwwfEwsPDwMHAwP4E/cXKCBEfWaTDAMLGzjIEEQdafQH/";
//            //txtTemplate2.Text = "THlTUzIxAAAFOjgECAUHCc7QAAAlO3YBAAAAhec1fjqxAA0NeAAYAEk9ggCRAAsOdwDNOqEOUADwAJcNPDqgAE4NTgC6AOA0MQDZAM8OFwCpOh8L2gDJAOUHpzpkAIIPhgCTAHI1MwAMAdEMFgAJO4YIjgA9ASYPrTpCAesPGgCEAOE1EABCAc0EugDMOlQJoQDFAOcNTTrLANIPrAAmADI3rQCVAB8OWAD6OsIKgwBvALUPvTr3ADYITQCrAOM20gCdACALGACxOqAHEwCsABMPSjorAdYPwgDlAfg3ogBCAAoPtQBTO+sKFAA3ARcHbToMAPcPaAB6AOo1nwDYADoNVACMOgcNtgDWAOMOoTqIAIANMgB2AMs3ywC3AKIK7gCnOlMNxwDtAGMIuDp1ABAOQQDLAdE2UgBbAO0LQwA3O90ObQA+AKgNZzo8AeALOAD2AGAzMgAlAOkPHCKogD8EAfniXfcWZD7DAG6NFXysgEBRg4HmgsqGdPyEu6t6WQBi+TeamMV3As5+fXx4fCA6JIEFghYE03STNnMUTQoe9G8NsMaTgtL/VY9r4JNrRAip+wL65AsvKHPzjoB6/mr7ecWK/6f7eQl4auBwZISl6Xn/3AdXPqPlJRDl/bACDJr3GDYRsY9XjIew1Po+bAbw8JDwOLx32QBp+Vp6oD6Qgq2DeYScASC7nIe2CKKKCPtfPk8HYXs6yxsXv7PflfueQQ+/Cmsp+QeLAaf/Aoz+Ke6XdeahnsMdZD8MGUUW5fy8iSOiEIi18Nnu7GCER+dwZX2Bgfv4vL7YAQGGLQSkhaRFRHCBg2lzw5DTqsfxMQTaAEeF5DPbCXKCzfjf/r/IN/lygtN4hA/IwZcGzXyKgIJ4zL73Bof39F/OZbrFviBRAQKZ2JsAOi8AacF3B8VABVeGbwkAUgCueGH4BwBsAHCBBQUFnQOGgwYArs6DffoEAGQPcG3BAR81QoYLALgWQ8Jc+8HCwf8IAHghhlHAfBEAxjBJh3v7isFq/wcA/DZbxcFwBwDJO0PAcfgCAGw8ccLCADoFY2rAwQQAYEEFCwIAbEJrwMIAmHiBhsHBBADTRmW3BAClRgzAOsAOOklSZG//fKoJBXRSXsBzwmbGAIFufMMHAIxWxv/4XgIAhFl3wc4AU2fxwv3+//84wF4yAU9fXmttzQCaWIfCeYgFAGJnFlcGAJ9og3lNDAW4b3fDdcNapQkFsHEDMMD/TsAAT0lbwML/CwBHc3W6hsFbBAC/sxZtMAGghobEeQfCYz4BqIwQKwfFhoo5//39aAkAu5R1/n3AawwAhlAJL/pZwGAEAK9dHMXEGQDQmJduBMOUssHAw/5VQgcEBe+eJnMIAChmV8X4TP0JAC2llWRB+AIAEbJQ/8EAeIn8+v0IADB8UG1uCQBnvdz+PvwnPwFrweb4IcEAYf9RZQgAocnswG/+wAgAes1TnP9WOQHazSbBA8Wc3w78CACi2jA4/DPFBAAu31D/hAcFQt5JQzYIAGjmMcf6wP5GAwCW80nHFABC9NrDOsH7xf79/Pv+/jr/xMXBwWAFELbLWkM+ETATXGQF1T4RbU0IEOcXiQfEx0wHEAkbQGI4AxXZHobDChDf4n36/sJeXQUQTOpixPr/BBDWMHpTCxXvOG1Vwjc9zBBnAN90wsDF/soAeuI8KUb//sGB/hwqZD/piHqRAMPO+cPAwMDA/Qf+Yz4R/0AT//3GEIx7Z8AFEF5Cof9UPxH1RB7AR80Qz3JqWsA/BBCnUWwCBRBrWmYm";
//            byte[] blob1 = Convert.FromBase64String(txtTemplate1.Text.Trim());
//            byte[] blob2 = Convert.FromBase64String(txtTemplate2.Text.Trim());

//            int ret = zkfp2.DBMatch(mDBHandle, blob1, blob2);
//            //textRes.AppendText("Match template 1 vs template 2 score=" + ret + "!\n");
//            MessageBox.Show("Match template 1 vs template 2 score=" + ret.ToString());
//        }

//        private void InitializeComponent()
//        {
//            this.SuspendLayout();
//            // 
//            // zkt
//            // 
//            this.ClientSize = new System.Drawing.Size(453, 208);
//            this.Name = "zkt";
//            this.ResumeLayout(false);

//        }
    
//    }
//}

