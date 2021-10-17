using System;
using System.Collections;
using System.Net;
using System.Net.Sockets.Kcp;

namespace Hybs.Realtime
{
    abstract partial class Client
    {
        protected Client(Region region, Environment environment = Environment.Production)
        {
            _region = region;
            _env = environment;

            _connected = false;
            _heartbeatInterval = TimeSpan.FromSeconds(1);
            _lastHeartbeat = DateTime.UtcNow;
            _udpSession = new UDPSession();
            _sessionId = 0;
            _sendBuffer = new byte[c_sendBufferSize];
            _recvBuffer = new byte[c_recvBufferSize];
            _sendBufferLen = 0;
            _recvBufferLen = 0;
            _recvBufferStatus = 0;
            _recvBufferPayloadLen = 0;
            _recvBufferPayloadRemain = 0;
            _failedCount = 0;

            _centralClient = new Central.Client(_region, _env);
            _controller = new Controller();
        }
        ~Client()
        {
            Stop();
        }

        /// <summary>
        /// ユーザー認証情報を設定
        /// </summary>
        /// <param name="token">文字列のトークン</param>
        protected void Authentication(string token)
        {
            _credential = Credential.FromBase64String(token);
        }
        /// <summary>
        /// クライアントを起動
        /// </summary>
        public void Start()
        {
            var lobbyServerResp = _centralClient.GetLobbyServer(_credential.AppId);
            ServerConnectDescriptor lobbyServer = null;
            if (lobbyServerResp.MoveNext())
            {
                lobbyServer = lobbyServerResp.Current;
            }
            if (lobbyServer == null)
            {
                throw new Exception("fetch lobby server endpoint failed");
            }

#if DEBUG
            lobbyServer.host = "127.0.0.1";
#endif

            _controller.MakeHandlerTable();
            _udpSession.Connect(lobbyServer.host, lobbyServer.port);
            SendAuthentication();

            _connected = true;
        }
        protected DateTime m_lastUpdatedAt = DateTime.Now;
        /// <summary>
        /// 
        /// </summary>
        public void Update()
        {
            if (_connected)
            {
                _udpSession.Update();

                Receive();

                if (DateTime.UtcNow >= _lastHeartbeat.Add(_heartbeatInterval))
                {
                    SendHeartbeat();
                }
            }
            else
            {
                _lastHeartbeat = DateTime.UtcNow;
            }
        }
        public void Stop()
        {
            if (!_connected)
            {
                return ;
            }
            // send closing message
            Send(new byte[] { packetHeaderCmdClosing }, 0, 1);
            _connected = false;
        }
        /// <summary>
        /// 処理ロジックを登録する
        /// </summary>
        /// <param name="code">コード</param>
        /// <param name="h">処理メソッド</param>
        public void Handle(ushort code, PacketHandler h)
        {
            _controller.RegisterHandler(code, h);
        }
        /// <summary>
        /// 処理ロジック（共通部分）を登録する
        /// </summary>
        /// <param name="code">イベントコード</param>
        /// <param name="mask">セグメントマスク(0-16)。0=ルート, 16=リーフ</param>
        /// <param name="m">ミドルウェア関数</param>
        public void Use(ushort code, int mask, PacketMiddleware m)
        {
            _controller.RegisterMiddleware(code, mask, m);
        }
        /// <summary>
        /// パケットを送信する
        /// </summary>
        /// <param name="pkt">パケット</param>
        public void Send(IPacketWriter pkt)
        {
            pkt.Send(out byte[] val, out int index, out int len);
            Send(val, index, len);
        }
        /// <summary>
        /// 生データを送信する
        /// </summary>
        /// <param name="val">データ配列</param>
        /// <param name="index">始まるインデックス</param>
        /// <param name="len">長さ</param>
        public void Send(byte[] val, int index, int len)
        {
            while (index < len)
            {
                var n = _udpSession.Send(val, index, len);
                index += n;
                len -= n;
                if (n < 1)
                {
                    throw new Exception("パケット送信に失敗しました");
                }
            }
        }
    }

    /// <summary>
    /// rUDP通信を行うクライアントの実装
    /// </summary>
    abstract partial class Client
    {
        protected Region _region;
        protected Environment _env;

