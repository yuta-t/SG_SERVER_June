using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace AgentServer.Structuring
{
    public static class Rooms
    {
        //private static List<NormalRoom> NormalRoomList = new List<NormalRoom>();
        public static ConcurrentDictionary<int, NormalRoom> RoomList = new ConcurrentDictionary<int, NormalRoom>();
        public static int RoomID = 1;


        public static void AddRoom(int roomsession, NormalRoom room)
        {
            //NormalRoomList.Add(room);
            RoomList.TryAdd(roomsession, room);
        }
        public static bool ExistRoom(int roomsession)
        {
            return RoomList.ContainsKey(roomsession);
        }

        public static NormalRoom GetRoom(int roomsession)
        {
            if (RoomList.TryGetValue(roomsession, out var getroom))
                return getroom;
            else
                return null;
        }

        public static void RemoveRoom(int roomsession)
        {
            RoomList.TryRemove(roomsession, out _);
            //NormalRoomList.Remove(room);
        }
    }
}
