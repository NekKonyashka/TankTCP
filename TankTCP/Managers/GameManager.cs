using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TankTCP
{
    public class GameManager
    {
        private SendedDto _worldState;
        private const double MAX_DELTA_TIME = 0.1;
        private const double KEY_INTERVAL = 1.0 / 60.0;
        public static Random random = new Random();
        private double _gameTime;
        private List<Bullet> _bullets;
        private List<Obstacle> _obstacles;
        private TankView _tankView;
        private TankView _enemyTankView;
        public bool IsClient { get; private set; }
        private string[] _remotedKeys = { };

        public event Action<GameObject> OnObjectCreated;
        public event Action<GameObject> OnGameObjectDestroy;
        public event Action<SendedDto> OnWorldSended;
        public event Action<InputManager, double, double> OnTapToSend;
        public event Action<SendedDto> OnTankDestroy;
        public event Action<TankView> OnTankCreated;

        public GameManager()
        {
            _bullets = new List<Bullet>();
            _obstacles = new List<Obstacle>();
            _worldState = new SendedDto();
        }
        public void SetState(bool isClient)
        {
            IsClient = isClient;
        }
        public void SpawnTank(Point pos)
        {
            var tank = new Tank(pos, AttachType.Host,
                new ImageBrush(new BitmapImage(new Uri("./res/RedTank.png", UriKind.Relative))));
            _tankView = new TankView(tank);
            if (IsClient)
            {
                _tankView.ReloadBar.Visibility = Visibility.Hidden;
            }
            OnTankCreated?.Invoke(_tankView);
        }
        public void SpawnEnemyTank(Point pos)
        {
            var enemyTank = new Tank(pos, AttachType.Client,
                new ImageBrush(new BitmapImage(new Uri("./res/BlueTank.png", UriKind.Relative))));
            _enemyTankView = new TankView(enemyTank);
            if (!IsClient)
            {
                _enemyTankView.ReloadBar.Visibility = Visibility.Hidden;
            }
            OnTankCreated?.Invoke(_enemyTankView);
        }
        public Obstacle CreateObstacle(Point pos)
        {
            var obst = new Obstacle(pos);
            _obstacles.Add(obst);

            return obst;
        }

        public void PrepareNewMatch()
        {
            _gameTime = 0;
            _bullets.Clear();
            _obstacles.Clear();
            _remotedKeys = Array.Empty<string>();
        }

        public void Update(InputManager input, double dt)
        {
            if (dt <= 0)
            {
                return;
            }

            double result_dt = Math.Min(dt, MAX_DELTA_TIME);
            _gameTime += result_dt;

            if (!IsClient)
            {
                if (input.IsPressed(Key.W))
                {
                    _tankView.Tank.MoveForward();
                }
                if (input.IsPressed(Key.S))
                {
                    _tankView.Tank.MoveBackward();
                }
                if (input.IsPressed(Key.A))
                {
                    if (input.IsPressed(Key.S))
                    {
                        _tankView.Tank.RotateRight();
                    }
                    else
                    {
                        _tankView.Tank.RotateLeft();
                    }
                }
                if (input.IsPressed(Key.D))
                {
                    if (input.IsPressed(Key.S))
                    {
                        _tankView.Tank.RotateLeft();
                    }
                    else
                    {
                        _tankView.Tank.RotateRight();
                    }
                }
                if (input.IsPressed(Key.Space) && _tankView.Tank.CanShoot(_gameTime))
                {
                    var bullet = _tankView.Tank.Shoot(_gameTime);
                    _bullets.Add(bullet);

                    OnObjectCreated?.Invoke(bullet);
                }

                RemoteControl();

                CheckCollisions(input);

                _tankView.Update(_gameTime);
                _enemyTankView.Update(_gameTime);

                foreach (var bullet in _bullets)
                {
                    bullet.Update();
                }
                SaveWorld();
            }
            else
            {
                OnTapToSend?.Invoke(input, _gameTime, KEY_INTERVAL);
                _tankView.Update(_gameTime);
                _enemyTankView.Update(_gameTime);

                foreach (var bullet in _bullets)
                {
                    bullet.Update();
                }
            }
        }

        public void SetRemoted(string[] keys)
        {
            _remotedKeys = keys;
        }

        public void RemoteControl()
        {
            if (_remotedKeys.Contains("W"))
            {
                _enemyTankView.Tank.MoveForward();
            }
            if (_remotedKeys.Contains("S"))
            {
                _enemyTankView.Tank.MoveBackward();
            }
            if (_remotedKeys.Contains("A"))
            {
                if (_remotedKeys.Contains("S"))
                {
                    _enemyTankView.Tank.RotateRight();
                }
                else
                {
                    _enemyTankView.Tank.RotateLeft();
                }
            }
            if (_remotedKeys.Contains("D"))
            {
                if (_remotedKeys.Contains("S"))
                {
                    _enemyTankView.Tank.RotateLeft();
                }
                else
                {
                    _enemyTankView.Tank.RotateRight();
                }
            }
            if (_remotedKeys.Contains("Space") && _enemyTankView.Tank.CanShoot(_gameTime))
            {
                var bullet = _enemyTankView.Tank.Shoot(_gameTime);
                _bullets.Add(bullet);

                OnObjectCreated?.Invoke(bullet);
            }
        }


        public void ApplyWorld(SendedDto dto)
        {
            _gameTime = dto.GameTime;
            foreach (var obj in dto.gameObjects)
            {
                if (obj.AttachType == AttachType.Host && obj.Type == GameObjectType.Tank)
                {
                    _tankView.Tank.Apply(obj);
                    _tankView.SyncHealthBars();
                }
                else if (obj.AttachType == AttachType.Client && obj.Type == GameObjectType.Tank)
                {
                    _enemyTankView.Tank.Apply(obj);
                    _enemyTankView.SyncHealthBars();
                }
                else if (obj.Type == GameObjectType.Bullet)
                {
                    if (_bullets.Where(b => b.Id == obj.Id).FirstOrDefault() == null)
                    {
                        Bullet bullet = new Bullet(obj.Position, obj.AttachType, obj.Angle);
                        _bullets.Add(bullet);
                        bullet.Apply(obj);
                        OnObjectCreated?.Invoke(bullet);
                    }
                }
            }
            //Короче тут проходит по списку текущих пуль и каждую пулю передает в перебор списка состояния объектов от сервера,
            //у который тип Bullet.А далее сравнивается айди переданной пули со всеми пулями сервера и если количество
            //совпадений равно 0, то пуля на серваке уже не существует и поэтому она удаляется у клиента.

            var deletedBullets = _bullets.Where(b => dto.gameObjects.Where(
                obj => obj.Type == GameObjectType.Bullet && b.Id == obj.Id).Count() == 0).ToList();
            foreach (var bullet in deletedBullets)
            {
                _bullets.Remove(bullet);
                OnGameObjectDestroy?.Invoke(bullet);
            }
        }


        public TankView GetTankView(AttachType attachType)
        {
            return attachType == AttachType.Client ? _enemyTankView : _tankView;
        }



        private void SaveWorld()
        {
            _worldState.GameTime = _gameTime;
            GameObjectDto tank = new GameObjectDto()
            {
                Position = _tankView.Tank.Position,
                Angle = _tankView.Tank.Angle,
                AttachType = _tankView.Tank.AttachType,
                Type = GameObjectType.Tank,
                Id = -1,
                Health = _tankView.Tank.Health,
                LastTimeShooting = _tankView.Tank._lastTimeShooting
            };
            _worldState.gameObjects.Add(tank);
            GameObjectDto enemyTank = new GameObjectDto()
            {
                Position = _enemyTankView.Tank.Position,
                Angle = _enemyTankView.Tank.Angle,
                AttachType = _enemyTankView.Tank.AttachType,
                Type = GameObjectType.Tank,
                Id = -1,
                Health = _enemyTankView.Tank.Health,
                LastTimeShooting = _enemyTankView.Tank._lastTimeShooting
            };
            _worldState.gameObjects.Add(enemyTank);
            foreach (var bullet in _bullets)
            {
                var bulletDto = new GameObjectDto()
                {
                    Position = bullet.Position,
                    Angle = bullet.Angle,
                    AttachType = bullet.AttachType,
                    Type = GameObjectType.Bullet,
                    Id = bullet.Id
                };
                _worldState.gameObjects.Add(bulletDto);
            }
            OnWorldSended?.Invoke(_worldState);
            _worldState.gameObjects.Clear();
        }

        private void TankObstacleCollision(Tank tank, Obstacle obstacle, InputManager input)
        {
            var next_points = tank.GetCorners(tank.Angle, tank.NextBodyPosition).ToList();
            var next_end_points = tank.GetEndPoints(tank.Angle, tank.NextBodyPosition);
            next_points.AddRange(next_end_points);

            var points = tank.GetCorners(tank.PrevAngle, tank.BodyPosition).ToList();
            var end_points = tank.GetEndPoints(tank.PrevAngle, tank.BodyPosition).ToList();
            points.AddRange(end_points);

            for (int i = 0; i < points.Count(); i++)
            {
                var dif_x = points[i].X - next_points[i].X;
                var dif_y = points[i].Y - next_points[i].Y;

                if (IsCollider(
                    new Point(next_points[i].X - dif_x, points[i].Y),
                    point_min: obstacle.Position,
                    point_max: new Point(obstacle.Position.X + obstacle.Width,
                                        obstacle.Position.Y + obstacle.Height)
                    ))
                {
                    if (tank.AttachType == AttachType.Host)
                    {
                        if (input.IsPressed(Key.A) || input.IsPressed(Key.D))
                        {
                            tank.MoveBackX(dif_x);
                        }
                        else
                        {
                            tank.ReturnX();
                        }
                    }
                    else
                    {
                        if (_remotedKeys.Contains("A") || _remotedKeys.Contains("D"))
                        {
                            tank.MoveBackX(dif_x);
                        }
                        else
                        {
                            tank.ReturnX();
                        }
                    }
                }
                if (IsCollider(
                    new Point(next_points[i].X, points[i].Y - dif_y),
                    point_min: obstacle.Position,
                    point_max: new Point(obstacle.Position.X + obstacle.Width,
                                        obstacle.Position.Y + obstacle.Height)
                    ))
                {
                    if (tank.AttachType == AttachType.Host)
                    {
                        if (input.IsPressed(Key.A) || input.IsPressed(Key.D))
                        {
                            tank.MoveBackY(dif_y);
                        }
                        else
                        {
                            tank.ReturnY();
                        }
                    }
                    else
                    {
                        if (_remotedKeys.Contains("A") || _remotedKeys.Contains("D"))
                        {
                            tank.MoveBackY(dif_y);
                        }
                        else
                        {
                            tank.ReturnY();
                        }
                    }

                }
            }
        }

        private void TankBulletCollision(Tank tank, Bullet bullet)
        {
            foreach (var pos in bullet.GetCorners(bullet.Angle, bullet.Position))
            {
                if (IsCollider(bullet.Position,
                    point_min: tank.BodyPosition,
                    point_max: new Point(tank.BodyPosition.X + tank.Width,
                                        tank.BodyPosition.Y + tank.Height)
                    ) && bullet.AttachType != tank.AttachType)
                {
                    _bullets.Remove(bullet);
                    OnGameObjectDestroy?.Invoke(bullet);
                    tank.Hit();
                    if (tank.Health == 0)
                    {

                        OnTankDestroy?.Invoke(new SendedDto()
                        {
                            gameObjects = { new GameObjectDto() { Id = -67, AttachType = tank.AttachType,
                                Position = new Point(tank.BodyPosition.X - tank.Width / 2,tank.BodyPosition.Y - tank.Height / 2) } }
                        });
                    }
                    break;
                }
            }
        }

        private void CheckCollisions(InputManager input)
        {
            foreach (var obstacle in _obstacles)
            {
                TankObstacleCollision(_tankView.Tank, obstacle, input);
                TankObstacleCollision(_enemyTankView.Tank, obstacle, input);
            }

            var bullets = _bullets.ToList();
            foreach (var obstacle in _obstacles)
            {
                foreach (var bullet in bullets)
                {
                    foreach (var pos in bullet.GetCorners(bullet.Angle, bullet.Position))
                    {
                        if (IsCollider(bullet.Position,
                            point_min: obstacle.Position,
                            point_max: new Point(obstacle.Position.X + obstacle.Width,
                                                obstacle.Position.Y + obstacle.Height)
                            ))
                        {
                            _bullets.Remove(bullet);
                            OnGameObjectDestroy?.Invoke(bullet);
                            break;
                        }
                    }
                }
            }

            foreach (var bullet in bullets)
            {
                TankBulletCollision(_tankView.Tank, bullet);
                TankBulletCollision(_enemyTankView.Tank, bullet);
            }


            CheckTankOffScreen(_tankView.Tank, input);
            CheckTankOffScreen(_enemyTankView.Tank, input);
            CheckBulletsOffScreen(bullets);
        }

        private void CheckTankOffScreen(Tank tank, InputManager input)
        {
            var next_points = tank.GetCorners(tank.Angle, tank.NextBodyPosition).ToList();
            var points = tank.GetCorners(tank.PrevAngle, tank.BodyPosition).ToList();

            for (int i = 0; i < points.Count; i++)
            {
                var dif_x = points[i].X - next_points[i].X;
                var dif_y = points[i].Y - next_points[i].Y;

                if (!IsCollider(
                    new Point(next_points[i].X - dif_x, points[i].Y),
                    point_min: new Point(0, 0),
                    point_max: new Point(SystemParameters.WorkArea.Width,
                                        SystemParameters.WorkArea.Height)
                    ))
                {
                    if (tank.AttachType == AttachType.Host)
                    {
                        if (input.IsPressed(Key.A) || input.IsPressed(Key.D))
                        {
                            tank.MoveBackX(dif_x);
                        }
                        else
                        {
                            tank.ReturnX();
                        }
                    }
                    else
                    {
                        if (_remotedKeys.Contains("A") || _remotedKeys.Contains("D"))
                        {
                            tank.MoveBackX(dif_x);
                        }
                        else
                        {
                            tank.ReturnX();
                        }
                    }
                }
                if (!IsCollider(
                    new Point(next_points[i].X, points[i].Y - dif_y),
                    point_min: new Point(0, 0),
                    point_max: new Point(SystemParameters.WorkArea.Width,
                                        SystemParameters.WorkArea.Height)
                    ))
                {

                    if (tank.AttachType == AttachType.Host)
                    {
                        if (input.IsPressed(Key.A) || input.IsPressed(Key.D))
                        {
                            tank.MoveBackY(dif_y);
                        }
                        else
                        {
                            tank.ReturnY();
                        }
                    }
                    else
                    {
                        if (_remotedKeys.Contains("A") || _remotedKeys.Contains("D"))
                        {
                            tank.MoveBackY(dif_y);
                        }
                        else
                        {
                            tank.ReturnY();
                        }
                    }

                }
            }
        }

        private void CheckBulletsOffScreen(List<Bullet> bullets)
        {
            foreach (var bullet in bullets)
            {
                if (!IsCollider(bullet.Position,
                    point_min: new Point(0, 0),
                    point_max: new Point(SystemParameters.WorkArea.Width,
                                        SystemParameters.WorkArea.Height)))
                {
                    _bullets.Remove(bullet);
                    OnGameObjectDestroy?.Invoke(bullet);
                }


            }
        }

        private bool IsCollider(Point point1, Point point_min, Point point_max)
        {
            return point1.X >= point_min.X && point1.X <= point_max.X &&
                   point1.Y >= point_min.Y && point1.Y <= point_max.Y;
        }

        //public bool IsCollision(CollisionDto obj1,CollisionDto obj2)
        //{
        //    var (angle1, center1, half_w1, half_h1) = obj1;
        //    var (angle2, center2, half_w2, half_h2) = obj2;

        //    Point u1 = new Point(Math.Cos(angle1 * 180 / Math.PI),Math.Sin(angle1 * 180 / Math.PI));
        //    Point v1 = new Point(-Math.Sin(angle1 * 180 / Math.PI), Math.Cos(angle1 * 180 / Math.PI));
        //    Point u2 = new Point(Math.Cos(angle2 * 180 / Math.PI), Math.Sin(angle2 * 180 / Math.PI));
        //    Point v2 = new Point(-Math.Sin(angle2 * 180 / Math.PI), Math.Cos(angle2 * 180 / Math.PI));

        //    var axis = new List<Point>() { u1, v1, u2, v2 };

        //    for(int i = 0;i < axis.Count; i++)
        //    {
        //        var d = axis[i];

        //        double plane_c1 = d.X * center1.X + d.Y * center1.Y;
        //        double radius1 = half_w1 * Math.Abs(d.X * u1.X + d.Y * u1.Y) + 
        //                         half_h1 * Math.Abs(d.X * v1.X + d.Y * v1.Y);
        //        var plane1 = new Point(plane_c1 - radius1, plane_c1 + radius1);

        //        double plane_c2 = d.X * center2.X + d.Y * center2.Y;
        //        double radius2 = half_w2 * Math.Abs(d.X * u2.X + d.Y * u2.Y) +
        //                         half_h2 * Math.Abs(d.X * v2.X + d.Y * v2.Y);
        //        var plane2 = new Point(plane_c2 - radius2, plane_c2 + radius2);

        //        if(plane1.X > plane2.Y || plane1.Y < plane2.X)
        //        {
        //            return false;
        //        }
        //    }

        //    return true;
        //}
    }
}
