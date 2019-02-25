using LiteNetLib;
using System;
using System.Threading;

namespace LCR
{
    class Program
    {
        static void Main(string[] args)
        {
            LCRServer listener = new LCRServer();
            NetManager server = new NetManager(listener);
            listener.server = server;
            server.Start(9050 /* port */);

            Console.WriteLine($"[Server {DateTime.Now}] Ready at {server.LocalPort}!");

            while (!Console.KeyAvailable)
            {
                server.PollEvents();
                Thread.Sleep(15);
            }
            server.Stop();

        }
    }
}
