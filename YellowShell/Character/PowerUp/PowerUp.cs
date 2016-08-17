

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using YellowShell.Display;
using YellowShell.Level;

namespace YellowShell.Character.PowerUp
{
    enum PowerUpType
    {
        Missile = 0,

    }

    class PowerUp
    {
        public Vector2 Position { get; set; }

        public PowerUpType Type { get { return m_powerUpType; } }

        private LevelManager m_level;

        private Animation m_sprite;
        private AnimationManager m_animationManager;


        private PowerUpType m_powerUpType;

        public PowerUp(Vector2 position, LevelManager level, PowerUpType type)
        {
            Position = new Vector2(position.X , position.Y);

            m_level = level;
            m_powerUpType = type;

            LoadContent();
        }

        /// <summary>
        /// Loads the player sprite sheet and sounds.
        /// </summary>
        public void LoadContent()
        {
            // Load animated textures.
            m_sprite = new Animation(m_level.Content.Load<Texture2D>("Sprites/PowerUp/" + m_powerUpType + "_PU"), 0.1f, true);
        }

        public void Update(GameTime gameTime)
        {
            m_animationManager.PlayAnimation(m_sprite);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {// Draw that sprite.
            Rectangle rect = new Rectangle((int)Position.X * Tile.WIDTH, (int)Position.Y * Tile.HEIGHT, Tile.WIDTH, Tile.HEIGHT);

            m_animationManager.Draw(gameTime, spriteBatch, rect.GetBottomCenter(), SpriteEffects.None);
        }
    }
}
