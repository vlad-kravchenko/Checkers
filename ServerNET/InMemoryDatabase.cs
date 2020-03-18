using System.Collections.Generic;

namespace ServerNET
{
    public static class InMemoryDatabase
    {
        public static List<User> ChatUsers { get; set; }
        public static List<Room> ChatRooms { get; set; }

        public static void Setup()
        {
            ChatUsers = new List<User>();
            ChatRooms = new List<Room>();
        }
    }
}