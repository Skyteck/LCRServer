using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LCRLibrary;
using LiteNetLib;
using LiteNetLib.Utils;

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

    class LCRServer : INetEventListener
    {
        public NetManager server;
        private readonly NetPacketProcessor _netPacketProcessor = new NetPacketProcessor();
        
        private string ServerSignature
        {
            get
            {
                return $"[Server {DateTime.Now}]";
            }
        }

        public LCRServer()
        {
            _netPacketProcessor.RegisterNestedType(() => { return new pl(); });
            _netPacketProcessor.SubscribeReusable<pl, NetPeer>(Ontest);

        }

        public void Ontest(pl sp, NetPeer p)
        {
            Console.WriteLine($"{ServerSignature} Received Packet! Details: {sp.Name} with {sp.Tickets}");
            sp.Tickets--;
            _netPacketProcessor.Send(p, sp, DeliveryMethod.ReliableOrdered);
        }

        void INetEventListener.OnConnectionRequest(ConnectionRequest request)
        {
            Console.WriteLine($"{ServerSignature} Connection attempt!");
            if (server.PeersCount < 10 /* max connections */)
                request.AcceptIfKey("SomeConnectionKey");
            else
                request.Reject();
        }

        void INetEventListener.OnPeerConnected(NetPeer peer)
        {
            Console.WriteLine($"{ServerSignature} Peer connected at {peer.EndPoint}"); // Show peer ip
            //NetDataWriter writer = new NetDataWriter();                 // Create writer class
            //writer.Put("Hello client!");                                // Put some string
            //peer.Send(writer, DeliveryMethod.ReliableOrdered);             // Send with reliability
        }

        void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Console.WriteLine($" {ServerSignature} {peer.EndPoint} go bye bye because {disconnectInfo.Reason}");
        }

        void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            throw new NotImplementedException();
        }

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            Console.WriteLine($"{ServerSignature} Received data. Processing...");
            _netPacketProcessor.ReadAllPackets(reader, peer); // LiteNetLib.Utils.ParseException: 'Undefined packet in NetDataReader'

        }

        void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            throw new NotImplementedException();
        }

        void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }

    }
}
