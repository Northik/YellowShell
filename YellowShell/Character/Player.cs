
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using YellowShell.Display;
using YellowShell.Level;
using Microsoft.Xna.Framework.Content;
using YellowShell.Ammo;
using static YellowShell.Ammo.Ammo;
using YellowShell.Character.PowerUp;

namespace YellowShell.Character
{
    class Player
    {
        private const float GUN_Y_POSITION = 14.0f;
        private const float MOVE_ACCELERATION = 13000.0f;
        private const float MAX_MOVE_SPEED = 1750.0f;
        private const float GROUD_DRAG_FACTOR = 0.48f;
        private const float AIR_DRAG_FACTOR = 0.58f;

        // Constants for controlling vertical movement
        private const float MAX_JUMP_TIME = 0.35f;
        private const float JUMP_LAUNCH_VELOCITY = -3500.0f;
        private const float GRAVITY_ACCELERATION = 3400.0f;
        private const float MAX_FALL_SPEED = 550.0f;
        private const float JUMP_CONTROL_POWER = 0.14f;

        // Input configuration
        private const float MOVE_STICK_SCALE = 1.0f;
        private const float ACCELEROMETER_SCALE = 1.5f;
        private const Buttons JUMP_BUTTON = Buttons.A;
        private const Buttons FIRE_BUTTON = Buttons.X;
        private const Buttons CHANGE_AMMO_BUTTON = Buttons.RightTrigger;

        private bool m_isJumping;
        private bool m_isShooting;
        private bool m_wasJumping;
        private bool m_missileUnlock;
        private float m_previousBottom;
        private float m_jumpTime;
        private float m_movement;
        private Rectangle m_localBounds;

        private Direction m_facing;

        // Animations
        private Animation idleAnimation;
        private Animation idleShootingAnimation;
        private Animation runAnimation;
        private Animation runShootingAnimation;
        private Animation jumpAnimation;
        private Animation celebrateAnimation;
        private Animation dieAnimation;
        private SpriteEffects flip = SpriteEffects.None;
        private AnimationManager sprite;
        private FiringManager m_firingManager;


        bool m_isAlive;
        bool m_isOnGround;
        Vector2 m_position;
        Vector2 m_velocity;
        LevelManager m_level;

        // Physics state
        public Vector2 Position
        {
            get { return m_position; }
            set { m_position = value; }
        }


        public Vector2 Velocity
        {
            get { return m_velocity; }
            set { m_velocity = value; }
        }

        public bool IsOnGround
        {
            get { return m_isOnGround; }
        }
       
        public LevelManager Level
        {
            get { return m_level; }
        }

        public bool IsAlive
        {
            get { return m_isAlive; }
        }

        /// <summary>
        /// Gets a rectangle which bounds this player in world space.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + m_localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + m_localBounds.Y;

