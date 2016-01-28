using Google.Apis.YouTube.v3.Data;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasyMusicBot
{
    public partial class Form1 : Form
    {
        Program p;
        public Form1(Program p)
        {
            InitializeComponent();
            this.Hide();
            this.p = p;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Looper();
        }

        private void AxWindowsMediaPlayer1_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            if (e.newState == 1)
            {
                //this.Close();
                //System.Threading.Thread.CurrentThread.);
                //MessageBox.Show("done!");
            }
            //throw new NotImplementedException();
        }
        async Task PutTaskDelay(int delay)
        {
            await Task.Delay(delay);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            p.SendCmd(textBox1.Text);
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.Enter))
            {
                p.SendCmd(textBox1.Text);
            }
        }

        private void SkipButton_Click(object sender, EventArgs e)
        {
            if (p.UseVLC)
            {
                try { p.CurVLC.Kill(); } catch { }
            }
            else
            {
                axWindowsMediaPlayer1.Ctlcontrols.stop();
            }
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            if (p.UseVLC)
            {
                SendKeys.SendWait("%(j)");
            }
            else
            {
                axWindowsMediaPlayer1.Ctlcontrols.play();
            }
        }

        private void PauseButton_Click(object sender, EventArgs e)
        {
            if (p.UseVLC)
            {
                SendKeys.SendWait("%(j)");
            }
            else
            {
                axWindowsMediaPlayer1.Ctlcontrols.pause();
            }
        }

        private void SRToggleButton_Click(object sender, EventArgs e)
        {
            if (p.SREnable)
            {
                p.SREnable = false;
                SRToggleButton.Text = "Enable";
            }
            else
            {
                p.SREnable = true;
                SRToggleButton.Text = "Disable";
            }
        }
        public void BoxHandler()
        {
            checkedListBox1.Items.Clear();
            foreach (Video v in p.VidList)
            {
                checkedListBox1.Items.Add(v.Snippet.Title);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            BoxHandler();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (checkedListBox1.SelectedIndex == 0)
            {
                if (p.UseVLC)
                {
                    try { p.CurVLC.Kill(); } catch { }
                }
                else
                {
                    axWindowsMediaPlayer1.Ctlcontrols.stop();
                }
            }
            else
            {
                p.VidList.RemoveAt(checkedListBox1.SelectedIndex);
            }
            BoxHandler();

        }

        async Task Looper()
        {
            while (true)
            {
                await PutTaskDelay(1000);
                BoxHandler();
            }
            
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (checkedListBox1.SelectedIndex == 0)
            {
                if (p.UseVLC)
                {
                    try { p.CurVLC.Kill(); } catch { }
                }
                else
                {
                    axWindowsMediaPlayer1.Ctlcontrols.stop();
                }
            }
            else
            {
                if (!File.Exists("C:/Downloads/"+p.VidList[checkedListBox1.SelectedIndex].Snippet.Title.Remove(p.VidList[checkedListBox1.SelectedIndex].Snippet.Title.Length - 4) + " !done.mp3"))
                {

                }
                p.VidList.RemoveAt(checkedListBox1.SelectedIndex);
            }
            BoxHandler();
        }
    }
}

