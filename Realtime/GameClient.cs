using System;
using System.Collections.Generic;
using System.IO;

namespace Hybs.Realtime
{
    public partial class GameClient : Client
    {
        public GameClient(Region region, string credential, Environment environment = Environment.Playground) : base(region, environment)
        {
            Authentication(credential);
            BuiltinEvent.Import(this);
        }

        /// <summary>
        /// パケットデータを送信
        /// </summary>
        /// <param name="pkt">送信するパケット</param>
        public void SendPacket(IPacketWriter pkt)
        {
            Send(pkt);
        }
        public void SendPacket(ushort code)
        {
            IPacketWriter pkt = new GamePacket(code);
            SendPacket(pkt);
        }
        public void SendPacket(ushort code, short arg)
        {
            IPacketWriter pkt = new GamePacket(code);
            pkt.Write(arg);
            SendPacket(pkt);
        }
        public void SendPacket(ushort code, ushort arg)
        {
            IPacketWriter pkt = new GamePacket(code);
            pkt.Write(arg);
            SendPacket(pkt);
        }
        public void SendPacket(ushort code, int arg)
        {
            IPacketWriter pkt = new GamePacket(code);
            pkt.Write(arg);
            SendPacket(pkt);
        }
        public void SendPacket(ushort code, uint arg)
        {
            IPacketWriter pkt = new GamePacket(code);
            pkt.Write(arg);
            SendPacket(pkt);
        }
        public void SendPacket(ushort code, float x, float y)
        {
            IPacketWriter pkt = new GamePacket(code);
            pkt.Write(x).Write(y);
            SendPacket(pkt);
        }
        public void SendPacket(ushort code, float x, float y, float z)
        {
            IPacketWriter pkt = new GamePacket(code);
            pkt.Write(x).Write(y).Write(z);
            SendPacket(pkt);
        }
        public void SendPacket(ushort code, float x, float y, float z, float w)
        {
            IPacketWriter pkt = new GamePacket(code);
            pkt.Write(x).Write(y).Write(z).Write(w);
            SendPacket(pkt);
        }
        /// <summary>
        /// 受信時の処理メソッドを登録する
        /// </summary>
        /// <param name="code">イベントコード</param>
        /// <param name="h">処理メソッド。引数：IPacketReader</param>
        public void OnReceivedPacket(ushort code, PacketHandler h)
        {
            Handle(code, h);
        }
        /// <summary>
        /// 受信時の処理メソッドにミドルウェアを付加
        /// </summary>
        /// <param name="code">イベントコード</param>
        /// <param name="m">処理メソッドのミドルウェア</param>
        public void OnReceivedPacket(ushort code, PacketMiddleware m)
        {
            Use(code, 0x10, m);
        }
        /// <summary>
        /// クライアント側オリジナルメッセージの処理メソッド
        /// </summary>
        /// <param name="command">コマンドコード</param>
        /// <param name="h">ハンドラー</param>
        public void OnReceivedUserMessage(ushort command, UserMessagePacketHandler h)
        {
            BuiltinEvent.OnUserMessage(command, h);
        }
        /// <summary>
        /// エラー処理メソッドを登録（リクエストエラー）
        /// </summary>
        /// <param name="code">エラーコード</param>
        /// <param name="h">処理メソッド</param>
        public void OnErrorClient(ushort code, PacketHandler h)
        {
            BuiltinEvent.OnErrorClient(code, h);
        }
        /// <summary>
        /// エラー処理メソッドを登録（サーバー側内部エラー）
        /// </summary>
        /// <param name="code">エラーコード</param>
        /// <param name="h">処理メソッド</param>
        public void OnErrorServer(ushort code, PacketHandler h)
        {
            BuiltinEvent.OnErrorServer(code, h);
        }
        public Lobby lobby { get; private set; }

        public Action UpdateSucceedCallback { get; set; }
        public Action UpdateFailedCallback { get; set; }
        protected override void ProcessPacketPayload(IPacketReader r) 
        {
            lobby.triggeringEventCode = (r as IGamePacketReader).EventCode;
            if ((r as IGamePacketReader).IsBuiltin) {
                _controller.HandleBuiltin((IGamePacketReader)r);
            }
            _controller.Handle((IGamePacketReader)r);
        }
    }

