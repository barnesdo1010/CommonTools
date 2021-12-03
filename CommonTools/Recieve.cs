using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools
{
    class Recieve : IDisposable
    {
        public bool connected = false;
        private List<Socket> _Error = new List<Socket>();
        public IPEndPoint EndPoint;
        Socket s;

        public string SocketMessge(string IPaddress, int port)
        {
            //** Needs work
            while (true)
            {
                if (!connected)
                {
                    CreateSocket(IPaddress, port);
                    Connect();
                }
                else
                {
                    Receive();
                }
            }
            Disconnect();
        }

        public void CreateSocket(string ip, int port)
        {
            try
            {
                s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                s.NoDelay = true;
                s.ReceiveTimeout = 5000;
                s.SendTimeout = 5000;
                EnableKeepalive(s);
                EndPoint = new IPEndPoint(System.Net.IPAddress.Parse(ip), port);
                Log.ToFile("Socket Created");
            }
            catch (Exception ex)
            {
                Log.ToFile(ex.ToString());
                Disconnect();
            }
        }

        public void Connect()
        {
            Log.ToFile("connecting...");
            try
            {
                s.Connect(EndPoint);
                if (s.Connected)
                {
                    Log.ToFile("Connected!");
                    connected = true;
                }
                else
                {
                    Log.ToFile("Not connected. Closing socket...");
                    Disconnect();
                }
            }
            catch (Exception ex)
            {
                Log.ToFile(ex.ToString());
                Disconnect();
            }
        }

        public void Disconnect()
        {
            Log.ToFile("disconnecting");
            if (null != s)
            {
                try
                {
                    s.Shutdown(SocketShutdown.Both);
                }
                catch (Exception ex)
                {
                    Log.ToFile($"Socket Client Disconnect Error: {ex.ToString()}");
                }
                finally
                {
                    connected = false;
                    s.Close();
                    s = null;
                    Dispose();
                }
            }

        }

        public void Receive()
        {
            byte[] buffer = new byte[256];
            try
            {
                if (!HasErrored())
                {
                    if (s.Poll(1, SelectMode.SelectRead))
                    {
                        int bytes = s.Receive(buffer);

                        StringBuilder _EventBuffer = new StringBuilder();
                        _EventBuffer.Append(Encoding.ASCII.GetString(buffer, 0, bytes));

                        if (0 == bytes)
                        {
                            Log.ToFile($"message string empty.");
                        }
                        else
                        {
                            string receivedData = _EventBuffer.ToString();
                            Log.ToFile($"Received Data:  {receivedData}\nparsing...");
                            Parse(receivedData);
                        }
                    }
                }
                else
                {
                    Log.ToFile($"socket connection has errored!!");
                    connected = false;
                    Connect();
                    Receive();
                }
            }
            catch (Exception ex)
            {
                Log.ToFile(ex.ToString());
                connected = false;
                Connect();
            }
        }

        public bool HasErrored()
        {
            bool retVal = false;
            if (connected == true)
            {
                _Error.Add(s);
                Socket.Select(null, null, _Error, 500);
                if (_Error.Contains(s))
                {
                    retVal = true;
                    connected = false;
                    s.Close();
                    s = null;
                }
                else
                {
                    if (s.Poll(10, SelectMode.SelectRead) && s.Available == 0)
                    {
                        retVal = true;
                        connected = false;
                        s.Close();
                        s = null;
                    }
                }
                _Error.Clear();
            }
            return retVal;
        }

        private void EnableKeepalive(Socket s)
        {
            // Get the size of the uint to use to back the byte array
            int size = Marshal.SizeOf((uint)0);

            // Create the byte array
            byte[] keepAlive = new byte[size * 3];

            // Pack the byte array:
            // Turn keepalive on
            Buffer.BlockCopy(BitConverter.GetBytes((uint)1), 0, keepAlive, 0, size);
            // Set amount of time without activity before sending a keepalive to 5 seconds
            Buffer.BlockCopy(BitConverter.GetBytes((uint)5000), 0, keepAlive, size, size);
            // Set keepalive interval to 5 seconds
            Buffer.BlockCopy(BitConverter.GetBytes((uint)5000), 0, keepAlive, size * 2, size);

            // Set the keep-alive settings on the underlying Socket
            s.IOControl(IOControlCode.KeepAliveValues, keepAlive, null);
        }

        private void Parse(string receivedData)
        {
            Log.ToFile("parsing the received string from device...");
            try
            {
                if (receivedData.Length > 1)
                {
                    string RecData = receivedData;
                    string word = receivedData;
                    int TempStartIndex = word.IndexOf('$');
                    int OxyStartIndex = word.IndexOf('#');
                    int MiliVolt = word.IndexOf('%');
                    int CarbonStartIndex = word.IndexOf('&');
                    int end = word.IndexOf('/');

                    string Temp = word.Substring(TempStartIndex + 1, (MiliVolt - 1) - TempStartIndex).Trim();
                    string Oxygen = word.Substring(OxyStartIndex + 1, (TempStartIndex - 1) - OxyStartIndex).Trim();
                    string Milivolt = word.Substring(MiliVolt + 1, (CarbonStartIndex - 1) - MiliVolt).Trim();
                    string Carbon = word.Substring(CarbonStartIndex + 1, (end - 2) - CarbonStartIndex).Trim();

                    Log.ToFile($" Temp: {Temp}°F  Oxygen: {Oxygen} Carbon: {Carbon}");
                }

            }
            catch (Exception ex) { Log.ToFile($"Could not parse data. {ex}"); }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
