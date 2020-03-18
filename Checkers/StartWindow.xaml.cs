using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Checkers
{
    public partial class StartWindow : Window
    {
        Client client;

        public StartWindow()
        {
            InitializeComponent();
        }

        private async void UserList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (client.Connected && UserList.SelectedItem.ToString() != UserName.Text)
                await client.StartGame(UserList.SelectedItem.ToString());
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            UserName.IsEnabled = false;
            client = new Client(UserName.Text);
            client.SetUIThread(Thread.CurrentThread);
            client.UpdateUserList += UpdateUserList;
            client.NewGame += NewGame;
        }

        public void NewGame(string user1, string user2)
        {
            MainWindow window = null;
            if (user1 != UserName.Text)
            {
                window = new MainWindow(user1, user2, client, true);
            }
            else
            {
                window = new MainWindow(user1, user2, client, false);
            }
            window.Show();
            Close();
        }

        public void UpdateUserList(List<string> userList)
        {
            UserList.Items.Clear();
            foreach (var user in userList)
                UserList.Items.Add(user);
        }

        private void Offline_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow(string.Empty, string.Empty, null, true);
            window.Show();
            Close();
        }
    }
}