using Microsoft.Xna.Framework;
using YellowShell.Level;

namespace YellowShell.Ammo
{
    class Ammo_Standard : Ammo
    {
        private static readonly float SPEED = 7.5f;
        private static readonly float DAMAGE = 10.0f;


        public Ammo_Standard(LevelManager level, Vector2 position, Direction direction) : base(AmmoType.Standard, level, position, direction, SPEED, DAMAGE)
        {
            LoadContent();
        }
    }
}
