using System;
using System.Collections.Generic;

namespace Hybs.Realtime
{
    public partial class Lobby
    {
        public string id { get; internal set; }

        public User self
        {
            get { return _self; }
        }

        internal void UserModify(User user)
        {
            if ( self.Equals(user) )
            {
                _self = user;
            } 
            else
            {
                switch ( user.status )
                {
                    case UserStatus.InLobby:
                    // todo
                    break;
                    case UserStatus.InRoom:
                    if (_rooms.TryGetValue(user.roomId, out Room room))
                    {
                        room.UserUpsert(user);
                    }
                    break;
                }
            }
        }

        public int roomNum
        {
            get { return _rooms.Count; }
        }
        internal void RoomAdd(Room room)
        {
            room.lobby = this;
            _rooms[room.roomId] = room;
        }
        internal void RoomModify(Room room)
        {
            _rooms[room.roomId] = room;
        }
        internal void RoomDel(ushort roomId)
        {
            _rooms.Remove(roomId);
        }
	public bool TryGetRoom(ushort roomId, out Room room)
        {
            return _rooms.TryGetValue(roomId, out room);
        }
    }
    public partial class Lobby
    {
        public Room lastCreatedRoom = null;
        public Room lastEnteredRoom = null;
        public Room lastExitedRoom = null;
        public Room lastLockedRoom = null;
        public Room lastUnlockedRoom = null;
        public User lastEnteredUser = null;
        public User lastExitedUser = null;
        public User triggeredUser = null;
        public ushort triggeringEventCode = 0;
        protected Dictionary<ushort, Room> _rooms;
        protected Dictionary<ushort, User> _knownUsers;
        protected User _self;
    }
    public partial class Lobby
    {
        internal Lobby()
        {
            _rooms = new Dictionary<ushort, Room>();
            _knownUsers = new Dictionary<ushort, User>();
            _self = new User();
        }
    }
}