        protected UDPSession _udpSession;
        protected bool _connected;
        protected TimeSpan _heartbeatInterval;
        protected DateTime _lastHeartbeat;
        protected ushort _sessionId;
        protected Credential _credential;

        protected const ushort c_recvBufferSize = 0xff00;
        protected const int c_sendBufferSize = 0xff00;
        protected const ushort c_frameMTU = 540;
        protected byte[] _sendBuffer;
        protected int _sendBufferLen;
        protected byte[] _recvBuffer;
        protected ushort _recvBufferLen;
        protected byte _recvBufferStatus;

        protected byte _bufferHeader;
        protected ushort _recvBufferPayloadLen;
        protected ushort _recvBufferPayloadRemain;
        protected int _failedCount;

        protected Central.Client _centralClient;
        internal Controller _controller;
    }

    public partial class Client
    {
        protected void SendAuthentication()
        {
            var bytes = new byte[_credential.Base64Token.Length + 3];
            bytes[0] = packetHeaderCmdAuthenticate;
            bytes[1] = (byte)((_credential.Base64Token.Length >> 8) & 0xff);
            bytes[2] = (byte)(_credential.Base64Token.Length & 0xff);
            Array.Copy(_credential.Base64Token, 0, bytes, 3, _credential.Base64Token.Length);
            Send(bytes, 0, _credential.Base64Token.Length + 3);
        }
        protected void SendHeartbeat()
        {
            _lastHeartbeat = DateTime.UtcNow;
            var dt = BitConverter.GetBytes(_lastHeartbeat.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks*100);
            var bytes = new byte[1 + dt.Length];
            bytes[0] = 0;
            Array.Copy(dt, 0, bytes, 1, dt.Length);
            Send(bytes, 0, bytes.Length);
        }
        protected void Receive()
        {
            while (true)
            {
                int n;
                if (_recvBufferStatus == packetStatusHeader)
                {
                    // read header
                    n = _udpSession.Recv(_recvBuffer, _recvBufferLen, 1);
                    if (n < 1)
                    {
                        break;
                    }
                    _bufferHeader = _recvBuffer[_recvBufferLen++];
                    ProcessPacketHeader();
                }
                else if (_recvBufferStatus == packetStatusKeepAlive)
                {
                    n = _udpSession.Recv(_recvBuffer, _recvBufferLen, 1);
                    if (n < 1)
                    {
                        break;
                    }
                    var keepAliveSeconds = _recvBuffer[_recvBufferLen++];
                    _heartbeatInterval = TimeSpan.FromSeconds(keepAliveSeconds);
                    _recvBufferStatus = packetStatusSessionIdHigh;
                }
                else if (_recvBufferStatus == packetStatusSessionIdHigh)
                {
                    n = _udpSession.Recv(_recvBuffer, _recvBufferLen, 1);
                    if (n < 1)
                    {
                        break;
                    }
                    _recvBufferStatus = packetStatusSessionIdLow;
                    _sessionId = _recvBuffer[_recvBufferLen++];
                }
                else if (_recvBufferStatus == packetStatusSessionIdLow)
                {
                    n = _udpSession.Recv(_recvBuffer, _recvBufferLen, 1);
                    if (n < 1)
                    {
                        break;
                    }
                    _recvBufferStatus = packetStatusHeader;
                    _sessionId = (ushort)(_sessionId << 8 | _recvBuffer[_recvBufferLen++]);
                    _recvBufferLen = 0;
                }
                else if (_recvBufferStatus >= packetStatusNTP && _recvBufferStatus < packetStatusNTP + 0x10)
                {
                    n = _udpSession.Recv(_recvBuffer, _recvBufferLen, packetStatusNTP + 0x10 - _recvBufferStatus);
                    if (n < 1)
                    {
                        break;
                    }
                    _recvBufferLen += (ushort)n;
                    _recvBufferStatus += (byte)n;
                    if (_recvBufferStatus == packetStatusNTP + 0x10)
                    {
                        var nanoUnix = BitConverter.ToInt64(_recvBuffer, _recvBufferLen - 16);
                        var ts = new TimeSpan(nanoUnix / 100);
                        var st = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        var dt = st.Add(ts);
                        var newLatency = DateTime.UtcNow.Subtract(dt).TotalMilliseconds;
                        _recvBufferLen = 0;
                        _recvBufferStatus = packetStatusHeader;
                    }
                }
                else if (_recvBufferStatus == packetStatusPayloadLenHigh)
                {
                    // read payload len high bits
                    n = _udpSession.Recv(_recvBuffer, _recvBufferLen, 1);
                    if (n < 1)
                    {
                        break;
                    }
                    _recvBufferPayloadLen = _recvBuffer[_recvBufferLen++];
                    _recvBufferStatus = packetStatusPayloadLenLow;
                }
                else if (_recvBufferStatus == packetStatusPayloadLenLow)
                {
                    // read payload len low bits
                    n = _udpSession.Recv(_recvBuffer, _recvBufferLen, 1);
                    if (n < 1)
                    {
                        break;
                    }
                    var payLoadLenPart2 = _recvBuffer[_recvBufferLen++];
                    _recvBufferPayloadLen = (ushort)((_recvBufferPayloadLen << 8) | payLoadLenPart2);
                    _recvBufferPayloadRemain = _recvBufferPayloadLen;
                    _recvBufferStatus = packetStatusPayload;
                }
                else if (_recvBufferStatus == packetStatusPayload)
                {
                    // read payload bytes
                    n = _udpSession.Recv(_recvBuffer, _recvBufferLen, _recvBufferPayloadRemain);
                    if (n < 1)
                    {
                        break;
                    }
                    _recvBufferLen += (ushort)n;
                    _recvBufferPayloadRemain -= (ushort)n;
                    if (_recvBufferPayloadRemain == 0)
                    {
                        var packet = new GamePacket(_bufferHeader, _recvBufferLen, _recvBuffer);
                        ProcessPacketPayload(packet);
                        _recvBufferLen = 0;
                        _recvBufferStatus = packetStatusHeader;
                    }
                }
                else
                {
                    break;
                }
            }
        }
        protected void ProcessPacketHeader()
        {
            switch (_bufferHeader & 0x0f)
            {
                case packetHeaderCmdHello:
                    // ただのネットワーク疎通確認
                    _recvBufferLen = 0;
                    break;
                case packetHeaderCmdClosing:
                case packetHeaderCmdClosed:
                    GC.Collect();
                    break;
                case packetHeaderCmdEstablished:
                    // established コネクション確立完了
                    _recvBufferStatus = packetStatusKeepAlive;
                    break;
                case packerHeaderCmdNTP:
                    _recvBufferStatus = packetStatusNTP;
                    break;
                case packetHeaderCmdBuiltin:
                case packetHeaderCmdOriginal:
                    _recvBufferStatus = packetStatusPayloadLenHigh;
                    break;
                default:
                    _recvBufferStatus = packetStatusHeader;
                    break;
            }
        }
        protected virtual void ProcessPacketPayload(IPacketReader r)
        {
            throw new NotImplementedException();
        }

        protected const byte packetStatusHeader = 0x00;
        protected const byte packetStatusKeepAlive = 0x30;
        protected const byte packetStatusSessionIdHigh = 0x31;
        protected const byte packetStatusSessionIdLow = 0x32;
        protected const byte packetStatusNTP = 0x50;
        protected const byte packetStatusPayloadLenHigh = 0xc0;
        protected const byte packetStatusPayloadLenLow = 0xc1;
        protected const byte packetStatusPayload = 0xc2;

        protected const byte packetHeaderCmdHello = 0x0;
        protected const byte packetHeaderCmdClosing = 0x1;
        protected const byte packetHeaderCmdClosed = 0x2;
        protected const byte packetHeaderCmdEstablished = 0x3;
        protected const byte packetHeaderCmdAuthenticate = 0x4;
        protected const byte packerHeaderCmdNTP = 0x5;
        protected const byte packetHeaderCmdProxy = 0x6;
        protected const byte packetHeaderCmdRpc = 0x7;
        protected const byte packetHeaderCmdBuiltin = 0x8;
        protected const byte packetHeaderCmdOriginal = 0x9;

    }
}
