using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using YellowShell.Display;

namespace YellowShell.Level
{
    enum DoorType
    {
        Standard = 1,

        Missile = 2,

    }

    class Door
    {
        public Vector2 Position { get; set; }
        public Vector2 BottomTile { get; set; }
        public bool IsOpen { get; set; }

        public DoorType Type { get{ return m_doorLevel; } }

        private LevelManager m_level;

        private Animation m_doorOppenning;
        private Animation m_doorClosed;

        private AnimationManager m_sprite;

        private DoorType m_doorLevel;

        /// <summary>
        /// Constructs a new tile.
        /// </summary>
        public Door(Vector2 position, Vector2 bottomTile, LevelManager level, DoorType doorLevel)
        {
            Position = new Vector2(position.X+32, position.Y+32); // -32 to have 2 tile heigth
            BottomTile = bottomTile;
            IsOpen = false;

            m_level = level;
            m_doorLevel = doorLevel;

            LoadContent();
        }

        /// <summary>
        /// Loads the player sprite sheet and sounds.
        /// </summary>
        public void LoadContent()
        {
            // Load animated textures.
            m_doorClosed = new Animation(m_level.Content.Load<Texture2D>("Sprites/Door/Door_" + (int)m_doorLevel), 0.1f, false);
            m_doorOppenning = new Animation(m_level.Content.Load<Texture2D>("Sprites/Door/Door_" + (int)m_doorLevel + "_Oppening"), 0.1f, false);

        }

        public void Update(GameTime gameTime)
        {
            if(IsOpen)
            {
                m_sprite.PlayAnimation(m_doorOppenning);
            }
            else
            {
                m_sprite.PlayAnimation(m_doorClosed);
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {// Draw that sprite.
            m_sprite.Draw(gameTime, spriteBatch, Position, SpriteEffects.None);
        }
    }
}
