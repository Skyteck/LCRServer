using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            SomeServerListener listener = new SomeServerListener();
            NetManager server = new NetManager(listener);
            listener.server = server;
            server.Start(9050 /* port */);

            Console.WriteLine("[Server] Ready!");

            listener.PeerConnectedEvent += peer =>
            {
                Console.WriteLine($"Connected to {peer.EndPoint}");
            };

            while (!Console.KeyAvailable)
            {
                server.PollEvents();
                Thread.Sleep(15);
            }
            server.Stop();

        }
    }

    class SomeServerListener : INetEventListener
    {
        public NetManager server;
        private readonly NetPacketProcessor _netPacketProcessor = new NetPacketProcessor();

        public event OnNetworkError NetworkErrorEvent;
        public event OnConnectionRequest ConnectionRequestEvent;
        public event OnPeerConnected PeerConnectedEvent;
        public event OnPeerDisconnected PeerDisconnectedEvent;
        public event OnNetworkLatencyUpdate NetworkLatencyUpdateEvent;
        public event OnNetworkReceive NetworkReceiveEvent;
        public event OnNetworkReceiveUnconnected NetworkReceiveUnconnectedEvent;

        public delegate void OnPeerConnected(NetPeer peer);
        public delegate void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo);
        public delegate void OnNetworkError(IPEndPoint endPoint, SocketError socketError);
        public delegate void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod);
        public delegate void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType);
        public delegate void OnNetworkLatencyUpdate(NetPeer peer, int latency);
        public delegate void OnConnectionRequest(ConnectionRequest request);

        public SomeServerListener()
        {
            _netPacketProcessor.RegisterNestedType<pl>(() => { return new pl(); });
            _netPacketProcessor.SubscribeReusable<SamplePacket, NetPeer>(Ontest);

        }

        public void Ontest(SamplePacket sp, NetPeer p)
        {
            Console.WriteLine("[Server] Received Packet!");
        }

        void INetEventListener.OnConnectionRequest(ConnectionRequest request)
        {
            Console.WriteLine("[Server] Connection attempt!");
            if (server.PeersCount < 10 /* max connections */)
                request.AcceptIfKey("SomeConnectionKey");
            else
                request.Reject();
        }

        void INetEventListener.OnPeerConnected(NetPeer peer)
        {
            Console.WriteLine($"[Server] Peer connected at {peer.EndPoint}"); // Show peer ip
            NetDataWriter writer = new NetDataWriter();                 // Create writer class
            writer.Put("Hello client!");                                // Put some string
            peer.Send(writer, DeliveryMethod.ReliableOrdered);             // Send with reliability
        }

        void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Console.WriteLine($"{peer.EndPoint} go bye bye because {disconnectInfo.Reason}");
        }

        void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            throw new NotImplementedException();
        }

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            Console.WriteLine("[Server] received data. Processing...");
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

    class SamplePacket : INetSerializable
    {
        public pl testPl { get; set;}

        public void Deserialize(NetDataReader reader)
        {
            throw new NotImplementedException();
        }

        public void Serialize(NetDataWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class pl : INetSerializable
    {
        public string Name { get; set; }
        public int Tickets { get; set; }
        public delegate pl reee();
        public pl()
        {
        }
        public void Deserialize(NetDataReader reader)
        {
            Name = reader.GetString();
            Tickets = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Tickets);
            writer.Put(Name);
        }
    }
}
