using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using libzkfpcsharp;

using System.Linq;
namespace ReadAndVerify
{
    public partial class createUser : zkt
    {
        public createUser()
        {

            InitializeComponent();
        }

        private void createUser_Load(object sender, EventArgs e)
        {
            free();
            bnInit_Click();
            bnOpen_Click();
            FormHandle = this.Handle;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bnEnroll_Click();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox2.Text.Length <= 0)
            {
                MessageBox.Show("Введите пароль");
                return;
            }
            new Users().createUser(textBox3.Text, textBox1.Text, textBox2.Text, new int[] { 1 });
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        protected override void DefWndProc(ref Message m)
        {
            if (m.Msg == MESSAGE_CAPTURED_OK)
            {
                if (IsRegister)
                {
                    //DoCapture();
                    int ret = zkfp.ZKFP_ERR_OK;
                    int fid = 0, score = 0;
                    ret = zkfp2.DBIdentify(mDBHandle, CapTmp, ref fid, ref score);
                    if (zkfp.ZKFP_ERR_OK == ret)
                    {
                        MessageBox.Show("This finger was already register by " + fid + "!\n");
                        return;
                    }

                    //if (RegisterCount > 0 && zkfp2.DBMatch(mDBHandle, CapTmp, RegTmps[RegisterCount - 1]) <= 0)
                    //{
                    //    MessageBox.Show(String.Join(" ", CapTmp.ToString()));
                    //    MessageBox.Show(String.Join(" ", RegTmps[RegisterCount - 1].ToString()));
                    //    MessageBox.Show("Please press the same finger 3 times for the enrollment.\n");
                    //    return;
                    //}
                    var s = from x in CapTmp where x > 0 select x;
                    int refTTT = 0;
                    foreach (byte sss in s)
                    {
                        refTTT++;
                    }
                    //Array.Copy(CapTmp, RegTmps[RegisterCount], cbCapTmp);
                    Array.Copy(CapTmp, RegTmps[RegisterCount], refTTT);
                    String strBase64 = zkfp2.BlobToBase64(CapTmp, cbCapTmp);
                    byte[] blob = zkfp2.Base64ToBlob(strBase64);
                    //RegisterCount++;
                    textBox2.Text = strBase64;
                    //if (RegisterCount >= REGISTER_FINGER_COUNT)
                    //{
                    //    RegisterCount = 0;
                    //    if (zkfp.ZKFP_ERR_OK == (ret = zkfp2.DBMerge(mDBHandle, RegTmps[0], RegTmps[1], RegTmps[2], RegTmp, ref cbRegTmp)) &&
                    //            zkfp.ZKFP_ERR_OK == (ret = zkfp2.DBAdd(mDBHandle, iFid, RegTmp)))
                    //    {
                    //        iFid++;
                    //        textBox2.Text = strBase64;
                    //        MessageBox.Show("enroll succ\n");
                    //    }
                    //    else
                    //    {
                    //        MessageBox.Show(String.Join(" ", CapTmp.ToString()));
                    //        MessageBox.Show("enroll fail, error code=" + ret + "\n");
                    //    }
                    //    IsRegister = false;
                    //    return;
                    //}
                    //else
                    //{
                    //    MessageBox.Show("You need to press the " + (REGISTER_FINGER_COUNT - RegisterCount) + " times fingerprint\n");
                    //}
                }
            }
            else
            {
                base.DefWndProc(ref m);
            }
        }

        private void createUser_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        private void createUser_FormClosed(object sender, FormClosedEventArgs e)
        {
            free();
        }
    }
}