    public partial class GameClient : Client
    {
        public void RoomCreate()
        {
            var pkt = new GamePacket(Header.RequestBuiltin, EventCode.RoomCreate);
            SendPacket(pkt);
        }
        public void RoomEnter(ushort roomId)
        {
            var pkt = new GamePacket(Header.RequestBuiltin, EventCode.RoomEnter);
            pkt.WriteUshort(roomId);
            SendPacket(pkt);
        }
        public void RoomExit()
        {
            var pkt = new GamePacket(Header.RequestBuiltin, EventCode.RoomExit);
            SendPacket(pkt);
        }
        public void RoomLock()
        {
            var pkt = new GamePacket(Header.RequestBuiltin, EventCode.RoomLock);
            SendPacket(pkt);
        }
        public void RoomUnlock()
        {
            var pkt = new GamePacket(Header.RequestBuiltin, EventCode.RoomUnlock);
            SendPacket(pkt);
        }
        public void RoomMatch(string mux, ushort capacity)
        {
            IPacketWriter pkt = new GamePacket(Header.RequestBuiltin, EventCode.RoomMatch);
            pkt.Write(mux).Write(capacity);
            SendPacket(pkt);
        }
        /// <summary>
        /// クライアント側オリジナルメッセージを送信する
        /// </summary>
        /// <param name="command">クライアント側が自分で定義するコマンドコード</param>
        /// <param name="msg">送るメッセージ</param>
        /// <param name="destType">送信先の種類</param>
        /// <param name="destId">送るメッセージ</param>
        public void SendUserMessage(ushort command, PacketDestination destType=PacketDestination.Room, ushort destId=0xffff)
        {
            var pkt = new GamePacket(Header.RequestBuiltin, EventCode.UserMessage);
            pkt.Write((byte)destType).Write(destId);
            pkt.Write(command);
            SendPacket(pkt);
        }
        public void SendUserMessage(ushort command, Packet packet, PacketDestination destType=PacketDestination.Room, ushort destId=0xffff)
        {
            var pkt = new GamePacket(Header.RequestBuiltin, EventCode.UserMessage);
            pkt.Write((byte)destType).Write(destId);
            pkt.Write(command).Write(packet);
            SendPacket(pkt);
        }
        public void SendUserMessage(ushort command, short arg, PacketDestination destType=PacketDestination.Room, ushort destId=0xffff)
        {
            var pkt = new GamePacket(Header.RequestBuiltin, EventCode.UserMessage);
            pkt.Write((byte)destType).Write(destId);
            pkt.Write(command).Write(arg);
            SendPacket(pkt);
        }
        public void SendUserMessage(ushort command, ushort arg, PacketDestination destType=PacketDestination.Room, ushort destId=0xffff)
        {
            var pkt = new GamePacket(Header.RequestBuiltin, EventCode.UserMessage);
            pkt.Write((byte)destType).Write(destId);
            pkt.Write(command).Write(arg);
            SendPacket(pkt);
        }
        public void SendUserMessage(ushort command, int arg, PacketDestination destType=PacketDestination.Room, ushort destId=0xffff)
        {
            var pkt = new GamePacket(Header.RequestBuiltin, EventCode.UserMessage);
            pkt.Write((byte)destType).Write(destId);
            pkt.Write(command).Write(arg);
            SendPacket(pkt);
        }
        public void SendUserMessage(ushort command, uint arg, PacketDestination destType=PacketDestination.Room, ushort destId=0xffff)
        {
            var pkt = new GamePacket(Header.RequestBuiltin, EventCode.UserMessage);
            pkt.Write((byte)destType).Write(destId);
            pkt.Write(command).Write(arg);
            SendPacket(pkt);
        }
        public void SendUserMessage(ushort command, bool arg, PacketDestination destType=PacketDestination.Room, ushort destId=0xffff)
        {
            var pkt = new GamePacket(Header.RequestBuiltin, EventCode.UserMessage);
            pkt.Write((byte)destType).Write(destId);
            pkt.Write(command).Write(arg);
            SendPacket(pkt);
        }
    }
    static partial class BuiltinEvent
    {
        static PacketHandler OnUserUpdated(Lobby lobby) {
            return (IPacketReader r) =>
            {
                User user = User.Read(r);
                lobby.UserModify(user);
            };
        }
        static PacketHandler OnUserEnteredRoom(Lobby lobby) {
            return (IPacketReader r) =>
            {
                User user = User.Read(r);
                // leave old room
                if (user.status == UserStatus.InRoom)
                {
                    if (lobby.TryGetRoom(user.roomId, out Room leavingRoom))
                    {
                        leavingRoom.UserExit(user.sessionId);
                    }
                }
                // enter new room
                if (lobby.TryGetRoom(user.roomId, out Room enteringRoom))
                {
                    enteringRoom.UserEnter(ref user);
                }
                if (user.Equals(lobby.self))
                {
                    lobby.self.roomId = user.roomId;
                    lobby.self.status = user.status;
                }
                lobby.triggeredUser = lobby.lastEnteredUser;
            };
        }
        static PacketHandler OnUserExitedRoom(Lobby lobby) {
            return (IPacketReader r) =>
            {
                r.Read(out ushort sessionId).Read(out ushort roomId);
                if (lobby.TryGetRoom(roomId, out Room leavingRoom))
                {
                    leavingRoom.UserExit(sessionId);
                }
                lobby.triggeredUser = lobby.lastExitedUser;
            };
        }

