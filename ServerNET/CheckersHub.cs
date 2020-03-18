using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerNET
{
    public class CheckersHub : Hub
    {
        List<User> Users { get { return InMemoryDatabase.ChatUsers; } }
        List<Room> Rooms { get { return InMemoryDatabase.ChatRooms; } }

        public override Task OnConnected()
        {
            Users.Add(new User
            {
                UserName = Context.Headers["UserName"],
                ConnectionId = Context.ConnectionId
            });
            LogUsers();
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            string connection = Context.ConnectionId;

            var rooms = Rooms.Where(r => r.User1.ConnectionId == connection || r.User2.ConnectionId == connection || r.User1.ConnectionId == connection || r.User2.ConnectionId == connection).ToList();
            for (int i = 0; i < rooms.Count; i++)
            {
                Clients.Clients(new List<string> { rooms[i].User1.ConnectionId, rooms[i].User2.ConnectionId }).roomClosed();
                Rooms.Remove(rooms[i]);
            }

            Users.RemoveAll(u => u.ConnectionId == connection);
            LogUsers();
            return base.OnDisconnected(stopCalled);
        }

        private async void LogUsers()
        {
            await Clients.All.broadcastUsers(Users.Select(u => u.UserName));
        }

        public async Task NewGame(string u1, string u2)
        {
            string group = Guid.NewGuid().ToString();
            var user1 = Users.FirstOrDefault(u => u.UserName == u1);
            var user2 = Users.FirstOrDefault(u => u.UserName == u2);

            var rooms = Rooms.Where(r => r.User1.UserName == u1 || r.User2.UserName == u1 || r.User1.UserName == u1 || r.User2.UserName == u2).ToList();
            for (int i = 0; i < rooms.Count; i++)
            {
                await Clients.Clients(new List<string> { rooms[i].User1.ConnectionId, rooms[i].User2.ConnectionId }).roomClosed();
                Rooms.Remove(rooms[i]);
            }

            var room = new Room
            {
                User1 = user1,
                User2 = user2
            };
            Rooms.Add(room);

            await Clients.Clients(new List<string> { room.User1.ConnectionId, room.User2.ConnectionId }).newGameStarted(u1, u2);
        }

        public async Task NewMove(int row1, int col1, int row2, int col2)
        {
            var room = Rooms.FirstOrDefault(r => r.User1.ConnectionId == Context.ConnectionId || r.User2.ConnectionId == Context.ConnectionId);
            if (room == null) return;

            await Clients.Clients(new List<string>
            {
                room.User1.ConnectionId,
                room.User2.ConnectionId
            }).newMove(Users.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId).UserName, row1, col1, row2, col2);
        }
    }
}