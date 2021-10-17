using System;

namespace Hybs.Realtime
{
    /// <summary>
    /// ビルトインイベントコード
    /// 0x0000-0x0fff：クライアント → サーバー
    /// 0x8000-0x8fff：サーバー → クライアント
    /// </summary>
    public static class EventCode
    {
        #region request
        public static readonly ushort UserValue = 0x00fe;
        public static readonly ushort UserMessage = 0x00ff;
        public static readonly ushort RoomCreate = 0x0100;
        public static readonly ushort RoomEnter = 0x0101;
        public static readonly ushort RoomExit = 0x0102;
        public static readonly ushort RoomLock = 0x0103;
        public static readonly ushort RoomUnlock = 0x0104;
        public static readonly ushort RoomMatch = 0x0110;
        public static readonly ushort RoomBroadcast = 0x01ff;
        #endregion

        #region response
        public static readonly ushort OnUserUpdated = 0x8000;
        public static readonly ushort OnUserEmoji = 0x8001;
        public static readonly ushort OnUserEnteredRoom = 0x8002;
        public static readonly ushort OnUserExitedRoom = 0x8003;
        public static readonly ushort OnUserValue = 0x80fe;
        public static readonly ushort OnUserMessage = 0x80ff;
        
        public static readonly ushort OnRoomCreated = 0x8100;
        public static readonly ushort OnRoomUpdated = 0x8101;
        public static readonly ushort OnRoomRemoved = 0x8102;
        public static readonly ushort OnRoomLocked = 0x8103;
        public static readonly ushort OnRoomUnlocked = 0x8104;
        public static readonly ushort OnRoomUserMessage = 0x81ff;

        public static readonly ushort OnErrorServer = 0x8ffe;
        public static readonly ushort OnErrorClient = 0x8fff;
        #endregion
    }
    /// <summary>
    /// コマンドタイプ
    /// </summary>
    internal enum HeaderCommand: byte
    {
        Hello = 0x0,
        Closing = 0x1,
        Closed = 0x2,
        Establlished = 0x3,
        Authenticate = 0x4,
        NTP = 0x5,
        Proxy = 0x6,
        Forward = 0x7,
        Builtin = 0x8, // ビルトイン処理を呼び出す
        Original = 0x9, // ユーザー定義関数を呼び出す
    }
    internal static class Header
    {
        public static readonly byte RequestBuiltin = (byte)HeaderCommand.Builtin;
    }

    /// <summary>
    /// クライアント側エラー定義
    /// </summary>
    public static class ClientError
    {
        /// <summary>
        /// 共通コード：特に区別しないエラー。不正リクエストやデータ不整合
        /// </summary>
        public static readonly ushort BadRequest = 0x0000;
    }

    /// <summary>
    /// サーバー側エラー定義
    /// </summary>
    public static class ServerError
    {
        /// <summary>
        /// 共通コード：特に区別しないエラー。サーバー側内部処理にエラーが出る
        /// </summary>
        public static readonly ushort Internal = 0xffff;
    }

    /// <summary>
    /// 送信するターゲット
    /// </summary>
    public enum PacketDestination: byte
    {
        /// <summary>
        /// 自分へ送信する
        /// </summary>
        Self = 0,
        /// <summary>
        /// 送信先を指定する
        /// </summary>
        Specified = 1,
        /// <summary>
        /// ルーム全員へ送信する
        /// </summary>
        Room = 2,
        /// <summary>
        /// ロビー全員へ送信する
        /// </summary>
        Lobby = 3,
    }
    
    /// <summary>
    /// ネットワーク層プロトコール（L4-L5）
    /// </summary>
    enum NetworkProtocol: byte
    {
        /// <summary>
        /// KCPはサポーターしている
        /// </summary>
        Kcp,
        /// <summary>
        /// TODO
        /// </summary>
        Quic,
        /// <summary>
        /// TODO
        /// </summary>
        WebSocket,
    }
    
    /// <summary>
    /// サーバー接続情報
    /// </summary>
    [Serializable]
    public class ServerConnectDescriptor
    {
        public string scheme;
        public string host;
        public int port;
        public string key;
        public string options;
    }
}
