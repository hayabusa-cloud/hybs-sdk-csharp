using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hybs.Realtime
{
    public partial class User
    {
        public string id { get; internal set; }
        public UserStatus status { get; internal set; }
        public ushort roomId { get; internal set; }
        public ushort roomIndex { get; internal set; }
        internal ushort sessionId;
    }
    public partial class User : IEquatable<User>
    {
        protected static Dictionary<ushort, User> userDict;
        public void InitUserManager()
        {
            userDict = new Dictionary<ushort, User>();
        }
        public bool Equals(User x)
        {
            return sessionId == x.sessionId;
        }
        public static User Read(IPacketReader r)
        {
            User ret = new User();
            r.Read(out ret.sessionId);
            r.Read(out byte status); 
            ret.status = (UserStatus)status;
            if (ret.status == UserStatus.InRoom)
            {
		        r.Read(out ushort roomId);
                ret.roomId = roomId;
		        r.Read(out ushort roomIndex);
                ret.roomIndex = roomIndex;
            } else
            {
                ret.roomId = 0;
                ret.roomIndex = 0;
            }
            return ret;
        }
    }
    public enum UserStatus: byte
    {
        InLobby = 0,
        InRoom  = 1,
    }
}
