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
        private string _username;
        private string _clientName;
        private GameManager gameManager;
        private TcpManager tcpManager;
        private InputManager inputManager;
        private SoundManager soundManager;
        private double _lastTimeSend = 0;
        private TimeSpan _lastRenderingTime = TimeSpan.Zero;
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
            soundManager = new SoundManager();

            gameManager.OnObjectCreated += GameManager_OnObjectCreated;
            gameManager.OnGameObjectDestroy += GameManager_OnGameObjectDestroy;
            gameManager.OnWorldSended += GameManager_OnSended;
            gameManager.OnTapToSend += GameManager_OnKeySend;
            gameManager.OnTankDestroy += GameManager_OnTankDestroy;
            gameManager.OnTankCreated += GameManager_OnTankCreated;

            tcpManager.OnPlayerConnected += TcpManager_OnPlayerConnected;
            tcpManager.OnGameStart += TcpManager_OnGameStart;
            tcpManager.OnClientReceived += TcpManager_OnClientReceived;
            tcpManager.OnHostReceived += TcpManager_OnHostReceived;
            tcpManager.OnTankDestroyed += TcpManager_OnTankDestroyed;

            NameInput.Text = "Аноним" + new Random().Next();
        }

        private void GameManager_OnTankCreated(TankView obj)
        {
            GameCanvas.Children.Add(obj.Grid);
            Canvas.SetLeft(obj.Grid,obj.Tank.Position.X);
            Canvas.SetTop(obj.Grid, obj.Tank.Position.Y);
            UpdateLayout();
            GameCanvas.UpdateLayout();
        }

        private void GameManager_OnTankDestroy(SendedDto obj)
        {
            string name = obj.gameObjects[0].AttachType == AttachType.Client ?  _username : _clientName;
            obj.gameObjects[0].UserName = name;
            tcpManager.SendTankDestroyMessage(obj);
            EndGame(name);
        }

        private void EndGame(string username)
        {
            UnsubscribeGameLoop();
            GameCanvas.Children.Clear();
            soundManager.DoAction(SoundAction.Destroy);
            Menu.Visibility = Visibility.Visible;
            DefeatAndWInGrid.Visibility = Visibility.Visible;
            WInnerName.Text = username;
            if (!gameManager.IsClient)
            {
                RestartButton.Visibility = Visibility.Visible;
            }
        }

        private void TcpManager_OnTankDestroyed(AttachType obj,string name)
        {
            EndGame(name);
            tcpManager.GameEnded = true;
            Dispatcher.BeginInvoke(() =>
            {
                gameManager.Destroy(obj);
            });
        }

        private void TcpManager_OnHostReceived(string[] obj)
        {
            Dispatcher.BeginInvoke(() => gameManager.SetRemoted(obj));
        }

        private void GameManager_OnKeySend(InputManager obj,double time,double deltaTime)
        {
            if(time - _lastTimeSend > deltaTime)
            {
                var keys = obj.Pressed.Select(k => k.ToString()).ToArray();
                tcpManager.SendRemoteKey(keys);
                _lastTimeSend = time;
            }
        }

        private void TcpManager_OnClientReceived(SendedDto obj)
        {
            Dispatcher.BeginInvoke(() => gameManager.ApplyWorld(obj));
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

        private void TcpManager_OnPlayerConnected(string name)
        {
            _clientName = name;
            Players.Text = "2";
            Start.IsEnabled = true;
        }

        private void GameManager_OnObjectCreated(GameObject obj)
        {
            Rectangle spawnObject = obj.Object;
            GameCanvas.Children.Add(obj.Object);
            Canvas.SetLeft(spawnObject, obj.Position.X);
            Canvas.SetTop(spawnObject, obj.Position.Y);
            Canvas.SetZIndex(spawnObject, -1);
            soundManager.DoAction(SoundAction.Shoot);

            UpdateLayout();
            GameCanvas.UpdateLayout();
        }

        private void GameManager_OnGameObjectDestroy(GameObject obj)
        {
            GameCanvas.Children.Remove(obj.Object);
        }


        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            if (e is not RenderingEventArgs args)
            {
                return;
            }

            if (_lastRenderingTime == TimeSpan.Zero)
            {
                _lastRenderingTime = args.RenderingTime;
                return;
            }
            double deltaTime = (args.RenderingTime - _lastRenderingTime).TotalSeconds;
            _lastRenderingTime = args.RenderingTime;
            gameManager.Update(inputManager, deltaTime);
        }

        private void SubscribeGameLoop()
        {
            CompositionTarget.Rendering += CompositionTarget_Rendering;
            _lastRenderingTime = TimeSpan.Zero;
        }
        private void UnsubscribeGameLoop()
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            _lastRenderingTime = TimeSpan.Zero;
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
            tcpManager.GameEnded = false;
            Menu.Visibility = Visibility.Hidden;
            gameManager.PrepareNewMatch();
            gameManager.SpawnTank(new Point(300, 100));
            gameManager.SpawnEnemyTank(new Point(600, 300));
            SubscribeGameLoop();

            var obst = gameManager.CreateObstacle(new Point(400, 200));
            GameCanvas.Children.Add(obst);
            Canvas.SetTop(obst, 200);
            Canvas.SetLeft(obst, 400);

            var obst2 = gameManager.CreateObstacle(new Point(1000, 700));
            GameCanvas.Children.Add(obst2);
            Canvas.SetTop(obst2, 700);
            Canvas.SetLeft(obst2, 1000);
            _lastTimeSend = 0;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            tcpManager.StartGameAsync();
            Waiting_Server.Visibility = Visibility.Hidden;
            LoadGame();
        }

        private async void ButtonOfInput_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Input.Text) && tcpManager.TryIP(Input.Text))
            {
                MenuOfInput.Visibility = Visibility.Hidden;
                Waiting_Client.Visibility = Visibility.Visible;
                tcpManager.Connect(true);
                tcpManager.SendName(_username);
            }
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            tcpManager.StartGameAsync();
            LoadGame();
        }

        private void NameEnter_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(NameInput.Text))
            {
                _username = NameInput.Text;
                Name.Visibility = Visibility.Hidden;
                Buttons.Visibility = Visibility.Visible;
            }
        }
    }
}