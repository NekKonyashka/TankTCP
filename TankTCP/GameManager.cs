using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private const double _deltaTime = 1.0 / 60.0;
        public static Random random = new Random();
        private double _gameTime;
        private Tank _tank;
        private Tank _enemyTank;
        private List<Bullet> _bullets;
        private List<Obstacle> _obstacles;
        private bool IsClient;

        public event Action<GameObject> OnObjectCreated;
        public event Action<Bullet> OnBulletDestroy;
        public event Action<SendedDto> OnWorldSended;
        public event Action<InputManager> OnTapToSend;

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
            _tank = new Tank(pos, new ImageBrush(new BitmapImage(new Uri("./res/RedTank.png", UriKind.Relative))));
            OnObjectCreated?.Invoke(_tank);
        }
        public void SpawnEnemyTank(Point pos)
        {
            _enemyTank = new Tank(pos, new ImageBrush(new BitmapImage(new Uri("./res/BlueTank.png", UriKind.Relative))));
            OnObjectCreated?.Invoke(_enemyTank);
        }
        public Rectangle CreateObstacle(Point pos)
        {
            var obst = new Obstacle(pos, 100, 100);
            _obstacles.Add(obst);

            return obst.Object;
        }

        public void Update(InputManager input)
        {
            if (!IsClient)
            {
                _gameTime += _deltaTime;

                if (input.IsPressed(Key.W))
                {
                    _tank.MoveForward();
                }
                if (input.IsPressed(Key.S))
                {
                    _tank.MoveBackward();
                }
                if (input.IsPressed(Key.A))
                {
                    if (input.IsPressed(Key.S))
                    {
                        _tank.RotateRight();
                    }
                    else
                    {
                        _tank.RotateLeft();
                    }
                }
                if (input.IsPressed(Key.D))
                {
                    if (input.IsPressed(Key.S))
                    {
                        _tank.RotateLeft();
                    }
                    else
                    {
                        _tank.RotateRight();
                    }
                }
                if (input.IsPressed(Key.Space) && _tank.CanShoot(_gameTime))
                {
                    var bullet = _tank.Shoot(_gameTime);
                    _bullets.Add(bullet);

                    OnObjectCreated?.Invoke(bullet);
                }

                RemoteControl();

                CheckCollisions(input);

                _tank.Update();

                foreach (var bullet in _bullets)
                {
                    bullet.Update();
                }

                SaveWorld();
            }
            else
            {
                OnTapToSend?.Invoke(input);
                _tank.Update();
            }
        }


        public void ApplyWorld(SendedDto dto)
        {
            var tank = dto.gameObjects[0];
            _tank.Apply(tank);
            var enemyTank = dto.gameObjects[1];
            _enemyTank.Apply(enemyTank);
        }


        private void SaveWorld()
        {

            GameObjectDto tank = new GameObjectDto()
            {
                Position = _tank.Position,
                Angle = _tank.Angle,
                Type = GameObjectType.Tank,
            };
            _worldState.gameObjects.Add(tank);
            GameObjectDto enemyTank = new GameObjectDto()
            {
                Position = _enemyTank.Position,
                Angle = _enemyTank.Angle,
                Type = GameObjectType.Tank,
            };
            _worldState.gameObjects.Add(enemyTank);
            foreach ( var bullet in _bullets)
            {
                var bulletDto = new GameObjectDto()
                {
                    Position = bullet.Position,
                    Angle = bullet.Angle,
                    Type = GameObjectType.Bullet,
                };
                _worldState.gameObjects.Add(bulletDto);
            }
            OnWorldSended?.Invoke(_worldState);
            _worldState.gameObjects.Clear();
        }

        private void CheckCollisions(InputManager input)
        {
            foreach(var obstacle in _obstacles)
            {
                var next_points = _tank.GetCorners(_tank.Angle, _tank.NextPosition).ToList();
                var next_end_points = _tank.GetEndPoints(_tank.Angle, _tank.NextPosition);
                next_points.AddRange(next_end_points);

                var points = _tank.GetCorners(_tank.PrevAngle, _tank.Position).ToList();
                var end_points = _tank.GetEndPoints(_tank.PrevAngle, _tank.Position).ToList();
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

                        if (input.IsPressed(Key.A) || input.IsPressed(Key.D))
                        {
                            _tank.MoveBackX(dif_x);
                        }
                        else
                        {
                            _tank.ReturnX();
                        }

                    }
                    if (IsCollider(
                        new Point(next_points[i].X, points[i].Y - dif_y),
                        point_min: obstacle.Position,
                        point_max: new Point(obstacle.Position.X + obstacle.Width,
                                            obstacle.Position.Y + obstacle.Height)
                        ))
                    {

                        if (input.IsPressed(Key.A) || input.IsPressed(Key.D))
                        {
                            _tank.MoveBackY(dif_y);
                        }
                        else
                        {
                            _tank.ReturnY();
                        }
                    }
                }
            }
            var bullets = _bullets.ToList();
            foreach(var obstacle in _obstacles)
            {
                foreach(var bullet in bullets)
                {
                    foreach(var pos in bullet.GetCorners(bullet.Angle,bullet.Position))
                    {
                        if (IsCollider(bullet.Position,
                            point_min: obstacle.Position,
                            point_max: new Point(obstacle.Position.X + obstacle.Width,
                                                obstacle.Position.Y + obstacle.Height)
                            ))
                        {
                            _bullets.Remove(bullet);
                            OnBulletDestroy?.Invoke(bullet);
                        }
                    }
                }
            }
            CheckTankOffScreen(input);
            CheckBulletsOffScreen(bullets);
        }

        private void CheckTankOffScreen(InputManager input)
        {
            var next_points = _tank.GetCorners(_tank.Angle, _tank.NextPosition).ToList();
            var points = _tank.GetCorners(_tank.PrevAngle, _tank.Position).ToList();

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

                    if (input.IsPressed(Key.A) || input.IsPressed(Key.D))
                    {
                        _tank.MoveBackX(dif_x);
                    }
                    else
                    {
                        _tank.ReturnX();
                    }
                }
                if (!IsCollider(
                    new Point(next_points[i].X, points[i].Y - dif_y),
                    point_min: new Point(0, 0),
                    point_max: new Point(SystemParameters.WorkArea.Width,
                                        SystemParameters.WorkArea.Height)
                    ))
                {

                    if (input.IsPressed(Key.A) || input.IsPressed(Key.D))
                    {
                        _tank.MoveBackY(dif_y);
                    }
                    else
                    {
                        _tank.ReturnY();
                    }

                }
            }
        }

        private void CheckBulletsOffScreen(List<Bullet> bullets)
        {
            foreach(var bullet in bullets)
            {
                if(!IsCollider(bullet.Position,
                    point_min:new Point(0,0),
                    point_max: new Point(SystemParameters.WorkArea.Width,
                                        SystemParameters.WorkArea.Height)))
                {
                    _bullets.Remove(bullet);
                    OnBulletDestroy?.Invoke(bullet);
                }
                    

            }
        }

        private bool IsCollider(Point point1,Point point_min,Point point_max)
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
