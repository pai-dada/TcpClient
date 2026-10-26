
namespace a

{
    class p
    {
        static void Main()
        {
            TcpClient t = new TcpClient("127.0.0.1", 5050);
            t.Start();
        }
    }
}
