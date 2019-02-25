using LCRLibrary;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace LCR
{
    class LCRServer : INetEventListener
    {
        public NetManager server;
        private readonly NetPacketProcessor _netPacketProcessor = new NetPacketProcessor();
        public List<LCRLibrary.LCRLobby> GameList = new List<LCRLobby>();
        private string ServerSignature
        {
            get
            {
                return $"[Server {DateTime.Now}]";
            }
        }

        public LCRServer()
        {
            //_netPacketProcessor.RegisterNestedType(() => { return new pl(); });
            _netPacketProcessor.SubscribeReusable<pl, NetPeer>(ProcessPacket);
            _netPacketProcessor.SubscribeReusable<CreateGamePacket, NetPeer>(ProcessNewGame);

        }

        private void ProcessNewGame(CreateGamePacket cgp, NetPeer p)
        {
            LCRLobby g = new LCRLobby(cgp.Id, cgp.Playernum);
            GameList.Add(g);
            cgp.Success = true;
            Console.WriteLine($"{ServerSignature} game created! ID:{cgp.Id} Players:{cgp.Playernum}");
            _netPacketProcessor.SendNetSerializable(p, cgp, DeliveryMethod.ReliableOrdered);
        }

        public void ProcessPacket(pl sp, NetPeer p)
        {
            sp.Tickets--;
            Console.WriteLine($"{ServerSignature} Received Packet! Details: {sp.Name} with {sp.Tickets}");
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
            Console.WriteLine($"{ServerSignature} {peer.EndPoint} go bye bye because {disconnectInfo.Reason}");
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
