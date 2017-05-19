using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestSocketServer
{
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    public class AsynchronousSocketListener
    {
        // Thread signal.
        public ManualResetEvent allDone = new ManualResetEvent(false);
        private Form1 _form;
        public AsynchronousSocketListener(Form1 frm)
        {
            this._form = frm;
        }

        private List<ClientSocketList> _clientList = null;
        private List<ClientSocketList> clientList
        {
            get
            {
                if (_clientList == null)
                    _clientList = new List<ClientSocketList>();
                return _clientList;
            }
        }

        public void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com".
            IPHostEntry ipHostInfo = Dns.GetHostEntry("pg");
            IPAddress ipAddress = ipHostInfo.AddressList[0];// ipHostInfo.AddressList.Where(f => f.ToString().IndexOf("%") < 0).FirstOrDefault();
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 18764);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                _form.Invoke(_form._myDelegate, "StartListening ERROR: " + e.Message);
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public void SendMessageToClient(string message)
        {
            if (clientList == null)
                return;
            if (clientList.Count < 1)
                return;

            foreach (var item in clientList)
            {
                Socket sock = item.SocketConnection;
                Send(sock, message);
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            var hd = handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead < 1)
            {
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                return;
            }
            
            // There  might be more data, so store the data received so far.
            state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                
            // Check for end-of-file tag. If it is not there, read 
            // more data.
            content = state.sb.ToString();
                
            if (content.IndexOf("<EOF>") < 0)
            {
                return;
            }

            //string filterContent = content.Replace("<EOF>", "");
            if (content.IndexOf("CONNECT") > -1)
            {
                string remoteEndPointFullText = state.workSocket.RemoteEndPoint.ToString();
                string[] temp = remoteEndPointFullText.Split(':');
                string remoteEndPoint = temp[0];

                var isexists = clientList.Where(c => c.Address == remoteEndPoint).FirstOrDefault();

                if (isexists == null)
                {
                    ClientSocketList csl = new ClientSocketList();
                    csl.LastConnected = DateTime.Now;
                    csl.port = 18764;
                    csl.SocketConnection = handler;
                    csl.Address = remoteEndPoint;

                    clientList.Add(csl);
                }
                else
                {
                    isexists.LastConnected = DateTime.Now;
                }


                _form.Invoke(_form._myDelegate, " CONNECT AND SAVE CLIENT LIST");
            }

            if (content.IndexOf("MESSAGE") > -1)
            {
                // All the data has been read from the 
                // client. Display it on the console.
                Console.WriteLine("Read {0} bytes from socket. \n Data : {1}", content.Length, content);

                _form.Invoke(_form._myDelegate, "REC: " + content);
                // Echo the data back to the client.
                Send(handler, "Send Data Back To Client -> " + content);
            }

            
        }

        private void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                _form.Invoke(_form._myDelegate, "SendCallback Error: " + e.Message);
                Console.WriteLine(e.ToString());
            }
        }


    }


}
