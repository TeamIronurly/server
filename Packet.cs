class Packet
{
    public enum Type { PING, GET_ID, WON, LOST, JOIN, JOINED, JOIN_FAILED, CREATE_LOBBY, CREATED_LOBBY, LEFT, MOVED };
    public enum Protocol { TCP, UDP };
    public Type type;
    public int length;
    public Protocol protocol;
    public byte[] bytes = new byte[0];

    public Packet(Type type)
    {
        this.type = type;
        length = 8;
        bytes = new byte[8];
        BitConverter.GetBytes(length).CopyTo(bytes, 0);
        BitConverter.GetBytes((int)type).CopyTo(bytes, 4);
        protocol = (type == Type.MOVED||type==Type.PING) ? Protocol.UDP : Protocol.TCP;
    }
    public Packet(Type type, int player)
    {
        this.type = type;
        length = 12;
        bytes = new byte[12];
        BitConverter.GetBytes(length).CopyTo(bytes, 0);
        BitConverter.GetBytes((int)type).CopyTo(bytes, 4);
        BitConverter.GetBytes(player).CopyTo(bytes, 8);
        protocol = (type == Type.MOVED||type==Type.PING) ? Protocol.UDP : Protocol.TCP;
    }
    public Packet(Type type, int player, int a)
    {
        this.type = type;
        length = 16;
        bytes = new byte[16];
        BitConverter.GetBytes(length).CopyTo(bytes, 0);
        BitConverter.GetBytes((int)type).CopyTo(bytes, 4);
        BitConverter.GetBytes(player).CopyTo(bytes, 8);
        BitConverter.GetBytes(a).CopyTo(bytes, 12);
        protocol = (type == Type.MOVED||type==Type.PING) ? Protocol.UDP : Protocol.TCP;
    }
    public Packet(Type type, int player, byte[] extraBytes)
    {
        this.type = type;
        length = 12+extraBytes.Length;
        bytes = new byte[12+extraBytes.Length];
        BitConverter.GetBytes(length).CopyTo(bytes, 0);
        BitConverter.GetBytes((int)type).CopyTo(bytes, 4);
        BitConverter.GetBytes(player).CopyTo(bytes, 8);
        extraBytes.CopyTo(bytes, 12);
        protocol = (type == Type.MOVED||type==Type.PING) ? Protocol.UDP : Protocol.TCP;
    }

    public Packet(Protocol protocol, byte[] bytes)
    {
        this.bytes = bytes;
        this.protocol = protocol;
        type = (Type)BitConverter.ToInt32(bytes, 4);
    }
}