        static PacketHandler OnRoomCreated(Lobby lobby) {
            return (IPacketReader r) =>
            {
                Room room = Room.Read(r);
                lobby.RoomAdd(room);
            };
        }
        static PacketHandler OnRoomUpdated(Lobby lobby)
        {
            return (IPacketReader r) => { Room room = Room.Read(r); lobby.RoomModify(room); };
        }
        static PacketHandler OnRoomRemoved(Lobby lobby) {
            return (IPacketReader r) =>
             {
                 r.Read(out ushort roomId);
                 lobby.RoomDel(roomId);
             };
        }
        static PacketHandler OnRoomLocked(Lobby lobby) {
            return (IPacketReader r) =>
            {
                r.Read(out ushort roomId);
                if (lobby.TryGetRoom(roomId, out Room room))
                {
                    room.Lock();
                }
            };
        }
        static PacketHandler OnRoomUnlocked(Lobby lobby) {
            return (IPacketReader r) =>
            {
                r.Read(out ushort roomId);
                if (lobby.TryGetRoom(roomId, out Room room))
                {
                    room.Unlock();
                }
            };
        }

        static void onErrorClientInternal(IPacketReader r)
        {
            r.Read(out ushort errorCode);
            clientError.Handle(errorCode, r);
        }
        static void onErrorServerInternal(IPacketReader r)
        {
            r.Read(out ushort errorCode);
            serverError.Handle(errorCode, r);
        }

        static private ErrorManager clientError;
        static private ErrorManager serverError;

        internal static void OnErrorClient(ushort errorCode, PacketHandler h)
        {
            clientError.OnReceived(errorCode, h);
        }
        internal static void OnErrorServer(ushort errorCode, PacketHandler h)
        {
            serverError.OnReceived(errorCode, h);
        }
        internal static void OnUserMessage(ushort command, UserMessagePacketHandler h)
        {
	    if (userMessageHandlerManager.TryGetValue(command, out UserMessagePacketHandler x))
            {
                x += h;
                userMessageHandlerManager[command] = x;
            } else
            {
                userMessageHandlerManager.Add(command, h);
            }
        }

	    private static void onUserMessageInternal(IPacketReader r)
        {
            var userMsg = new UserMessage(r);
	    if (userMessageHandlerManager.TryGetValue(userMsg.Command, out UserMessagePacketHandler h)) {
                h(userMsg);
            }
        }
        private static Dictionary<ushort, UserMessagePacketHandler> userMessageHandlerManager;

        public static void Import(GameClient client)
        {
            client.OnReceivedPacket(EventCode.OnUserUpdated, OnUserUpdated(client.lobby));
            client.OnReceivedPacket(EventCode.OnRoomCreated, OnRoomCreated(client.lobby));
            client.OnReceivedPacket(EventCode.OnRoomUpdated, OnRoomUpdated(client.lobby));
            client.OnReceivedPacket(EventCode.OnRoomRemoved, OnRoomRemoved(client.lobby));
            client.OnReceivedPacket(EventCode.OnRoomLocked, OnRoomLocked(client.lobby));
            client.OnReceivedPacket(EventCode.OnRoomUnlocked, OnRoomUnlocked(client.lobby));
            client.OnReceivedPacket(EventCode.OnUserEnteredRoom, OnUserEnteredRoom(client.lobby));
            client.OnReceivedPacket(EventCode.OnUserExitedRoom, OnUserExitedRoom(client.lobby));

            client.OnReceivedPacket(EventCode.OnErrorClient, onErrorClientInternal);
            client.OnReceivedPacket(EventCode.OnErrorServer, onErrorServerInternal);

            client.OnReceivedPacket(EventCode.OnUserMessage, onUserMessageInternal);

            clientError = new ErrorManager(); serverError = new ErrorManager();
            userMessageHandlerManager = new Dictionary<ushort, UserMessagePacketHandler>();
        }
    }

    class ErrorManager
    {
        public void OnReceived(ushort errorCode, PacketHandler h)
        {
            if (dictHandler.TryGetValue(errorCode, out PacketHandler x))
            {
                x += h;
                dictHandler[errorCode] = x;
            }
            else
            {
                dictHandler.Add(errorCode, h);
            }
        }
        public void Handle(ushort code, IPacketReader r)
        {
            if (dictHandler.TryGetValue(code, out PacketHandler h))
            {
                h(r);
            }
        }

        public ErrorManager()
        {
            dictHandler = new Dictionary<ushort, PacketHandler>();
        }
        private Dictionary<ushort, PacketHandler> dictHandler;
    }
}
