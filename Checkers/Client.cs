using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Checkers
{
    public class Client
    {
        private HubConnection _connection;
        private IHubProxy _proxy;
        private string user;
        private Thread UIthread;

        public delegate void OnUpdateUserList(List<string> userList);
        public OnUpdateUserList UpdateUserList;

        public delegate void OnNewGame(string user1, string user2);
        public OnNewGame NewGame;

        public delegate void OnReceiveMove(int row1, int col1, int row2, int col2);
        public OnReceiveMove ReceiveMove;

        public Client(string user)
        {
            this.user = user;
            _connection = new HubConnection("http://localhost:5323/");
            _proxy = _connection.CreateHubProxy("CheckersHub");
            SetupProxy();
            _connection.Headers.Add("UserName", user);
            _connection.Start();
        }

        public bool Connected { get { return _connection.State == ConnectionState.Connected; } }

        public async Task StartGame(string opponent)
        {
            await _proxy.Invoke("NewGame", user, opponent);
        }

        public async Task SendMove(int row1, int col1, int row2, int col2)
        {
            await _proxy.Invoke("NewMove", row1, col1, row2, col2);
        }

        private void SetupProxy()
        {
            _proxy.On<List<string>>("broadcastUsers", (users) =>
            {
                Dispatcher.FromThread(UIthread).BeginInvoke((Action)(() =>
                {
                    UpdateUserList(users);
                }));
            });
            _proxy.On<string, string>("newGameStarted", (user1, user2) =>
            {
                Dispatcher.FromThread(UIthread).BeginInvoke((Action)(() =>
                {
                    NewGame(user1, user2);
                }));
            });
            _proxy.On<string, int, int, int, int>("newMove", (sender, row1, col1, row2, col2) =>
            {
                Dispatcher.FromThread(UIthread).BeginInvoke((Action)(() =>
                {
                    if (sender != user)
                        ReceiveMove(row1, col1, row2, col2);
                }));
            });
            _proxy.On("roomClosed", () =>
            {
                MessageBox.Show("Opponent left the game");
                Dispatcher.FromThread(UIthread).BeginInvoke((Action)(() =>
                {
                    Application.Current.Shutdown();
                }));
            });
        }

        public void SetUIThread(Thread thread)
        {
            UIthread = thread;
        }

        public void CloseConnection()
        {
            _connection.Stop();
        }
    }
}