using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using YellowShell.Level;

namespace YellowShell.Ammo
{
    enum AmmoType
    {
        Standard = 1,
        Missile = 2,
    }

    public enum Direction
    {
        Left = 0,
        Up = 1,
        Right = 2,
        Down = 3,
    }

    abstract class Ammo
    {
        private LevelManager m_level;

        protected AmmoType m_type;
        protected bool m_needDelete;
        protected Direction m_direction;
        protected float m_speed;
        protected float m_damage;
        protected Rectangle m_localBounds;
        protected Texture2D m_texture;
        protected Vector2 m_position;

        protected SpriteEffects flip = SpriteEffects.None;

        public Vector2 Position { get; set; }

        public SpriteEffects Flip { get { return flip; } }

        public Texture2D Texture { get { return m_texture; } }

        public bool NeedToDelete { get { return m_needDelete; } }

        protected Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(m_position.X - m_texture.Bounds.X) + m_localBounds.X;
                int top = (int)Math.Round(m_position.Y - m_texture.Bounds.Y) + m_localBounds.Y;

                return new Rectangle(left, top, m_localBounds.Width, m_localBounds.Height);
            }
        }



        public Ammo(AmmoType type, LevelManager level, Vector2 position, Direction direction, float speed, float damage)
        {
            m_type = type;
            m_level = level;
            m_position = position;
            m_direction = direction;
            m_speed = speed;
            m_damage = damage;
        }

        public void LoadContent()
        {
            m_texture = m_level.Content.Load<Texture2D>("Sprites/Ammo/" + m_type);

            m_localBounds = new Rectangle(m_texture.Bounds.Left, m_texture.Bounds.Top, m_texture.Width, m_texture.Height);
        }

        private void HandleCollisions()
        {
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.WIDTH);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.WIDTH)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.HEIGHT);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.HEIGHT)) - 1;

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = m_level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = m_level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            // Resolve the collision along the shallow axis.
                            if (absDepthY < absDepthX || collision == TileCollision.Platform)
                            {
                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable)
                                {
                                    m_needDelete = true;
                                }
                            }
                            else if (collision == TileCollision.Impassable) // Ignore platforms.
                            {
                                m_needDelete = true;
                            }
                            else if (collision == TileCollision.Door)
                            {
                                m_needDelete = true;
                                m_level.OpenDoor(new Vector2(x, y), m_type);
                            }
                        }
                    }
                }
            }
        }

        public void Update(GameTime gamtTime)
        {
            if (m_direction == Direction.Left)
            {
                m_position.X -= m_speed;
            }
            else if (m_direction == Direction.Right)
            {
                m_position.X += m_speed;
            }


            HandleCollisions();
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Flip the sprite to face the way we are moving.
            if (m_direction == Direction.Left)
                flip = SpriteEffects.FlipHorizontally;
            else if (m_direction == Direction.Right)
                flip = SpriteEffects.None;

            // Draw that sprite.
            spriteBatch.Draw(m_texture, m_position, Color.White);
        }
    }
}
