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
        private InputManager inputManager;
        public MainWindow()
        {
            InitializeComponent();
            WindowState = WindowState.Maximized;
            Width = SystemParameters.WorkArea.Width;
            Height = SystemParameters.WorkArea.Height;
            gameManager = new GameManager();
            inputManager = new InputManager();

            gameManager.OnTankCreated += GameManager_OnTankCreated;
            gameManager.OnShooting += GameManager_OnShooting;
            gameManager.OnBulletDestroy += GameManager_OnBulletDestroy;

            Loaded += MainWindow_Loaded;

        }

        private void GameManager_OnBulletDestroy(Bullet obj)
        {
            GameCanvas.Children.Remove(obj.Object);
        }

        private void GameManager_OnShooting(Bullet obj)
        {
            Rectangle bullet = obj.Object;
            GameCanvas.Children.Add(obj.Object);
            Canvas.SetLeft(bullet, obj.Position.X);
            Canvas.SetTop(bullet, obj.Position.Y);
            Canvas.SetZIndex(bullet, -1);
        }

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            gameManager.Update(inputManager);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            gameManager.SpawnTank(new Point(300, 100));
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

        private void GameManager_OnTankCreated(Tank obj)
        {
            Rectangle tank = obj.Object;
            GameCanvas.Children.Add(obj.Object);
            Canvas.SetLeft(tank, obj.Position.X);
            Canvas.SetTop(tank, obj.Position.Y);
        }

        private void GameWindow_KeyDown(object sender, KeyEventArgs e)
        {
            inputManager.OnKeyDown(e.Key);
        }

        private void GameWindow_KeyUp(object sender, KeyEventArgs e)
        {
            inputManager.OnKeyUp(e.Key);
        }
    }
}