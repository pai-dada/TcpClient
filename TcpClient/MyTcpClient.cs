using System.Net;
using System.Net.Sockets;
namespace a
{
    internal class MyTcpClient
    {
        public string IP;
        public int Port;
        public volatile Socket? ClientSocket;
        public volatile bool IsConnect;//反应当前socket是否连接
        public volatile bool IsContinue;//反应当前socket是否应该继续连接
        public Object _lock = new Object();
        public byte[] OriginalData = new byte[1024 * 1024];
        public int OriginalDataLength = 0;
        public event EventHandler<DataReceivedEventArgs>? DataReceived;
        public class DataReceivedEventArgs : EventArgs 
        {
            public byte[] Data { get; }
            public int Length { get; }
            public DataReceivedEventArgs(byte[] data,int length)
            {
                this.Data = data;
                this.Length = length;
            }
        }


        public MyTcpClient(string ip, int port)
        {
            this.IP = ip;
            this.Port = port;
        }

        public void Start()
        {
            this.IsContinue = true;
            this.IsConnect = false;
            Thread th = new Thread(() =>
            {

                try
                {
                    //循环建立socket
                    while (IsContinue == true)
                    {
                        ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        //循环尝试连接
                        while (IsConnect == false && IsContinue == true)
                        {
                            try
                            {
                                ClientSocket.Connect(IPAddress.Parse(this.IP), Port);
                                IsConnect = true;
                            }
                            catch (SocketException ex)
                            {
                                if (ex.ErrorCode == 10061 || ex.ErrorCode == 10060)
                                {
                                    Console.WriteLine(ex.Message);
                                    Thread.Sleep(1000);
                                    continue;
                                }
                                else
                                {
                                    throw;
                                }
                            }
                            catch
                            {
                                throw;
                            }
                        }

                        byte[] buffer = new byte[1024 * 1024];//程序缓冲区1MB大小
                        int length = -1;

                        //循环接收信息
                        while (IsContinue == true)
                        {
                            try
                            {
                                length = ClientSocket.Receive(buffer);
                                if (length == 0)
                                {
                                    break;
                                }

                            }
                            catch (SocketException ex)
                            {
                                if (ex.ErrorCode == 10053 || ex.ErrorCode == 10054)
                                {
                                    break;
                                }
                                else
                                {
                                    throw;
                                }
                            }
                            catch
                            {
                                throw;
                            }

                            if (length >= 0)
                            {
                                lock (_lock)
                                {
                                    Buffer.BlockCopy(buffer, 0, OriginalData, 0, length);
                                    OriginalDataLength = length;
                                }
                                byte[] data = GetOriginalData();
                                OnDataReceived(data, length);
                            }
                        }

                        IsConnect = false;
                        ClientSocket?.Close();
                        ClientSocket?.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    IsConnect = false;
                    ClientSocket?.Close();
                    ClientSocket?.Dispose();
                }

            });
            th.IsBackground = true;
            th.Start();
        }

        public void Stop()
        {
            this.IsConnect = false;
            this.IsContinue = false;
            try
            {
                ClientSocket?.Shutdown(SocketShutdown.Both);
                ClientSocket?.Close();
                ClientSocket?.Dispose();
            }
            catch
            {
                throw;
            }
        }


        public byte[] GetOriginalData()
        {
            lock (_lock)
            {
                byte[] res = new byte[OriginalDataLength];
                Buffer.BlockCopy(OriginalData, 0, res, 0, OriginalDataLength);
                return res;
            }
        }

        public int SendMsg(byte[] buffer)
        {
            try
            {
                int count = ClientSocket.Send(buffer);
                Console.WriteLine("发送" + count + "字节的数据");
                return count;
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode == 10053 || ex.ErrorCode == 10054)
                {
                    //10053:自己断开连接  10054:对方断开连接
                    throw;
                }
                else
                {
                    throw;
                }
            }
            catch
            {
                throw;
            }
        }

        private void OnDataReceived(byte[] data,int length)
        {
            EventHandler<DataReceivedEventArgs>? handler = DataReceived;
            if (handler == null) return;


            DataReceivedEventArgs args = new DataReceivedEventArgs(data,length);
            foreach(EventHandler<DataReceivedEventArgs> d in handler.GetInvocationList())
            {
                try
                {
                    d(this, args);
                }
                catch (Exception ex) 
                {
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
