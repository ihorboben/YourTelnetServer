using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace YourTelnetServer
{
    class Server
    {
        private readonly Socket _serverSocket;
        private readonly IPAddress _ipAddress;
        private readonly int _port;
        private byte[] _data;
        private Dictionary<Socket, Client> clients;

        public Server(IPAddress ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
            _data = new byte[1024];
            clients = new Dictionary<Socket, Client>();
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start()
        {
            _serverSocket.Bind(new IPEndPoint(_ipAddress, _port));
            _serverSocket.Listen(0);
            _serverSocket.BeginAccept(HandleIncomingConnection, _serverSocket);
        }
        public void SendMessageToClient(Client c, string message)
        {
            Socket clientSocket = GetSocketByClient(c);
            SendMessageToSocket(clientSocket, message);
        }
        private void SendMessageToSocket(Socket s, string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            SendBytesToSocket(s, data);
        }

        private Socket GetSocketByClient(Client client)
        {
            Socket s;

            s = clients.FirstOrDefault(x => x.Value.GetClientID() == client.GetClientID()).Key;

            return s;
        }
        private void HandleIncomingConnection(IAsyncResult result)
        {
            try
            {
                Socket oldSocket = (Socket)result.AsyncState;
                Socket newSocket = oldSocket.EndAccept(result);

                int clientID = clients.Count + 1;
                Client client = new Client(clientID, (IPEndPoint)newSocket.RemoteEndPoint);
                clients.Add(newSocket, client);

                SendBytesToSocket(newSocket, new byte[] {0xff, 0xfd, 0x01, 0xff, 0xfd, 0x21, 0xff, 0xfb, 0x01, 0xff, 0xfb, 0x03 });
                client.ResetReceivedData();
                _serverSocket.BeginAccept(HandleIncomingConnection, _serverSocket);
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        private void SendBytesToSocket(Socket s, byte[] data)
        {
            s.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendData), s);
        }

        private void SendData(IAsyncResult result)
        {
            try
            {
                Socket clientSocket = (Socket)result.AsyncState;
                clientSocket.EndSend(result);
                clientSocket.BeginReceive(_data, 0, 1024, SocketFlags.None, new AsyncCallback(ReceiveData), clientSocket);
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        private Client GetClientBySocket(Socket clientSocket)
        {
            Client c;
            if (!clients.TryGetValue(clientSocket, out c))
                c = null;

            return c;
        }
        private void ReceiveData(IAsyncResult result)
        {
            try
            {
                Socket clientSocket = (Socket)result.AsyncState;
                Client client = GetClientBySocket(clientSocket);

                int bytesReceived = clientSocket.EndReceive(result);
                if (bytesReceived == 0)
                {
                    CloseSocket(clientSocket);
                    clients.Remove(clientSocket);
                    _serverSocket.BeginAccept(HandleIncomingConnection, _serverSocket);
                } 
                else if (_data[0] < Chars.SpecChars)
                {
                    string receivedData = client.GetReceivedData();
                    if ((_data[0] == Chars.Dot && _data[1] == Chars.CarriageReturn) || (_data[0] == Chars.CarriageReturn && _data[1] == Chars.NewLine))
                    {
                        var resivedData = client.GetReceivedData();

                        if (resivedData.Equals("list", StringComparison.InvariantCultureIgnoreCase))
                        {
                            SendMessageToClient(client, "\r\n");
                            foreach (KeyValuePair<Socket, Client> keyValuePair in clients)
                            {
                                SendMessageToClient(client, $"Client: {keyValuePair.Value}, Sum: {keyValuePair.Value.GetSum()}\r\n");
                            }
                            client.ResetReceivedData();
                        }
                        else if (resivedData.Equals("q", StringComparison.InvariantCultureIgnoreCase))
                        {
                            CloseSocket(clientSocket);
                            clients.Remove(clientSocket);
                            _serverSocket.BeginAccept(HandleIncomingConnection, _serverSocket);
                        }
                        else if (Int32.TryParse(resivedData, out int i))
                        {
                            client.AddNumber(i);
                            SendMessageToClient(client, $"\r\n{client.GetSum()}\r\n");
                            client.ResetReceivedData();
                        }
                        else
                        {
                            SendMessageToClient(client, "\r\nError: Type digits only \r\n");
                            client.ResetReceivedData();
                        }
                    }
                    else
                    {
                        if (_data[0] == Chars.Backspace)
                        {
                            if (receivedData.Length > 0)
                            {
                                client.RemoveLastCharacterReceived();
                                SendBytesToSocket(clientSocket, new byte[] { 0x08, 0x20, 0x08 });
                            }
                            else
                                clientSocket.BeginReceive(_data, 0, 1024, SocketFlags.None, ReceiveData, clientSocket);
                        }
                        else if (_data[0] == Chars.Delete)
                            clientSocket.BeginReceive(_data, 0, 1024, SocketFlags.None, ReceiveData, clientSocket);

                        else
                        {
                            client.AppendReceivedData(Encoding.ASCII.GetString(_data, 0, bytesReceived));
                            SendBytesToSocket(clientSocket, new [] { _data[0] });
                            clientSocket.BeginReceive(_data, 0, 1024, SocketFlags.None, ReceiveData, clientSocket);
                        }
                    }
                }
                else
                {
                    clientSocket.BeginReceive(_data, 0, 1024, SocketFlags.None, ReceiveData, clientSocket);
                }
            }

            catch (Exception e)
            {
                // ignored
            }
        }

        private void CloseSocket(Socket clientSocket)
        {
            clientSocket.Close();
        }
    }
}
