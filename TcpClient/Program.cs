
using System.Text;

namespace a

{
    class p
    {
        static void Main()
        {
            MyTcpClient t = new MyTcpClient("127.0.0.1", 5055);
            t.Start();

            t.DataReceived += (sender, e) =>
            {
                Console.WriteLine("接收到" + e.Length + "字节的数据:"+Encoding.UTF8.GetString(e.Data));
            };

            Console.ReadLine();

            string a = "hello world";
            for(int i=0;i<100;i++)
            {
                try
                {
                    t.SendMsg(Encoding.UTF8.GetBytes(a));
                }
                catch
                {
                    continue;
                }
                Thread.Sleep(1000);
            }
            

            Console.ReadLine();
        }
    }
}
