using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TankTCP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GameManager gameManager;
        private TcpManager tcpManager;
        private InputManager inputManager;
        private double _lastTimeSend = 0;
        public MainWindow()
        {
            InitializeComponent();
            //WindowState = WindowState.Maximized;
            //Width = SystemParameters.WorkArea.Width;
            //Height = SystemParameters.WorkArea.Height;
            //ResizeMode = ResizeMode.NoResize;

            gameManager = new GameManager();
            inputManager = new InputManager();
            tcpManager = new TcpManager();

            gameManager.OnObjectCreated += GameManager_OnObjectCreated;
            gameManager.OnBulletDestroy += GameManager_OnBulletDestroy;
            gameManager.OnWorldSended += GameManager_OnSended;
            gameManager.OnTapToSend += GameManager_OnKeySend;

            tcpManager.OnPlayerConnected += TcpManager_OnPlayerConnected;
            tcpManager.OnGameStart += TcpManager_OnGameStart;
            tcpManager.OnClientReceived += TcpManager_OnClientReceived;
            tcpManager.OnHostReceived += TcpManager_OnHostReceived;
        }

        private void TcpManager_OnHostReceived(string[] obj)
        {
            gameManager.SetRemoted(obj);
        }

        private void GameManager_OnKeySend(InputManager obj,double time,double deltaTime)
        {
            if(time - _lastTimeSend >= deltaTime)
            {
                var keys = obj.Pressed.Select(k => k.ToString()).ToArray();
                tcpManager.SendRemoteKey(keys);
                _lastTimeSend = time;
            }
        }

        private void TcpManager_OnClientReceived(SendedDto obj)
        {
            gameManager.ApplyWorld(obj);
        }

        private void GameManager_OnSended(SendedDto obj)
        {
            tcpManager.SendWorldStateAsync(obj);
        }

        private void TcpManager_OnGameStart()
        {
            Waiting_Client.Visibility = Visibility.Hidden;
            LoadGame();
        }

        private void TcpManager_OnPlayerConnected()
        {
            Players.Text = "2";
            Start.IsEnabled = true;
        }

        private void GameManager_OnObjectCreated(GameObject obj)
        {
            Rectangle spawnObject = obj.Object;
            GameCanvas.Children.Add(obj.Object);
            Canvas.SetLeft(spawnObject, obj.Position.X);
            Canvas.SetTop(spawnObject, obj.Position.Y);
            if(obj is Tank)
            {
                Canvas.SetZIndex(spawnObject, 1);
            }
            else
            {
                Canvas.SetZIndex(spawnObject, -1);
            }
            UpdateLayout();
            GameCanvas.UpdateLayout();
        }

        private void GameManager_OnBulletDestroy(Bullet obj)
        {
            GameCanvas.Children.Remove(obj.Object);
        }


        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            gameManager.Update(inputManager);
        }


        private void GameWindow_KeyDown(object sender, KeyEventArgs e)
        {
            inputManager.OnKeyDown(e.Key);
        }

        private void GameWindow_KeyUp(object sender, KeyEventArgs e)
        {
            inputManager.OnKeyUp(e.Key);
        }

        private void ServerButton_Click(object sender, RoutedEventArgs e)
        {
            tcpManager.Connect(false);
            gameManager.SetState(false);
            Waiting_Server.Visibility = Visibility.Visible;
            Buttons.Visibility = Visibility.Hidden;
        }
        private void ClientButton_Click(object sender, RoutedEventArgs e)
        {
            gameManager.SetState(true);
            MenuOfInput.Visibility = Visibility.Visible;
            Buttons.Visibility = Visibility.Hidden;
        }

        private void LoadGame()
        {
            Menu.Visibility = Visibility.Hidden;
            gameManager.SpawnTank(new Point(300, 100));
            gameManager.SpawnEnemyTank(new Point(600, 300));
            CompositionTarget.Rendering += CompositionTarget_Rendering;

            var obst = gameManager.CreateObstacle(new Point(400, 200));
            GameCanvas.Children.Add(obst);
            Canvas.SetTop(obst, 200);
            Canvas.SetLeft(obst, 400);

            var obst2 = gameManager.CreateObstacle(new Point(1000, 700));
            GameCanvas.Children.Add(obst2);
            Canvas.SetTop(obst2, 700);
            Canvas.SetLeft(obst2, 1000);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            tcpManager.StartGameAsync();
            Waiting_Server.Visibility = Visibility.Hidden;
            LoadGame();
        }

        private void ButtonOfInput_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Input.Text) && tcpManager.TryIP(Input.Text))
            {
                MenuOfInput.Visibility = Visibility.Hidden;
                Waiting_Client.Visibility = Visibility.Visible;
                tcpManager.Connect(true);
            }
        }
    }
}