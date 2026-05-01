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
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace TankTCP
{
    public class GameManager
    {
        private const double _deltaTime = 1.0 / 60.0;
        private double _gameTime;
        private Tank _tank;
        private List<Bullet> _bullets;
        private List<Obstacle> _obstacles;

        public event Action<Tank> OnTankCreated;
        public event Action<Bullet> OnShooting;
        public event Action<Bullet> OnBulletDestroy;

        public GameManager()
        {
            _bullets = new List<Bullet>();
            _obstacles = new List<Obstacle>();
        }
        public void SpawnTank(Point pos)
        {
            _tank = new Tank(pos,60,50);
            OnTankCreated?.Invoke(_tank);
        }

        public Rectangle CreateObstacle(Point pos)
        {
            var obst = new Obstacle(pos, 100, 100);
            _obstacles.Add(obst);

            return obst.Object;
        }

        public void Update(InputManager input)
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

                OnShooting?.Invoke(bullet);
            }

            CheckCollisions(input);

            _tank.Update();

            foreach(var bullet in _bullets)
            {
                bullet.Update();
            }
        }

        private void CheckCollisions(InputManager input)
        {
            foreach(var obstacle in _obstacles)
            {
                var next_points = _tank.GetCorners(_tank.Angle,_tank.NextPosition).ToList();
                var next_end_points = _tank.GetEndPoints(_tank.Angle, _tank.NextPosition).ToList();
                next_points.AddRange(next_end_points);

                var points = _tank.GetCorners(_tank.PrevAngle,_tank.Position).ToList();
                var end_points = _tank.GetEndPoints(_tank.PrevAngle, _tank.Position).ToList();
                points.AddRange(end_points);

                for(int i = 0;i < points.Count; i++)
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
                    foreach(var pos in bullet.GetCorners())
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
    }
}
