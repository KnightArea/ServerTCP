using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerTCPService
{
    class TCPSocketListener
    {
        public delegate void _ClientServed(string request,string response,Socket clientSocket);
        public event _ClientServed onClientServed;

        /// <summary>
        /// Variables that are accessed by other classes indirectly.
        /// </summary>
        private Socket m_clientSocket = null;
        private bool m_stopClient = false;
        private Thread m_clientListenerThread = null;
        private bool m_markedForDeletion = false;

        /// <summary>
        /// Working Variables.
        /// </summary>
        private StringBuilder m_oneLineBuf = new StringBuilder();
        private long m_totalClientDataSize = 0;
        private DateTime m_lastReceiveDateTime;
        private DateTime m_currentReceiveDateTime;
        private string ClientRequest="",clientResponse="";
        /// <summary>
        /// Client Socket Listener Constructor.
        /// </summary>
        /// <param name="clientSocket"></param>
        public TCPSocketListener(Socket clientSocket)
        {
            m_clientSocket = clientSocket;
        }

        /// <summary>
        /// Client SocketListener Destructor.
        /// </summary>
        ~TCPSocketListener()
        {
            StopSocketListener();
        }

        /// <summary>
        /// Method that starts SocketListener Thread.
        /// </summary>
        public void StartSocketListener()
        {
            if (m_clientSocket != null)
            {
                m_clientListenerThread =
                    new Thread(new ThreadStart(SocketListenerThreadStart));

                m_clientListenerThread.Start();
            }
        }

        /// <summary>
        /// Thread method that does the communication to the client. This 
        /// thread tries to receive from client and if client sends any data
        /// then parses it and again wait for the client data to come in a
        /// loop. The recieve is an indefinite time receive.
        /// </summary>
        private void SocketListenerThreadStart()
        {
            int size = 0;
            Byte[] byteBuffer = new Byte[1048576];

            m_lastReceiveDateTime = DateTime.Now;
            m_currentReceiveDateTime = DateTime.Now;
            Timer t = new Timer(new TimerCallback(CheckClientCommInterval),
                null, 15000, 15000);

            while (!m_stopClient)
            {
                try
                {
                    size = m_clientSocket.Receive(byteBuffer);
                    m_currentReceiveDateTime = DateTime.Now;
                    ParseReceiveBuffer(byteBuffer, size);
                }
                catch (SocketException se)
                {
                    m_stopClient = true;
                    m_markedForDeletion = true;
                }
            }
            t.Change(Timeout.Infinite, Timeout.Infinite);
            t = null;
        }

        /// <summary>
        /// Method that stops Client SocketListening Thread.
        /// </summary>
        public void StopSocketListener()
        {
            if (m_clientSocket != null)
            {
                m_stopClient = true;
                m_clientSocket.Close();

                // Wait for one second for the the thread to stop.
                m_clientListenerThread.Join(1000);

                // If still alive; Get rid of the thread.
                if (m_clientListenerThread.IsAlive)
                {
                    m_clientListenerThread.Abort();
                }
                m_clientListenerThread = null;
                m_clientSocket = null;
                m_markedForDeletion = true;
            }
        }

        /// <summary>
        /// Method that returns the state of this object i.e. whether this
        /// object is marked for deletion or not.
        /// </summary>
        /// <returns></returns>
        public bool IsMarkedForDeletion()
        {
            return m_markedForDeletion;
        }

        /// <summary>
        /// This method parses data that is sent by a client using TCP/IP.
        /// As per the "Protocol" between client and this Listener, client 
        /// sends each line of information by appending "CRLF" (Carriage Return
        /// and Line Feed). But since the data is transmitted from client to 
        /// here by TCP/IP protocol, it is not guarenteed that each line that
        /// arrives ends with a "CRLF". So the job of this method is to make a
        /// complete line of information that ends with "CRLF" from the data
        /// that comes from the client and get it processed.
        /// </summary>
        /// <param name="byteBuffer"></param>
        /// <param name="size"></param>
        private void ParseReceiveBuffer(Byte[] byteBuffer, int size)
        {
            try
            {
                string data = Encoding.UTF8.GetString(byteBuffer, 0, size);
                int lineEndIndex = 0;

                // Check whether data from client has more than one line of 
                // information, where each line of information ends with "CRLF"
                // ("\r\n"). If so break data into different lines and process
                // separately.
                do
                {
                    lineEndIndex = data.IndexOf("\r\n");
                    if (lineEndIndex != -1)
                    {
                        m_oneLineBuf = m_oneLineBuf.Append(data, 0, lineEndIndex + 2);
                        ProcessClientData(m_oneLineBuf.ToString());
                        m_oneLineBuf.Remove(0, m_oneLineBuf.Length);
                        data = data.Substring(lineEndIndex + 2,
                            data.Length - lineEndIndex - 2);
                    }
                    else
                    {
                        m_oneLineBuf = m_oneLineBuf.Append(data);
                        ProcessClientData(m_oneLineBuf.ToString());
                    }
                } while (lineEndIndex != -1);

                bool hasResponse = false;

                clientResponse = ServerServices.Start(Newtonsoft.Json.JsonConvert.DeserializeObject<RequestType>(ClientRequest),
                                                    out hasResponse);
                if (hasResponse)
                {
                    try
                    {
                        m_clientSocket.Send(Encoding.UTF8.GetBytes(clientResponse));
                        
                        if (onClientServed != null)
                        {
                            this.
                               onClientServed(ClientRequest, clientResponse, m_clientSocket);
                        }
                    }
                    catch (SocketException se)
                    {

                    }
                }
                clientResponse = "";
                ClientRequest = "";
            }
            catch { }
        }

        /// <summary>
        /// Method that Process the client data as per the protocol. 
        /// The protocol works like this. 
        /// 1. Client first send a file name that ends with "CRLF".
        /// 
        /// 2. This SocketListener has to return the length of the file name
        /// to the client for validation. If the length matches with the length
        /// what  client had sent earlier, then client starts sending lines of
        /// information. Otherwise socket will be closed by the client.
        /// 
        /// 3. Each line of information that client sends will end with "CRLF".
        /// 
        /// 4. This socketListener has to store each line of information in 
        /// a text file whoose file name has been sent by the client earlier.
        /// 
        /// 5. As a last line of information client sends "[EOF]" line which
        /// also ends with "CRLF" ("\r\n"). This signals this SocketListener
        /// for an end of file and intern this SocketListener sends the total
        /// length of the data (lines of information excludes file name that
        /// was sent earlier) back to client for validation.
        /// </summary>
        /// <param name="oneLine"></param>
        private void ProcessClientData(String oneLine)
        {
            ClientRequest += oneLine;
        }

        /// <summary>
        /// Method that checks whether there are any client calls for the
        /// last 15 seconds or not. If not this client SocketListener will
        /// be closed.
        /// </summary>
        /// <param name="o"></param>
        private void CheckClientCommInterval(object o)
        {
            if (m_lastReceiveDateTime.Equals(m_currentReceiveDateTime))
            {
                this.StopSocketListener();
            }
            else
            {
                m_lastReceiveDateTime = m_currentReceiveDateTime;
            }
        }
    }
}
