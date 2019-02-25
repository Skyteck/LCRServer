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


                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(false);
                    if (key.KeyChar == 'g')
                    {
                        if(listener.GameList.Count > 0)
                        {
                            int gc = 0;
                            foreach (LCRLibrary.LCRLobby l in listener.GameList)
                            {
                                Console.WriteLine($"[Server {DateTime.Now}] Lobby {gc} info: ID:{l.Id} Players: {l.Players}");
                                gc++;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[Server {DateTime.Now}] no active lobbies.");
                        }
                    }
                }


            }
            server.Stop();

        }
    }
}
