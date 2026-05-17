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
using TankTCP.GameObjects;

namespace TankTCP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const double MAX_WIDTH = 1920;
        public const double MAX_HEIGHT = 1080;

        private double _widthCef;
        private double _heightCef;

        private string _username;
        private string _clientName;

        private GameManager gameManager;
        private TcpManager tcpManager;
        private InputManager inputManager;
        private SoundManager soundManager;
        private AnimationManager animationManager;


        private double _lastTimeSend = 0;
        private TimeSpan _lastRenderingTime = TimeSpan.Zero;
        public MainWindow()
        {
            InitializeComponent();
            //WindowState = WindowState.Maximized;
            //Width = SystemParameters.WorkArea.Width;
            //Height = SystemParameters.WorkArea.Height;
            //ResizeMode = ResizeMode.NoResize;

            _widthCef = Width / MAX_WIDTH;
            _heightCef = Height / MAX_HEIGHT;

            gameManager = new GameManager();
            inputManager = new InputManager();
            tcpManager = new TcpManager();
            soundManager = new SoundManager();
            animationManager = new AnimationManager(_widthCef, _heightCef);

            StandartObject.ObjectWidthCef = _widthCef;
            StandartObject.ObjectHeightCef = _heightCef;

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

            animationManager.OnAnimationStart += AnimationManager_OnAnimationStart;
            animationManager.OnAnimationEnd += AnimationManager_OnAnimationEnd;

            NameInput.Text = "Аноним" + new Random().Next();
        }

        private void AnimationManager_OnAnimationEnd(Image obj)
        {
            GameCanvas.Children.Remove(obj);
        }

        private void AnimationManager_OnAnimationStart(Image obj, Point pos)
        {
            GameCanvas.Children.Add(obj);
            Canvas.SetLeft(obj, pos.X);
            Canvas.SetTop(obj, pos.Y);
            Canvas.SetZIndex(obj, 5);
        }

        private void GameManager_OnTankCreated(TankView obj)
        {
            GameCanvas.Children.Add(obj.Grid);
            Canvas.SetLeft(obj.Grid, obj.Tank.Position.X);
            Canvas.SetTop(obj.Grid, obj.Tank.Position.Y);
            UpdateLayout();
            GameCanvas.UpdateLayout();
        }

        private async void GameManager_OnTankDestroy(SendedDto obj)
        {
            string name = obj.gameObjects[0].AttachType == AttachType.Client ? _username : _clientName;
            obj.UserName = name;
            tcpManager.SendTankDestroyMessage(obj);
            GameCanvas.Children.Remove(gameManager.GetTankView(obj.gameObjects[0].AttachType).Grid);
            animationManager.Start(obj.gameObjects[0].Position);
            UnsubscribeGameLoop();
            await animationManager.TaskCompletionSource.Task;
            EndGame(name);
        }

        private void EndGame(string username)
        {
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

        private async void TcpManager_OnTankDestroyed(SendedDto obj)
        {
            tcpManager.GameEnded = true;
            GameCanvas.Children.Remove(gameManager.GetTankView(obj.gameObjects[0].AttachType).Grid);
            animationManager.Start(obj.gameObjects[0].Position);
            UnsubscribeGameLoop();
            await animationManager.TaskCompletionSource.Task;
            EndGame(obj.UserName);
        }

        private void TcpManager_OnHostReceived(string[] obj)
        {
            Dispatcher.BeginInvoke(() => gameManager.SetRemoted(obj));
        }

        private void GameManager_OnKeySend(InputManager obj, double time, double deltaTime)
        {
            if (time - _lastTimeSend > deltaTime)
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
            Dispatcher.BeginInvoke(() =>
            {
                Waiting_Client.Visibility = Visibility.Hidden;
                LoadGame();
            });
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
            DefeatAndWInGrid.Visibility = Visibility.Hidden;
            gameManager.PrepareNewMatch();
            gameManager.SpawnTank(new Point(200 * _widthCef, 100 * _heightCef));
            gameManager.SpawnEnemyTank(new Point(1720 * _widthCef, 800 * _heightCef));
            SubscribeGameLoop();

            var obst = gameManager.CreateObstacle(new Point(400 * _widthCef, 200 * _heightCef));
            GameCanvas.Children.Add(obst.Object);
            Canvas.SetTop(obst.Object, obst.Position.Y);
            Canvas.SetLeft(obst.Object, obst.Position.X);
            Canvas.SetZIndex(obst.Object, -1);

            var obst2 = gameManager.CreateObstacle(new Point(1520 * _widthCef, 700 * _heightCef));
            GameCanvas.Children.Add(obst2.Object);
            Canvas.SetTop(obst2.Object, obst2.Position.Y);
            Canvas.SetLeft(obst2.Object, obst2.Position.X);
            Canvas.SetZIndex(obst2.Object, -1);

            _lastTimeSend = 0;
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            await tcpManager.StartGameAsync();
            Waiting_Server.Visibility = Visibility.Hidden;
            LoadGame();
        }

        private async void ButtonOfInput_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Input.Text) && tcpManager.TryIP(Input.Text))
            {
                MenuOfInput.Visibility = Visibility.Hidden;
                Waiting_Client.Visibility = Visibility.Visible;
                await tcpManager.Connect(true);
                tcpManager.SendName(_username);
            }
        }

        private async void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            await tcpManager.StartGameAsync();
            LoadGame();
        }

        private void NameEnter_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(NameInput.Text) && NameInput.Text.Length <= 32)
            {
                _username = NameInput.Text;
                Name.Visibility = Visibility.Hidden;
                Buttons.Visibility = Visibility.Visible;
            }
        }
    }
}