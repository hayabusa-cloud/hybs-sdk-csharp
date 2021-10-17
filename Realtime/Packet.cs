using System;
using System.Text;

namespace Hybs.Realtime
{
    public interface IPacketReader
    {
        IPacketReader Read(out bool val);
        IPacketReader Read(out byte val);
        IPacketReader Read(out short val);
        IPacketReader Read(out ushort val);
        IPacketReader Read(out int val);
        IPacketReader Read(out uint val);
        IPacketReader Read(out long val);
        IPacketReader Read(out ulong val);
        IPacketReader Read(out float val);
        IPacketReader Read(out double val);
        IPacketReader Read(out byte[] val);
        IPacketReader Read(out string val);
        IPacketReader Read(out short[] val);
        IPacketReader Read(out ushort[] val);
        IPacketReader Read(out int[] val);
        IPacketReader Read(out uint[] val);
        IPacketReader Read(out long[] val);
        IPacketReader Read(out ulong[] val);
        IPacketReader Read(out float[] val);
        IPacketReader Read(out double[] val);
        IPacketReader Read(out string[] val);
        IPacketReader Read(out byte[][] val);
        IPacketReader Read(out DateTime val);
        IPacketReader Read(out TimeSpan val);
        IPacketReader Read(out User val);
        IPacketReader Read(out Room val);
    }
    public interface IGamePacketReader : IPacketReader
    {
        ushort EventCode { get; }
        int PayloadLen { get; }
        bool IsBuiltin { get; }
    }
    public interface IPacketWriter
    {
        IPacketWriter WriteBool(bool val);
        IPacketWriter Write(bool val);
        IPacketWriter WriteByte(byte val);
        IPacketWriter Write(byte val);
        IPacketWriter WriteShort(short val);
        IPacketWriter Write(short val);
        IPacketWriter WriteUshort(ushort val);
        IPacketWriter Write(ushort val);
        IPacketWriter WriteInt(int val);
        IPacketWriter Write(int val);
        IPacketWriter WriteUint(uint val);
        IPacketWriter Write(uint val);
        IPacketWriter WriteLong(long val);
        IPacketWriter Write(long val);
        IPacketWriter WriteUlong(ulong val);
        IPacketWriter Write(ulong val);
        IPacketWriter WriteFloat(float val);
        IPacketWriter Write(float val);
        IPacketWriter WriteDouble(double val);
        IPacketWriter Write(double val);
        IPacketWriter Write(byte[] val);
        IPacketWriter Write(string val);
        IPacketWriter Write(DateTime val);
        IPacketWriter Write(TimeSpan val);
        IPacketWriter Write(Packet pkt);
        IPacketWriter Reset();
        IPacketWriter Seek(ushort offset);
        IPacketWriter Send(out byte[] val, out int index, out int len);
    }
    public partial class Packet : IPacketReader, IPacketWriter
    {
        public IPacketWriter Seek(ushort offset)
        {
            m_payloadOffset = offset;
            return this;
        }
        public IPacketWriter Reset() 
        {
            m_header = 0;
            m_payloadOffset = 3;
            return this;
        }

