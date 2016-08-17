using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using YellowShell.Character;
using YellowShell.Display;

namespace YellowShell.Ammo
{
    class FiringManager
    {
        private static readonly TimeSpan SHOOT_INTERVAL = TimeSpan.FromMilliseconds(333);
        public List<Ammo> Shots = new List<Ammo>();
        private TimeSpan? m_lastShot;
        

        public FiringManager()
        {

        }

        public void FireShot(GameTime gametime, Ammo shot)
        {

            if (m_lastShot == null || (gametime.TotalGameTime - (TimeSpan)m_lastShot) > SHOOT_INTERVAL)
            {
                m_lastShot = gametime.TotalGameTime;
                Shots.Add(shot);
            }


        }

        public void Update(GameTime gameTime)
        {
            for (int x = Shots.Count - 1; x >= 0; x--)
            {
                Shots[x].Update(gameTime);
                if (Shots[x].NeedToDelete)
                {
                    Shots.RemoveAt(x);
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (Ammo shot in Shots)
            {
                shot.Draw(gameTime, spriteBatch);
            }
        }
    }
}
