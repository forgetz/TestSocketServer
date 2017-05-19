using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestSocketServer
{
    public partial class Form1 : Form
    {
        //private Thread _myThread;
        private int _count = 0;

        public delegate void SetMessage(string text);
        public SetMessage _myDelegate;

        private AsynchronousSocketListener _asySocket = null;
        private AsynchronousSocketListener asySocket
        {
            get
            {
                if (_asySocket == null)
                    _asySocket = new AsynchronousSocketListener(this);
                return _asySocket;
            }
        }

        public Form1()
        {
            InitializeComponent();
            _myDelegate = new SetMessage(SetMessageMethod);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lbMessage.Text = "Ready";   
        }

        public void RefreshTextMessage()
        {
            Random rand = new Random();
            var randnum = rand.Next(1, 1000);
            lbMessage.Text = randnum.ToString();
        }

        public void RefreshTextMessage2()
        {
            Random rand = new Random();
            var randnum = rand.Next(1, 1000);
            lbMessage.Text = randnum.ToString();
            txtResult.Text = "GetResult";
        }

        public void SetMessageMethod(string text)
        {
            var oldText = txtResult.Text;
            txtResult.Text = " Message " + _count++ + ": " + text + Environment.NewLine + oldText;
            RefreshTextMessage();
        }


        private void btnRandom_Click(object sender, EventArgs e)
        {
            RefreshTextMessage();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            txtResult.Text = "Start Listening";

            // new thread
            
            new Thread(async () =>
            {
                await Task.Run(() => asySocket.StartListening());
            }).Start();

            //AsynchronousSocketListener asySocket = new AsynchronousSocketListener(this);
            //_myThread = new Thread(new ThreadStart(asySocket.StartListening));
            //_myThread.Start();

            txtResult.Text = "Running Listening";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string message = "LBL " + lbMessage.Text;
            new Thread(async () =>
            {
                await Task.Run(() => asySocket.SendMessageToClient(message));
            }).Start();
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            txtResult.Text = "";
        }


    }
}
