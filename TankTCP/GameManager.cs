using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
                foreach(var pos in _tank.GetCorners())
                {
                    if(pos.X >= obstacle.Position.X && pos.X <= obstacle.Position.X + obstacle.Width &&
                        pos.Y >= obstacle.Position.Y && pos.Y <= obstacle.Position.Y + obstacle.Height)
                    {
                        if (input.IsPressed(Key.W))
                        {
                            _tank.MoveBackward();
                        }
                        if(input.IsPressed(Key.S))
                        {
                            _tank.MoveForward();
                        }
                        if (input.IsPressed(Key.A))
                        {
                            _tank.RotateRight();
                        }
                        if (input.IsPressed(Key.D))
                        {
                            _tank.RotateLeft();
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
                        if (pos.X >= obstacle.Position.X && pos.X <= obstacle.Position.X + obstacle.Width &&
                            pos.Y >= obstacle.Position.Y && pos.Y <= obstacle.Position.Y + obstacle.Height)
                        {
                            _bullets.Remove(bullet);
                            OnBulletDestroy?.Invoke(bullet);
                        }
                    }
                }
            }
        }
    }
}