        public IPacketReader Read(out bool val)
        {
            val = m_payload[m_payloadOffset++] != 0;
            return this;
        }
        public IPacketReader Read(out byte val)
        {
            val = m_payload[m_payloadOffset++];
            return this;
        }
        public IPacketReader Read(out short val)
        {
            val = (short)(m_payload[m_payloadOffset] << 8 | m_payload[m_payloadOffset + 1]);
            m_payloadOffset += 2;
            return this;
        }
        public IPacketReader Read(out ushort val)
        {
            val = (ushort)(m_payload[m_payloadOffset] << 8 | m_payload[m_payloadOffset + 1]);
            m_payloadOffset += 2;
            return this;
        }
        public IPacketReader Read(out int val)
        {
            this.Read(out short v1);
            this.Read(out ushort v2);
            val = v1 << 16 | v2;
            return this;
        }
        public IPacketReader Read(out uint val)
        {
            this.Read(out ushort v1);
            this.Read(out ushort v2);
            val = (uint)(v1 << 16 | v2);
            return this;
        }
        public IPacketReader Read(out long val)
        {
            this.Read(out int v1);
            this.Read(out uint v2);
            val = (long)v1 << 32 | v2;
            return this;
        }
        public IPacketReader Read(out ulong val)
        {
            this.Read(out uint v1);
            this.Read(out uint v2);
            val = (ulong)v1 << 32 | v2;
            return this;
        }
        public IPacketReader Read(out float val)
        {
            var bytes = new byte[4];
            Array.Copy(m_payload, m_payloadOffset, bytes, 0, 4);
            Array.Reverse(bytes);
            val = BitConverter.ToSingle(bytes, 0);
            m_payloadOffset += 4;
            return this;
        }
        public IPacketReader Read(out double val)
        {
            var bytes = new byte[8];
            Array.Copy(m_payload, m_payloadOffset, bytes, 0, 8);
            Array.Reverse(bytes);
            val = BitConverter.ToDouble(bytes, 0);
            m_payloadOffset += 8;
            return this;
        }
        public IPacketReader Read(out byte[] val)
        {
            var len = (ushort)(m_payload[m_payloadOffset] << 8 | m_payload[m_payloadOffset + 1]);
            m_payloadOffset += 2;
            // val = UTF8Encoding.UTF8.GetString(m_payload, m_payloadOffset, len);
            val = new byte[len];
            Array.Copy(m_payload, m_payloadOffset, val, 0, len);
            m_payloadOffset += len;
            return this;
        }
        public IPacketReader Read(out string val)
        {
            var len = (ushort)(m_payload[m_payloadOffset] << 8 | m_payload[m_payloadOffset + 1]);
            m_payloadOffset += 2;
            val = UTF8Encoding.UTF8.GetString(m_payload, m_payloadOffset, len);
            m_payloadOffset += len;
            return this;
        }
        public IPacketReader Read(out short[] val)
        {
            var len = (ushort)(m_payload[m_payloadOffset] << 8 | m_payload[m_payloadOffset + 1]);
            m_payloadOffset += 2;
            val = new short[len];
            for (int i=0; i<len; i++)
            {
                Read(out val[i]);
            }
            return this;
        }
        public IPacketReader Read(out ushort[] val)
        {
            var len = (ushort)(m_payload[m_payloadOffset] << 8 | m_payload[m_payloadOffset + 1]);
            m_payloadOffset += 2;
            val = new ushort[len];
            for (int i=0; i<len; i++)
            {
                Read(out val[i]);
            }
            return this;
        }
        public IPacketReader Read(out int[] val)
        {
            var len = (ushort)(m_payload[m_payloadOffset] << 8 | m_payload[m_payloadOffset + 1]);
            m_payloadOffset += 2;
            val = new int[len];
            for (int i=0; i<len; i++)
            {
                Read(out val[i]);
            }
            return this;
        }
        public IPacketReader Read(out uint[] val)
        {
            var len = (ushort)(m_payload[m_payloadOffset] << 8 | m_payload[m_payloadOffset + 1]);
            m_payloadOffset += 2;
            val = new uint[len];
            for (int i=0; i<len; i++)
            {
                Read(out val[i]);
            }
            return this;
        }
        public IPacketReader Read(out long[] val)
        {
            var len = (ushort)(m_payload[m_payloadOffset] << 8 | m_payload[m_payloadOffset + 1]);
            m_payloadOffset += 2;
            val = new long[len];
            for (int i=0; i<len; i++)
            {
                Read(out val[i]);
            }
            return this;
        }
        public IPacketReader Read(out ulong[] val)
        {
            var len = (ushort)(m_payload[m_payloadOffset] << 8 | m_payload[m_payloadOffset + 1]);
            m_payloadOffset += 2;
            val = new ulong[len];
            for (int i=0; i<len; i++)
            {
                Read(out val[i]);
            }
            return this;
        }
        public IPacketReader Read(out float[] val)
        {
            var len = (ushort)(m_payload[m_payloadOffset] << 8 | m_payload[m_payloadOffset + 1]);
            m_payloadOffset += 2;
            val = new float[len];
            for (int i=0; i<len; i++)
            {
                Read(out val[i]);
            }
            return this;
        }
        public IPacketReader Read(out double[] val)
        {
            var len = (ushort)(m_payload[m_payloadOffset] << 8 | m_payload[m_payloadOffset + 1]);
            m_payloadOffset += 2;
            val = new double[len];
            for (int i=0; i<len; i++)
            {
                Read(out val[i]);
            }
            return this;
        }
        public IPacketReader Read(out byte[][] val)
        {
            var len = (ushort)(m_payload[m_payloadOffset] << 8 | m_payload[m_payloadOffset + 1]);
            m_payloadOffset += 2;
            val = new byte[len][];
            for (int i=0; i<len; i++)
            {
                Read(out val[i]);
            }
            return this;
        }
        public IPacketReader Read(out string[] val)
        {
            var len = (ushort)(m_payload[m_payloadOffset] << 8 | m_payload[m_payloadOffset + 1]);
            m_payloadOffset += 2;
            val = new string[len];
            for (int i=0; i<len; i++)
            {
                Read(out val[i]);
            }
            return this;
        }
        public IPacketReader Read(out DateTime val)
        {
            Read(out long nano);
            var ts = new TimeSpan(nano / 100);
            var st = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            val = st.Add(ts);
            return this;
        }
        public IPacketReader Read(out TimeSpan val)
        {
            Read(out double sec);
            val = TimeSpan.FromSeconds(sec);
            return this;
        }
        public IPacketReader Read(out Room val)
        {
            val = Room.Read(this);
            return this;
        }
        public IPacketReader Read(out User val)
        {
            val = User.Read(this);
            return this;
        }
        public IPacketWriter WriteBool(bool val)
        {
            m_payload[m_payloadOffset] = (byte)(val ? 1 : 0);
            m_payloadOffset++;
            return this;
        }
        public IPacketWriter Write(bool val)
        {
            return WriteBool(val);
        }
        public IPacketWriter WriteByte(byte val)
        {
            m_payload[m_payloadOffset++] = val;
            return this;
        }
        public IPacketWriter Write(byte val)
        {
            return WriteByte(val);
        }
        public IPacketWriter WriteShort(short val)
        {
            m_payload[m_payloadOffset] = (byte)(val >> 8);
            m_payload[m_payloadOffset + 1] = (byte)(val & 0xff);
            m_payloadOffset += 2;
            return this;
        }
        public IPacketWriter Write(short val)
        {
            return WriteShort(val);
        }
        public IPacketWriter WriteUshort(ushort val)
        {
            m_payload[m_payloadOffset] = (byte)(val >> 8);
            m_payload[m_payloadOffset + 1] = (byte)(val & 0xff);
            m_payloadOffset += 2;
            return this;
        }
        public IPacketWriter Write(ushort val)
        {
            return WriteUshort(val);
        }
        public IPacketWriter WriteInt(int val)
        {
            this.WriteShort((short)(val >> 16));
            this.WriteUshort((ushort)(val & 0xffff));
            return this;
        }
        public IPacketWriter Write(int val)
        {
            return WriteInt(val);
        }
        public IPacketWriter WriteUint(uint val)
        {
            this.WriteUshort((ushort)(val >> 16));
            this.WriteUshort((ushort)(val & 0xffff));
            return this;
        }
        public IPacketWriter Write(uint val)
        {
            return WriteUint(val);
        }
        public IPacketWriter WriteLong(long val)
        {
            this.WriteInt((int)(val >> 32));
            this.WriteUint((uint)(val & 0xffffffff));
            return this;
        }
        public IPacketWriter Write(long val)
        {
            return WriteLong(val);
        }
        public IPacketWriter WriteUlong(ulong val)
        {
            this.WriteUint((uint)(val >> 32));
            this.WriteUint((uint)(val & 0xffffffff));
            return this;
        }
        public IPacketWriter Write(ulong val)
        {
            return WriteUlong(val);
        }
        public IPacketWriter WriteFloat(float val)
        {
            var fBytes = BitConverter.GetBytes(val);
            Array.Reverse(fBytes);
            fBytes.CopyTo(m_payload, m_payloadOffset);
            m_payloadOffset += 4;
            return this;
        }
        public IPacketWriter Write(float val)
        {
            return WriteFloat(val);
        }
        public IPacketWriter WriteDouble(double val)
        {
            var dBytes = BitConverter.GetBytes(val);
            Array.Reverse(dBytes);
            dBytes.CopyTo(m_payload, m_payloadOffset);
            m_payloadOffset += 8;
            return this;
        }
        public IPacketWriter Write(double val)
        {
            return WriteDouble(val);
        }
	public IPacketWriter Write(byte[] val)
        {
            ushort len = (ushort)(val.Length & 0xffff);
            m_payload[m_payloadOffset] = (byte)(len >> 8);
            m_payload[m_payloadOffset + 1] = (byte)(len & 0xff);
            m_payloadOffset += 2;
            val.CopyTo(m_payload, m_payloadOffset);
            m_payloadOffset += len;
            return this;
        }
        public IPacketWriter Write(short[] val)
        {
            ushort len = (ushort)(val.Length & 0xffff);
            m_payload[m_payloadOffset] = (byte)(len >> 8);
            m_payload[m_payloadOffset + 1] = (byte)(len & 0xff);
            m_payloadOffset += 2;
            for (int i=0; i<val.Length; i++)
            {
                WriteShort(val[i]);
            }
            return this;
        }
        public IPacketWriter Write(ushort[] val)
        {
            ushort len = (ushort)(val.Length & 0xffff);
            m_payload[m_payloadOffset] = (byte)(len >> 8);
            m_payload[m_payloadOffset + 1] = (byte)(len & 0xff);
            m_payloadOffset += 2;
            for (int i=0; i<val.Length; i++)
            {
                WriteUshort(val[i]);
            }
            return this;
        }
        public IPacketWriter Write(int[] val)
        {
            ushort len = (ushort)(val.Length & 0xffff);
            m_payload[m_payloadOffset] = (byte)(len >> 8);
            m_payload[m_payloadOffset + 1] = (byte)(len & 0xff);
            m_payloadOffset += 2;
            for (int i=0; i<val.Length; i++)
            {
                WriteInt(val[i]);
            }
            return this;
        }
        public IPacketWriter Write(uint[] val)
        {
            ushort len = (ushort)(val.Length & 0xffff);
            m_payload[m_payloadOffset] = (byte)(len >> 8);
            m_payload[m_payloadOffset + 1] = (byte)(len & 0xff);
            m_payloadOffset += 2;
            for (int i=0; i<val.Length; i++)
            {
                WriteUint(val[i]);
            }
            return this;
        }
        public IPacketWriter Write(long[] val)
        {
            ushort len = (ushort)(val.Length & 0xffff);
            m_payload[m_payloadOffset] = (byte)(len >> 8);
            m_payload[m_payloadOffset + 1] = (byte)(len & 0xff);
            m_payloadOffset += 2;
            for (int i=0; i<val.Length; i++)
            {
                WriteLong(val[i]);
            }
            return this;
        }
        public IPacketWriter Write(ulong[] val)
        {
            ushort len = (ushort)(val.Length & 0xffff);
            m_payload[m_payloadOffset] = (byte)(len >> 8);
            m_payload[m_payloadOffset + 1] = (byte)(len & 0xff);
            m_payloadOffset += 2;
            for (int i=0; i<val.Length; i++)
            {
                WriteUlong(val[i]);
            }
            return this;
        }
        public IPacketWriter Write(float[] val)
        {
            ushort len = (ushort)(val.Length & 0xffff);
            m_payload[m_payloadOffset] = (byte)(len >> 8);
            m_payload[m_payloadOffset + 1] = (byte)(len & 0xff);
            m_payloadOffset += 2;
            for (int i=0; i<val.Length; i++)
            {
                WriteFloat(val[i]);
            }
            return this;
        }
        public IPacketWriter Write(double[] val)
        {
            ushort len = (ushort)(val.Length & 0xffff);
            m_payload[m_payloadOffset] = (byte)(len >> 8);
            m_payload[m_payloadOffset + 1] = (byte)(len & 0xff);
            m_payloadOffset += 2;
            for (int i=0; i<val.Length; i++)
            {
                WriteDouble(val[i]);
            }
            return this;
        }
        public IPacketWriter Write(string val)
        {
            var bytes = UTF8Encoding.UTF8.GetBytes(val);
            ushort len = (ushort)(bytes.Length & 0xffff);
            m_payload[m_payloadOffset] = (byte)(len >> 8);
            m_payload[m_payloadOffset + 1] = (byte)(len & 0xff);
            m_payloadOffset += 2;
            bytes.CopyTo(m_payload, m_payloadOffset);
            m_payloadOffset += len;
            return this;
        }
        public IPacketWriter Write(string[] val)
        {
            ushort len = (ushort)(val.Length & 0xffff);
            m_payload[m_payloadOffset] = (byte)(len >> 8);
            m_payload[m_payloadOffset + 1] = (byte)(len & 0xff);
            m_payloadOffset += 2;
            for (int i=0; i<val.Length; i++)
            {
                Write(val[i]);
            }
            return this;
        }
        public IPacketWriter Write(DateTime val)
        {
            return WriteLong(val.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks*100);
        }
        public IPacketWriter Write(TimeSpan val)
        {
            return WriteDouble(val.TotalSeconds);
        }
        public IPacketWriter Write(Packet pkt)
        {
            Array.Copy(pkt.m_payload, 3, m_payload, m_payloadOffset, pkt.m_payloadOffset - 3);
            m_payloadOffset += (ushort)(pkt.m_payloadOffset - 3);
            return this;
        }
    }
    public partial class Packet { 
        protected byte m_header;
        protected int m_payloadLen;
        protected byte[] m_payload;
        protected int m_payloadOffset;

