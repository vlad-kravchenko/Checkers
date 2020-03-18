using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Checkers
{
    public partial class MainWindow : Window
    {
        Game game = new Game();
        Client client;
        string username = string.Empty;
        string opponent = string.Empty;
        bool red = true;

        int row1, col1;

        public MainWindow(string user1, string user2, Client client, bool red)
        {
            InitializeComponent();
            UpdateGrid();

            this.client = client;
            this.red = red;
            username = red ? user2 : user1;
            opponent = red ? user1 : user2;

            if (client == null)
            {
                Status.Text = "Offline game";
            }
            else
            {
                game.ChangeOnFront += ChangeTurn;
                client.ReceiveMove += ReceiveMove;
                Status.Text = $"{user1} started new game with {user2}";
                game.Turn = 'B';
                ChangeTurn();
                Title = "Checkers online. Username: " + username + ", team: " + (red ? "red" : "black");
            }
        }

        public void ReceiveMove(int row1, int col1, int row2, int col2)
        {
            game.PickCell(row1, col1);
            game.PlaceCell(row2, col2);
            ChangeTurn();
            UpdateGrid();
            if (game.Victory != null)
            {
                MessageBox.Show($"{game.Victory} team vins!");
            }
        }

        private void UpdateGrid()
        {
            MainGrid.Children.Clear();
            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < 8; i++)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition());
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Grid grid = new Grid();
                    grid.MouseDown += Grid_MouseDown;

                    if (game.Map[row, col] == CellType._NONE) grid.Background = Brushes.White;
                    else if (game.Map[row, col] == CellType._AVAILABLE) grid.Background = Brushes.LightGreen;
                    else grid.Background = Brushes.Gray;

                    MainGrid.Children.Add(grid);
                    Grid.SetRow(grid, row);
                    Grid.SetColumn(grid, col);

                    if (game.Map[row, col] == CellType.BLACK_ || game.Map[row, col] == CellType.BLACK_K)
                    {
                        Ellipse ellipse = new Ellipse();
                        ellipse.Fill = Brushes.Black;
                        if (game.Map[row, col] == CellType.BLACK_K)
                            ellipse.Margin = new Thickness(20);
                        grid.Children.Add(ellipse);
                        Grid.SetRow(ellipse, row);
                        Grid.SetColumn(ellipse, col);
                    }
                    else if (game.Map[row, col] == CellType.RED_ || game.Map[row, col] == CellType.RED_K)
                    {
                        Ellipse ellipse = new Ellipse();
                        ellipse.Fill = Brushes.Red;
                        if (game.Map[row, col] == CellType.RED_K)
                            ellipse.Margin = new Thickness(20);
                        grid.Children.Add(ellipse);
                        Grid.SetRow(ellipse, row);
                        Grid.SetColumn(ellipse, col);
                    }
                    else continue;
                }
            }
        }

        private async void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int row = Grid.GetRow(sender as Grid);
            int col = Grid.GetColumn(sender as Grid);
            if (client == null)
            {
                if (game.PickCell(row, col))
                    UpdateGrid();
                else if (game.PlaceCell(row, col))
                    UpdateGrid();
            }
            else
            {
                if (game.PickCell(row, col))
                {
                    if (game.PickedCellType.ToString()[0] == 'B' && red)
                        game.ResetPick();
                    else if (game.PickedCellType.ToString()[0] == 'R' && !red)
                        game.ResetPick();
                    else
                    {
                        if (game.PickedCellType.ToString()[0] == game.Turn)
                        {
                            row1 = row;
                            col1 = col;
                            UpdateGrid();
                        }
                    }
                }
                else if (game.PlaceCell(row, col))
                {
                    UpdateGrid();
                    ChangeTurn();
                    await client.SendMove(row1, col1, row, col);
                }
            }

            if (game.Victory != null)
            {
                MessageBox.Show($"{game.Victory} team vins!");
            }
        }

        private void ChangeTurn()
        {
            game.ChangeTurn();
            if (game.Turn == 'R' && red)
            {
                Turn.Text = "Next move: Red (yours)";
            }
            else if (game.Turn == 'R' && !red)
            {
                Turn.Text = "Next move: Red (opponents)";
            }
            else if (game.Turn == 'B' && red)
            {
                Turn.Text = "Next move: Black (opponents)";
            }
            else if (game.Turn == 'B' && !red)
            {
                Turn.Text = "Next move: Black (yours)";
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (client != null)
                client.CloseConnection();
            Application.Current.Shutdown();
        }
    }
}