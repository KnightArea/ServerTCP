using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace ServerTCPService
{
    public partial class Form1 : Form
    {
        TCPServer server;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            server = new TCPServer(IPAddress.Any);
            server.onClientServed += server_onClientServed;
            server.StartServer();
        }

        void server_onClientServed(string request, string response, Socket clientSocket)
        {
            listBox1.Items.Add(((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString() +
                ":" + ((IPEndPoint)clientSocket.RemoteEndPoint).Port + " --> Was served");
        }
    }
}