        public Packet()
        {
            m_payload = new byte[0x2000];
            m_payloadOffset = 3;
        }
        public Packet(byte header, int len, byte[] payload)
        {
            m_header = header;
            m_payloadLen = len;
            m_payload = new byte[m_payloadLen];
            Array.Copy(payload, m_payload, m_payloadLen);
            m_payloadOffset = 3;
        }
        public IPacketWriter Send(out byte[]val, out int index, out int len)
        {
            var commandType = CommandType();
            val = new byte[m_payloadOffset + 3];
            index = 0; len = 1;
            switch (commandType)
            {
                case (byte)HeaderCommand.Hello:
                case (byte)HeaderCommand.Closing:
                case (byte)HeaderCommand.Closed:
                    // header only
                    val[0] = m_header;
                    break;
                case (byte)HeaderCommand.Builtin:
                case (byte)HeaderCommand.Original:
                    val[0] = m_header;
                    m_payloadLen = m_payloadOffset;
                    var lenBytes = BitConverter.GetBytes((ushort)m_payloadOffset);
                    Array.Reverse(lenBytes);
                    lenBytes.CopyTo(val, 1);
                    Array.Copy(m_payload, 3, val, 3, m_payloadOffset);
                    index = 0; len = m_payloadOffset;
                    break;
            }
            return this;
        }
        protected byte CommandType()
        {
            return (byte)(m_header & 0x0f);
        }
    }
    public class GamePacket: Packet, IGamePacketReader, IPacketWriter
    {
        protected readonly ushort m_eventCode;
        public GamePacket(ushort eventCode)
        {
            m_eventCode = eventCode;
            m_header = 0x39;
            byte []b = BitConverter.GetBytes(eventCode);
            m_payload[3] = b[1]; m_payload[4] = b[0];
            m_payloadOffset = 5;
        }
        public GamePacket(byte header, ushort eventCode)
        {
            m_eventCode = eventCode;
            m_header = header;
            byte []b = BitConverter.GetBytes(eventCode);
            m_payload[3] = b[1]; m_payload[4] = b[0];
            m_payloadOffset = 5;
        }
        public GamePacket(byte header, int len, byte []payload): base(header, len, payload)
        {
            m_eventCode = (ushort)((payload[3] << 8) | payload[4]);
            Seek(5);
        }
        public ushort EventCode 
        {
            get => m_eventCode;
        }
        public int PayloadLen
        {
            get => m_payloadLen;
        }
        public bool IsBuiltin
        {
            get => (m_header & 0xf) == (byte)HeaderCommand.Builtin;
        }
        public IPacketWriter WriteEventCode(ushort code)
        {
            byte []b = BitConverter.GetBytes(code);
            m_payload[3] = b[1]; m_payload[4] = b[0];
            m_payloadOffset = 5;
            return this;
        }
        public new IPacketWriter Reset()
        {
            m_payloadOffset = 3;
            return this;
        }
    }