                return new Rectangle(left, top, m_localBounds.Width, m_localBounds.Height);
            }
        }

        public Player(LevelManager level, Vector2 position)
        {
            this.m_level = level;
            m_firingManager = new FiringManager();
            m_facing = Direction.Right;
            m_missileUnlock = false;

            LoadContent();

            Reset(position);
        }

        /// <summary>
        /// Loads the player sprite sheet and sounds.
        /// </summary>
        public void LoadContent()
        {
            // Load animated textures.
            idleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Idle"), 0.1f, true);
            idleShootingAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Idle_Shooting"), 0.1f, true);
            runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Run"), 0.1f, true);
            runShootingAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Run_Shooting"), 0.1f, true);
            jumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Jump"), 0.1f, false);
            celebrateAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Celebrate"), 0.1f, false);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Die"), 0.1f, false);

            // Calculate bounds within texture size.            
            int width = (int)(idleAnimation.FrameWidth * 0.4);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.8);
            int top = idleAnimation.FrameHeight - height;
            m_localBounds = new Rectangle(left, top, width, height);
            
            // Load sounds.            
            //killedSound = Level.Content.Load<SoundEffect>("Sounds/PlayerKilled");
            //jumpSound = Level.Content.Load<SoundEffect>("Sounds/PlayerJump");
            //fallSound = Level.Content.Load<SoundEffect>("Sounds/PlayerFall");
        }

        /// <summary>
        /// Resets the player to life.
        /// </summary>
        /// <param name="position">The position to come to life at.</param>
        public void Reset(Vector2 position)
        {
            Position = position;
            Velocity = Vector2.Zero;
            m_isAlive = true;
            m_isJumping = false;
            m_movement = 0;
            sprite.PlayAnimation(idleAnimation);
        }

        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = Position;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            m_velocity.X += m_movement * MOVE_ACCELERATION * elapsed;
            m_velocity.Y = MathHelper.Clamp(m_velocity.Y + GRAVITY_ACCELERATION * elapsed, -MAX_FALL_SPEED, MAX_FALL_SPEED);

            m_velocity.Y = DoJump(m_velocity.Y, gameTime);

            // Apply pseudo-drag horizontally.
            if (IsOnGround)
                m_velocity.X *= GROUD_DRAG_FACTOR;
            else
                m_velocity.X *= AIR_DRAG_FACTOR;

            // Prevent the player from running faster than his top speed.            
            m_velocity.X = MathHelper.Clamp(m_velocity.X, -MAX_MOVE_SPEED, MAX_MOVE_SPEED);

            // Apply m_velocity.
            Position += m_velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            // If the player is now colliding with the level, separate them.
            HandleCollisions();

            // If the collision stopped us from moving, reset the m_velocity to zero.
            if (Position.X == previousPosition.X)
                m_velocity.X = 0;

            if (Position.Y == previousPosition.Y)
                m_velocity.Y = 0;
        }

        private float DoJump(float velocityY, GameTime gameTime)
        {
            // If the player wants to jump
            if (m_isJumping)
            {
                // Begin or continue a jump
                if ((!m_wasJumping && IsOnGround) || m_jumpTime > 0.0f)
                {
                    //if (m_jumpTime == 0.0f)
                    //    jumpSound.Play();

                    m_jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    sprite.PlayAnimation(jumpAnimation);
                }

                // If we are in the ascent of the jump
                if (0.0f < m_jumpTime && m_jumpTime <= MAX_JUMP_TIME)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocityY = JUMP_LAUNCH_VELOCITY * (1.0f - (float)Math.Pow(m_jumpTime / MAX_JUMP_TIME, JUMP_CONTROL_POWER));
                }
                else
                {
                    // Reached the apex of the jump
                    m_jumpTime = 0.0f;
                }
            }
            else
            {
                // Continues not jumping or cancels a jump in progress
                m_jumpTime = 0.0f;
            }
            m_wasJumping = m_isJumping;

            return velocityY;
        }

        private void HandleCollisions()
        {
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.WIDTH);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.WIDTH)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.HEIGHT);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.HEIGHT)) - 1;

            // Reset flag to search for ground collision.
            m_isOnGround = false;

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = Level.GetCollision(x, y);

                    if (collision == TileCollision.PowerUp)
                    {
                        m_level.ActivatePowerUp(new Vector2(x, y));
                    }
                    else if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            // Resolve the collision along the shallow axis.
                            if (absDepthY < absDepthX || collision == TileCollision.Platform)
                            {
                                // If we crossed the top of a tile, we are on the ground.
                                if (m_previousBottom <= tileBounds.Top)
                                    m_isOnGround = true;

                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable || collision == TileCollision.Door || IsOnGround)
                                {
                                    // Resolve the collision along the Y axis.
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);

                                    // Perform further collisions with the new bounds.
                                    bounds = BoundingRectangle;
                                }
                            }
                            else if (collision == TileCollision.Impassable || collision == TileCollision.Door) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                Position = new Vector2(Position.X, Position.Y);

                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;
                            }                            
                        }
                    }
                }
            }

            // Save the new bounds bottom.
            m_previousBottom = bounds.Bottom;
        }

        /// <summary>
        /// Handles input, performs physics, and animates the player sprite.
        /// </summary>
        /// <remarks>
        /// We pass in all of the input states so that our game is only polling the hardware
        /// once per frame. We also pass the game's orientation because when using the accelerometer,
        /// we need to reverse our motion when the orientation is in the LandscapeRight orientation.
        /// </remarks>
        public void Update(GameTime gameTime, KeyboardState keyboardState, GamePadState gamePadState, DisplayOrientation orientation)
        {
            GetInput(keyboardState, gamePadState, /*accelState,*/ orientation);
            m_firingManager.Update(gameTime);

            ApplyPhysics(gameTime);

            if (IsAlive && IsOnGround)
            {
                if (Math.Abs(Velocity.X) - 0.02f > 0)
                {
                    if (m_isShooting)
                    {
                        sprite.PlayAnimation(runShootingAnimation);
                    }
                    else
                    {
                        sprite.PlayAnimation(runAnimation);
                    }
                }
                else
                {
                    if (m_isShooting)
                    {
                        sprite.PlayAnimation(idleShootingAnimation);
                    }
                    else
                    {
                        sprite.PlayAnimation(idleAnimation);
                    }
                }
            }

            if(m_isShooting)
            {
                if (m_missileUnlock)
                {
                    m_firingManager.FireShot(gameTime, new Ammo_Missile(Level, new Vector2(m_position.X, m_position.Y - 48 + GUN_Y_POSITION), m_facing));
                }
                else
                {
                    m_firingManager.FireShot(gameTime, new Ammo_Standard(Level, new Vector2(m_position.X, m_position.Y - 48 + GUN_Y_POSITION), m_facing));
                }
            }

            // Clear input.
            m_movement = 0.0f;
            m_isJumping = false;

        }

        /// <summary>
        /// Gets player horizontal movement and jump commands from input.
        /// </summary>
        private void GetInput(KeyboardState keyboardState, GamePadState gamePadState, DisplayOrientation orientation)
        {
            // Get analog horizontal movement.
            m_movement = gamePadState.ThumbSticks.Left.X * MOVE_STICK_SCALE;

            // Ignore small movements to prevent running in place.
            if (Math.Abs(m_movement) < 0.5f)
                m_movement = 0.0f;

            // If any digital horizontal movement input is found, override the analog movement.
            if (gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                keyboardState.IsKeyDown(Keys.Left) ||
                keyboardState.IsKeyDown(Keys.A))
            {
                m_movement = -1.0f;
                m_facing = Direction.Left;
            }
            else if (gamePadState.IsButtonDown(Buttons.DPadRight) ||
                     keyboardState.IsKeyDown(Keys.Right) ||
                     keyboardState.IsKeyDown(Keys.D))
            {
                m_movement = 1.0f;
                m_facing = Direction.Right;
            }

            // Check if the player wants to jump.
            m_isJumping = gamePadState.IsButtonDown(JUMP_BUTTON) ||
                          keyboardState.IsKeyDown(Keys.Space) ||
                          keyboardState.IsKeyDown(Keys.Up) ||
                          keyboardState.IsKeyDown(Keys.W);

            m_isShooting = gamePadState.IsButtonDown(FIRE_BUTTON) ||
                         keyboardState.IsKeyDown(Keys.RightShift);  
        }

        /// <summary>
        /// Draws the animated player.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Flip the sprite to face the way we are moving.
            if (Velocity.X > 0)
                flip = SpriteEffects.FlipHorizontally;
            else if (Velocity.X < 0)
                flip = SpriteEffects.None;

            // Draw that sprite.
            sprite.Draw(gameTime, spriteBatch, Position, flip);
            m_firingManager.Draw(gameTime, spriteBatch);
        }

        /// <summary>
        /// Called when the player has been killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This parameter is null if the player was
        /// not killed by an enemy (fell into a hole).
        /// </param>
        public void OnKilled(Enemy killedBy)
        {
            m_isAlive = false;
        }

        /// <summary>
        /// Called when this player reaches the level's exit.
        /// </summary>
        public void OnReachedExit()
        {
            //sprite.PlayAnimation(celebrateAnimation);
        }

        public void ActivatePowerUp(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.Missile:
                    m_missileUnlock = true;
                    break;
                default:
                    break;
            }
        }
    }
}
