
using Microsoft.Xna.Framework;
using YellowShell.Level;

namespace YellowShell.Ammo
{
    class Ammo_Missile : Ammo
    {

        private static readonly float SPEED = 7.5f;
        private static readonly float DAMAGE = 15.0f;


        public Ammo_Missile(LevelManager level, Vector2 position, Direction direction) : base(AmmoType.Missile, level, position, direction, SPEED, DAMAGE)
        {
            LoadContent();
        }
    }
}