    public partial class UserMessage 
    {
        /// <summary>
        /// コマンドコード
        /// </summary>
        public ushort Command { get { return command; } }
        /// <summary>
        /// 送信元ユーザー
        /// </summary>
        public ushort OriginSessionId { get { return originId; } }
        /// <summary>
        /// 送信先の種類
        /// </summary>
        public PacketDestination DestinationType { get { return destinationType; } }
        /// <summary>
        /// 送信先ID
        /// </summary>
        public ushort DestinationId { get { return destination; } }
        /// <summary>
        /// SDK側に届いた時点
        /// </summary>
        public DateTime ReceivedAt { get { return receivedAt; } }

        private readonly GamePacket gamePacket;
        private readonly ushort originId;
        private readonly PacketDestination destinationType;
        private readonly ushort destination;
        private readonly ushort command;
        private readonly DateTime receivedAt;

        internal UserMessage(IPacketReader r)
        {
            gamePacket = (GamePacket)r;
            gamePacket.Read(out originId).Read(out byte desType).Read(out destination);
            destinationType = (PacketDestination)desType;
            gamePacket.Read(out command);
            receivedAt = DateTime.UtcNow;
        }
    }
    public partial class UserMessage : IPacketReader
    {
        public IPacketReader Read(out bool val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }

        public IPacketReader Read(out byte val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }

        public IPacketReader Read(out short val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }

        public IPacketReader Read(out ushort val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }

        public IPacketReader Read(out int val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }

        public IPacketReader Read(out uint val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }

        public IPacketReader Read(out long val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }

        public IPacketReader Read(out ulong val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }

        public IPacketReader Read(out float val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }

        public IPacketReader Read(out double val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }

        public IPacketReader Read(out byte[] val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }

        public IPacketReader Read(out string val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }

        public IPacketReader Read(out DateTime val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }

        public IPacketReader Read(out TimeSpan val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }

        public IPacketReader Read(out User val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }

        public IPacketReader Read(out Room val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }

        public IPacketReader Read(out short[] val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }

        public IPacketReader Read(out ushort[] val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }

        public IPacketReader Read(out int[] val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }

        public IPacketReader Read(out uint[] val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }

        public IPacketReader Read(out long[] val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }

        public IPacketReader Read(out ulong[] val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }

        public IPacketReader Read(out float[] val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }

        public IPacketReader Read(out double[] val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }

        public IPacketReader Read(out string[] val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }

        public IPacketReader Read(out byte[][] val)
        {
            return ((IPacketReader)gamePacket).Read(out val);
        }
    }
}
