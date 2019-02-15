using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Net;

namespace TCPTransfer
{
    /* TODO:
     * Implement Sender Destructor.
     * Tweak Chunk sizes.
     * test on seperate machines.
     * fix meta data accross different IPs.
     * MAKE WORK WITH FILES LARGER THAN 1 CHUNK IMPORTANT.
     */
    class Program : Form
    {
        #region Data Members
        private Button send;
        private Label label1;
        private Label label2;
        private Button clear;
        private ListBox squeue;
        private ListBox rqueue;
        private Button listen;
        private TextBox portlistenbox;
        private Label label3;
        private TextBox portsendbox;
        private TextBox ipbox;
        private Label label4;
        private Label label5;
        private Label sendstatus;
        private System.Windows.Forms.Timer timerRecieve;
        private System.ComponentModel.IContainer components;
        private Label lbltitle;
        #endregion

        private Queue<byte[]> recieveList;
        private string currentFileType;
        private string currentFileName;
        private Sender m_sender;
        private IPAddress myIp;

        Program()

        {
            myIp = getMyIP();
            recieveList = new Queue<byte[]>();
            InitializeComponent();
        }

        private IPAddress getMyIP()
        {
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress addr in localIPs)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    return addr;
                }
            }
            return null;
        }

        public void saveFileChunk(byte[] chunk)
        {
            string path = "Downloaded/" + currentFileName + currentFileType;

            using (FileStream FS = new FileStream(path, File.Exists(path) ? FileMode.Append : FileMode.OpenOrCreate, FileAccess.Write))
            {
                FS.Write(chunk, 0, chunk.Length);
            }

        }

        [STAThread]
        static void Main()
        {
           
            Application.EnableVisualStyles();
            Application.Run(new Program());

            

        }

        
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lbltitle = new System.Windows.Forms.Label();
            this.send = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.clear = new System.Windows.Forms.Button();
            this.squeue = new System.Windows.Forms.ListBox();
            this.rqueue = new System.Windows.Forms.ListBox();
            this.listen = new System.Windows.Forms.Button();
            this.portlistenbox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.portsendbox = new System.Windows.Forms.TextBox();
            this.ipbox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.sendstatus = new System.Windows.Forms.Label();
            this.timerRecieve = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // lbltitle
            // 
            this.lbltitle.AutoSize = true;
            this.lbltitle.Font = new System.Drawing.Font("Consolas", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbltitle.Location = new System.Drawing.Point(111, 9);
            this.lbltitle.Name = "lbltitle";
            this.lbltitle.Size = new System.Drawing.Size(286, 24);
            this.lbltitle.TabIndex = 0;
            this.lbltitle.Text = "TCP File Transfer Suite";
            // 
            // send
            // 
            this.send.Location = new System.Drawing.Point(12, 130);
            this.send.Name = "send";
            this.send.Size = new System.Drawing.Size(71, 39);
            this.send.TabIndex = 9;
            this.send.Text = "Send";
            this.send.UseVisualStyleBackColor = true;
            this.send.Click += new System.EventHandler(this.send_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(62, 184);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 19);
            this.label1.TabIndex = 10;
            this.label1.Text = "Send Queue";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(329, 184);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(135, 19);
            this.label2.TabIndex = 11;
            this.label2.Text = "Recieved Queue";
            // 
            // clear
            // 
            this.clear.Location = new System.Drawing.Point(218, 212);
            this.clear.Name = "clear";
            this.clear.Size = new System.Drawing.Size(71, 39);
            this.clear.TabIndex = 12;
            this.clear.Text = "Clear Send Queue";
            this.clear.UseVisualStyleBackColor = true;
            this.clear.Click += new System.EventHandler(this.clear_Click);
            // 
            // squeue
            // 
            this.squeue.AllowDrop = true;
            this.squeue.FormattingEnabled = true;
            this.squeue.HorizontalScrollbar = true;
            this.squeue.Location = new System.Drawing.Point(12, 212);
            this.squeue.Name = "squeue";
            this.squeue.Size = new System.Drawing.Size(200, 160);
            this.squeue.TabIndex = 13;
            // 
            // rqueue
            // 
            this.rqueue.FormattingEnabled = true;
            this.rqueue.Location = new System.Drawing.Point(297, 212);
            this.rqueue.Name = "rqueue";
            this.rqueue.Size = new System.Drawing.Size(200, 160);
            this.rqueue.TabIndex = 14;
            // 
            // listen
            // 
            this.listen.Location = new System.Drawing.Point(426, 130);
            this.listen.Name = "listen";
            this.listen.Size = new System.Drawing.Size(71, 39);
            this.listen.TabIndex = 15;
            this.listen.Text = "Listen";
            this.listen.UseVisualStyleBackColor = true;
            this.listen.Click += new System.EventHandler(this.listen_Click);
            // 
            // portlistenbox
            // 
            this.portlistenbox.Location = new System.Drawing.Point(427, 91);
            this.portlistenbox.Name = "portlistenbox";
            this.portlistenbox.Size = new System.Drawing.Size(70, 20);
            this.portlistenbox.TabIndex = 16;
            this.portlistenbox.Text = "6969";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Consolas", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(428, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 24);
            this.label3.TabIndex = 17;
            this.label3.Text = "Port:";
            // 
            // portsendbox
            // 
            this.portsendbox.Location = new System.Drawing.Point(13, 91);
            this.portsendbox.Name = "portsendbox";
            this.portsendbox.Size = new System.Drawing.Size(70, 20);
            this.portsendbox.TabIndex = 18;
            this.portsendbox.Text = "6969";
            // 
            // ipbox
            // 
            this.ipbox.Location = new System.Drawing.Point(13, 65);
            this.ipbox.Name = "ipbox";
            this.ipbox.Size = new System.Drawing.Size(119, 20);
            this.ipbox.TabIndex = 19;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Consolas", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(89, 91);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 24);
            this.label4.TabIndex = 20;
            this.label4.Text = "Port";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Consolas", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(138, 61);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(34, 24);
            this.label5.TabIndex = 21;
            this.label5.Text = "IP";
            // 
            // sendstatus
            // 
            this.sendstatus.AutoSize = true;
            this.sendstatus.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sendstatus.Location = new System.Drawing.Point(86, 139);
            this.sendstatus.Name = "sendstatus";
            this.sendstatus.Size = new System.Drawing.Size(126, 19);
            this.sendstatus.TabIndex = 22;
            this.sendstatus.Text = "Send Status: ";
            // 
            // timerRecieve
            // 
            this.timerRecieve.Interval = 50;
            this.timerRecieve.Tick += new System.EventHandler(this.timerRecieve_Tick);
            // 
            // Program
            // 
            this.AllowDrop = true;
            this.ClientSize = new System.Drawing.Size(509, 384);
            this.Controls.Add(this.sendstatus);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ipbox);
            this.Controls.Add(this.portsendbox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.portlistenbox);
            this.Controls.Add(this.listen);
            this.Controls.Add(this.rqueue);
            this.Controls.Add(this.squeue);
            this.Controls.Add(this.clear);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.send);
            this.Controls.Add(this.lbltitle);
            this.Name = "Program";
            this.Text = "TCP File Transfer Suite";
            this.Load += new System.EventHandler(this.Program_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void Program_Load(object sender, EventArgs e)
        {
            this.AllowDrop = true;
            this.ipbox.Text = myIp.ToString();
            this.DragDrop += Program_DragDrop;
            this.DragEnter += Program_DragEnter;
        }

        private void send_Click(object sender, EventArgs e)
        {
            try
            {
                m_sender = new Sender(squeue, new TcpClient(ipbox.Text,Convert.ToInt32(portsendbox.Text)));
                Thread m_thread = new Thread(new ThreadStart(m_sender.sendFiles));
                m_thread.Start();
                m_thread.IsBackground = true;
                sendstatus.Text = "Send Status: Success!";
                
            }

            catch (Exception ex)
            {
                Console.Write(ex);
                sendstatus.Text = "Send Status: Failed!";
            }
            
        }

        private void Program_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Link;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Program_DragDrop(object sender, DragEventArgs e)
        {
            Object data = e.Data.GetData(DataFormats.FileDrop);

            foreach(string filePath in (String[])data)
            {
                squeue.Items.Add(filePath);
            }
        }

        private void clear_Click(object sender, EventArgs e)
        {
            squeue.Items.Clear();
        }

        private void listen_Click(object sender, EventArgs e)
        {
            Receiver m_receiver = new Receiver(myIp, portlistenbox.Text, this.recieveList);
            ThreadStart listener = new ThreadStart(m_receiver.listen);
            Thread listenerThread = new Thread(listener);
            listenerThread.IsBackground = true;
            listenerThread.Start();
            timerRecieve.Enabled = true;
            listen.Enabled = false;
        }

        private void timerRecieve_Tick(object sender, EventArgs e)
        {
            if (recieveList.Count > 0)
            {
                byte[] chunk = recieveList.Dequeue();

                if (Encoding.UTF8.GetString(chunk).Replace("\0", string.Empty) == "BEGINNEWFILE") // if its a new file
                {
                    currentFileName = Encoding.UTF8.GetString(recieveList.Dequeue()).Replace("\0", string.Empty);
                    currentFileType = Encoding.UTF8.GetString(recieveList.Dequeue()).Replace("\0", string.Empty);
                    string path = "Downloaded/" + currentFileName + currentFileType;
                    // get next item which is ftype according to protocol and remove padding
                    if (!(rqueue.Items.Contains(path)))
                    {
                        rqueue.Items.Add(path);
                    }
                }

                else
                {
                    saveFileChunk(chunk);
                }
            }
            
        }
    }
}

