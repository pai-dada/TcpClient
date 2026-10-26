using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
namespace a
{
    internal class TcpClient
    {
        public string IP;
        public int Port;
        public Socket ClientSocket;
        public bool IsConnect;
        public Object _lock = new Object();
        public byte[] OriginalData;

        public TcpClient(string ip,int port)
        {
            this.IP = ip;
            this.Port = port;
            this.IsConnect = false;
        }

        public void Start()
        {
            Thread th = new Thread(() => {

                while(true)
                {
                    ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    while(true)
                    {
                        try
                        {
                            ClientSocket.Connect(IPAddress.Parse(this.IP), Port);
                            break;
                        }
                        catch (SocketException ex)
                        {
                            if (ex.ErrorCode == 10061)
                            {
                                Console.WriteLine(ex.Message);
                                continue;
                            }
                        }
                    }

                    int i = 0;
                    IsConnect = true;
                    byte[] buffer = new byte[1024 * 1024];
                    int Count=0;
                    bool readData = false;
                    while (true)
                    {
                        try
                        {
                            if(ClientSocket.Poll(0,SelectMode.SelectRead)==true)
                            {
                                Count = ClientSocket.Receive(buffer);
                                if (Count == 0)
                                {
                                    break;
                                }
                                readData = true;
                            }
                        }
                        catch (SocketException ex)
                        {
                            if (ex.ErrorCode == 10054)
                            {
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (readData)
                        {
                            lock (_lock)
                            {
                                OriginalData = new byte[Count];
                                Array.Copy(buffer, OriginalData, Count);
                                readData = false;
                            }
                        }
                        try
                        {
                            string msgStr = "hello world";
                            byte[] buffer1 = System.Text.Encoding.UTF8.GetBytes(msgStr);
                            int count = ClientSocket.Send(buffer1);
                            Console.WriteLine("发送" + count + " 字节" + i++.ToString());
                            //Thread.Sleep(1);
                        }
                        catch (SocketException ex)
                        {
                            if(ex.ErrorCode == 10053 || ex.ErrorCode == 10054)
                            {
                                
                            }
                            else
                            {
                                throw;
                            }
                        }
                        catch(Exception ex)
                        {
                            throw;
                        }
                    }

                 
                    IsConnect = false;
                    ClientSocket.Close();
                }
                


            });
            th.IsBackground = false;
            th.Start();
        }

        public void Stop()
        {
            if(ClientSocket != null)
            {
                ClientSocket.Shutdown(SocketShutdown.Both);
                ClientSocket.Close();
            }
        }


        public byte[] GetOriginalData()
        {
            lock (_lock)
            {
                return OriginalData;
            }
        }
    }
}
