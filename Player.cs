using System.Net;
using System.Net.Sockets;

class Player
{
    Lobby? lobby;
    public int id = 0;
    TcpClient tcpClient;
    UdpClient udp;
    NetworkStream tcp;
    public Player(TcpClient tcpClient, UdpClient udpClinet, int id)
    {
        this.id = id;
        this.tcpClient = tcpClient;
        this.tcp = tcpClient.GetStream();
        this.udp = udpClinet;
        startReceiveLoops();
        Log.info($"player {id} connected");
    }

    public void send(Packet packet)
    {
        if (packet.protocol == Packet.Protocol.TCP)
        {
            tcp.Write(packet.bytes);
        }
        else
        {
            udp.Send(packet.bytes);
        }
    }

    void receive(Packet packet)
    {
        switch (packet.type)
        {
            case Packet.Type.PING:
                send(packet);
                break;
            case Packet.Type.GET_ID:
                send(new Packet(Packet.Type.GET_ID, id));
                Log.info($"player {id} logged in");
                break;
            case Packet.Type.CREATE_LOBBY:
                lobby = Program.createLobby();
                lobby.join(this);
                send(new Packet(Packet.Type.CREATED_LOBBY, id, lobby.id));
                send(new Packet(Packet.Type.JOINED, id));
                break;
            case Packet.Type.JOIN:
                int lobby_id = BitConverter.ToInt32(packet.bytes, 12);
                if (Program.lobbies.ContainsKey(lobby_id) && Program.lobbies[lobby_id].join(this))
                {
                    send(new Packet(Packet.Type.JOINED, id));
                    lobby = Program.lobbies[lobby_id];
                }
                else
                {
                    send(new Packet(Packet.Type.JOIN_FAILED, id));
                }
                break;
            case Packet.Type.LEFT:
                lobby?.leave(this);
                lobby = null;
                break;
            default:
                lobby?.sendToAll(this, packet);
                break;
        }
    }

    void startReceiveLoops()
    {
        new Thread(TcpLoop).Start();
        new Thread(UdpLoop).Start();
    }

    void TcpLoop()
    {
        try
        {
            while (tcpClient.Connected)
            {
                byte[] lengthAndTypeBuffer = new byte[8];
                tcp.Read(lengthAndTypeBuffer, 0, 8);
                int length = BitConverter.ToInt32(lengthAndTypeBuffer, 0);
                Packet.Type type = (Packet.Type)BitConverter.ToInt32(lengthAndTypeBuffer, 4);

                byte[] bytes = new byte[Packet.Lengths[type]];
                lengthAndTypeBuffer.CopyTo(bytes, 0);
                int read = tcp.Read(bytes, 8, Packet.Lengths[type] - 8);
                if (read == 0)
                {
                    tcpClient.Close();
                    break;
                }

                receive(new Packet(Packet.Protocol.TCP, bytes));
            }
        }
        catch (SocketException)
        {
            tcpClient.Close();
        }
        catch (Exception e)
        {
            tcpClient.Close();
            Console.Error.WriteLine(e);
        }
        tcpClient.Close();
        udp.Close();
        lobby?.leave(this);
        Log.info($"player {id} disconnected");
    }
    void UdpLoop()
    {
        try
        {
            while (tcpClient.Connected)
            {
                IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
                byte[] bytes = udp.Receive(ref ip);
                receive(new Packet(Packet.Protocol.UDP, bytes));
            }
        }
        catch (SocketException)
        {
            udp.Close();
        }
        catch (Exception e)
        {
            tcpClient.Close();
            Console.Error.WriteLine(e);
        }
    }
}
