using System;
using System.Collections.Generic;
using System.Net.Sockets.Kcp;

namespace Hybs.Realtime
{
    public delegate void PacketHandler(IPacketReader r);
    public delegate PacketHandler PacketMiddleware(PacketHandler h);
    public delegate void UserMessagePacketHandler(UserMessage msg);


    public class Controller
    {

        protected static PacketMiddleware Combine(PacketMiddleware l, PacketMiddleware r)
        {
            return (PacketHandler h) => l(r(h));
        }

        protected Dictionary<ushort, PacketHandler> m_rawHandlerTable;
        protected Dictionary<ushort, PacketMiddleware[]> m_rawMiddlewareTable;
        protected Dictionary<ushort, PacketHandler> m_handlerTable;
        protected Dictionary<ushort, PacketHandler> m_builtinHandlerTable;
        public Controller()
        {
            m_rawHandlerTable = new Dictionary<ushort, PacketHandler>();
            m_rawMiddlewareTable = new Dictionary<ushort, PacketMiddleware[]>();
            m_builtinHandlerTable = new Dictionary<ushort, PacketHandler>();
        }
        public bool Handle(IGamePacketReader r)
        {
            var code = r.EventCode;
            if (!m_handlerTable.TryGetValue(code, out PacketHandler h))
            {
                return false;
            }
            else
            {
                h(r);
                return true;
            }
        }
        /// <summary>
        /// ハンドローラー関数をコントローラーに登録
        /// </summary>
        /// <param name="code">イベントコード</param>
        /// <param name="h">ハンドローラーメソッド</param>
        public void RegisterHandler(ushort code, PacketHandler h)
        {
            if (m_rawHandlerTable.TryGetValue(code, out PacketHandler val))
            {
                val += h;
                m_rawHandlerTable[code] = val;
            }
            else
            {
                m_rawHandlerTable.Add(code, h);
            }
        }
        /// <summary>
        /// 共通処理をコントローラーに登録
        /// </summary>
        /// <param name="code">イベントセグメント</param>
        /// <param name="mask">セグメントマスク(0-15)。0:root 1:leaf</param>
        /// <param name="m">ミドルウェアメソッド</param>
        public void RegisterMiddleware(ushort code, int mask, PacketMiddleware m)
        {
            mask &= 0x0f;
            code = (ushort)((code >> (0x10 - mask)) << (0x10 - mask));
            if (!m_rawMiddlewareTable.TryGetValue(code, out PacketMiddleware[] middlewareMap))
            {
                middlewareMap = new PacketMiddleware[0x10];
                m_rawMiddlewareTable.Add(code, middlewareMap);
            }
            if (middlewareMap[mask] == null) 
            {
                middlewareMap[mask] = m;
            }
            else
            {
                middlewareMap[mask] = Combine(middlewareMap[mask], m);
            }
        }
        /// <summary>
        /// 登録したハンドローラーとミドルウェアからハンドローラーを本登録
        /// </summary>
        internal void MakeHandlerTable()
        {
            m_handlerTable = new Dictionary<ushort, PacketHandler>();
            foreach (var kv in m_rawHandlerTable)
            {
                var rawHandler = kv.Value;
                for (int segLevel = 0x0f; segLevel >= 0; segLevel--)
                {
                    var seg = (ushort)(kv.Key >> (0x10 - segLevel) << (0x10 - segLevel));
                    if (!m_rawMiddlewareTable.TryGetValue(seg, out PacketMiddleware[] middlewareMap))
                    {
                        continue;
                    }
                    if (middlewareMap[segLevel] == null)
                    {
                        continue;
                    }
                    rawHandler = middlewareMap[segLevel](rawHandler);
                }
                m_handlerTable.Add(kv.Key, rawHandler);
            }
        }
        /// <summary>
        /// ビルトイン処理を呼び出す
        /// </summary>
        /// <param name="r">メッセージパケット</param>
        /// <returns></returns>
        internal bool HandleBuiltin(IGamePacketReader r)
        {
            var code = r.EventCode;
            if (!m_builtinHandlerTable.TryGetValue(code, out PacketHandler h))
            {
                return false;
            }
            else
            {
                h(r);
                return true;
            }
        }
        /// <summary>
        /// ビルトイン処理を登録する
        /// </summary>
        /// <param name="code">イベントコード</param>
        /// <param name="h">ハンドローラーメソッド</param>
        internal void RegisterHandlerBuiltin(ushort code, PacketHandler h)
        {
            if (m_builtinHandlerTable.TryGetValue(code, out PacketHandler val))
            {
                val += h;
                m_builtinHandlerTable[code] = val;
            }
            else
            {
                m_builtinHandlerTable.Add(code, h);
            }
        }
    }
}
