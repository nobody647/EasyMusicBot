﻿using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasyMusicBot
{
    public partial class Form1 : Form
    {
        Program p;
        delegate void SetTextCallback();
        public Form1(Program p)
        {
            InitializeComponent();
            //this.Hide();

            this.p = p;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Looper();
        }

        async Task PutTaskDelay(int delay)
        {
            await Task.Delay(delay);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            p.SendCmd(textBox1.Text, 500);
        }

        private void SkipButton_Click(object sender, EventArgs e)
        {
            if (Settings.AudioMethod.Contains("VLC"))
            {
                Modules.MusicModule.Skipping = true;
                try { Settings.CurAM.Kill(); } catch { }
            }
            else
            {
                axWindowsMediaPlayer1.Ctlcontrols.stop();
            }
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            if (Settings.AudioMethod.Contains("VLC"))
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
            if (Settings.AudioMethod.Contains("VLC"))
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
            if (Settings.SREnable)
            {
                Settings.SREnable = false;
                SRToggleButton.Text = "Enable";
            }
            else
            {
                Settings.SREnable = true;
                SRToggleButton.Text = "Disable";
            }
        }

        public void BoxHandler()
        {
            this.HandleBox();
        }

        public void HandleBox()
        {
            if (listBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(HandleBox);
                this.Invoke(d, new object[] { });
            }
            else
            {
                listBox1.Items.Clear();
                foreach (Video v in Modules.MusicModule.VidList)
                {
                    listBox1.Items.Add(v.Snippet.Title);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            BoxHandler();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == 0)
            {
                if (Settings.AudioMethod.Contains("VLC"))
                {
                    Modules.MusicModule.Skipping = true;
                    try { Settings.CurAM.Kill(); } catch { }
                }
                else
                {
                    axWindowsMediaPlayer1.Ctlcontrols.stop();
                }
            }
            else
            {
                Modules.MusicModule.VidList.RemoveAt(listBox1.SelectedIndex);
            }
            BoxHandler();

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (this.listBox1.SelectedItem == null) return;
            if (this.listBox1.SelectedIndex == 0) return;
            this.listBox1.DoDragDrop(this.listBox1.SelectedItem, DragDropEffects.Move);
        }

        private void listBox1_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            Point point = listBox1.PointToClient(new Point(e.X, e.Y));
            int index = this.listBox1.IndexFromPoint(point);
            if (index < 0) index = this.listBox1.Items.Count - 1;
            String data = (String)e.Data.GetData(typeof(String));

            if (index != 0)
            {
                this.listBox1.Items.Remove(data);
                List<Video> FakeList = Modules.MusicModule.VidList;
                for (int i = Modules.MusicModule.VidList.Count - 1; i >= 0; i--)
                {
                    if (Modules.MusicModule.VidList[i].Snippet.Title.Equals(data))
                    {
                        Modules.MusicModule.VidList.Remove(Modules.MusicModule.VidList[i]);
                        break;
                    }
                }
                this.listBox1.Items.Insert(index, data);
                Modules.MusicModule.VidList.Insert(index, p.GetVideoBySearch(data));
            }
            else
            {

            }


        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (listBox1.SelectedIndex == 0)
                {
                    if (Settings.AudioMethod.Contains("VLC"))
                    {
                        Modules.MusicModule.Skipping = true;
                        try { Settings.CurAM.Kill(); } catch { }
                    }
                    else
                    {
                        axWindowsMediaPlayer1.Ctlcontrols.stop();
                    }
                }
                else
                {
                    Modules.MusicModule.VidList.RemoveAt(listBox1.SelectedIndex);
                }
                BoxHandler();
            }
        }
    }
}

