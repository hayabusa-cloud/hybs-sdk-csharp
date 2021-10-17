using System;
using System.Collections.Generic;

namespace Hybs.Realtime
{
    public enum RoomType: byte
    {
        Normal = 0,
        Match = 1,
    }
    public enum RoomStatus: byte
    {
        Openning = 0,
        Locked = 1,
    }

    public partial class Room
    {
        internal bool UserEnter(ref User user)
        {
            user.status = UserStatus.InRoom;
            user.roomId = roomId;
            m_userDict[user.sessionId] = user;
            lobby.lastEnteredUser = user;
            lobby.lastEnteredRoom = this;
            return true;
        }
        internal bool UserExit(ushort sessionId)
        {
            if (m_userDict.TryGetValue(sessionId, out User user))
            {
                user.roomId = 0;
                user.status = UserStatus.InLobby;
                m_userDict.Remove(sessionId);
                lobby.lastExitedUser = user;
                lobby.lastExitedRoom = this;
                return true;
            }
            return false;
        }
        internal void UserUpsert(User user)
        {
            m_userDict[user.sessionId] = user;
        }
        public bool TryGetUser(ushort sessionId, out User user)
        {
            return m_userDict.TryGetValue(sessionId, out user);
        }
        internal bool Lock()
        {
            status = RoomStatus.Locked;
            lobby.lastLockedRoom = this;
            return true;
        }
        internal bool Unlock()
        {
            status = RoomStatus.Openning;
            lobby.lastUnlockedRoom = this;
            return true;
        }
        public Lobby lobby { get; internal set; }
        public RoomType type { get; private set; }
        public RoomStatus status { get; private set; }
        public ushort roomId { get; private set; }
        public ushort ownerId { get; private set; }
        public TimeSpan elapsedTime { get { return (DateTime.Now-m_createdAt); } }
        public ushort capacity { get; private set; }
        public int currentUserNum { get { return m_userDict.Count; } }
        public Dictionary<ushort, User> users { get { return m_userDict; } }
    }
    public partial class Room
    {
        protected DateTime m_createdAt;
        protected Dictionary<ushort, User> m_userDict;
    }

    public partial class Room
    {
        internal Room(RoomType type, ushort roomId, ushort capacity, ushort ownerId, Dictionary<ushort, User> userDict)
        {
            room(type, roomId, capacity, ownerId, userDict);
        }
        internal void Dispose()
        {
        }
        public static Room Read(IPacketReader r)
        {
            r.Read(out ushort roomId).Read(out byte roomType).Read(out ushort capacity).Read(out ushort ownerId);
            // read user list
            var userList = new Dictionary<ushort, User>();
            while (true)
            {
                r.Read(out ushort id).Read(out ushort pos); 
                if (id == 0xffff && pos == 0xffff)
                {
                    break;
                }
                var newUser = new User();
                newUser.sessionId = id;
                newUser.roomId = roomId;
                newUser.roomIndex = pos;
                newUser.status = UserStatus.InRoom;
                userList.Add(newUser.sessionId, newUser);
            }
            return new Room((RoomType)roomType, roomId, capacity, ownerId, userList );
        }
        private void room(RoomType type, ushort roomId, ushort capacity, ushort ownerId, Dictionary<ushort, User> userDict)
        {
            this.type = type;
            if ( type == RoomType.Normal )
            {
                status = RoomStatus.Openning;
            }
            if ( type == RoomType.Match )
            {
                status = RoomStatus.Openning;
            }
            this.roomId = roomId; this.capacity = capacity; this.ownerId = ownerId;

            m_createdAt = DateTime.Now;
            m_userDict = userDict;
        }
    }
}
