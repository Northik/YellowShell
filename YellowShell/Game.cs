using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using YellowShell.Display;
using YellowShell.Editor;
using YellowShell.Level;

namespace YellowShell
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class GameYellowShell : Game
    {
        enum GameState
        {
            MainTitle,
            Playing,
            PlayerDead,
            GameOver,
            LevelEditor,
            Exiting
        }

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GameState gameState;
        Camera2D camera;

        Vector2 baseScreenSize = new Vector2(800, 480);
        private LevelManager m_level;
        private LevelEditor m_editor;

        private Matrix m_globalTransformation;
        private SpriteFont m_hudFont;

        public GameYellowShell()
        {
            graphics = new GraphicsDeviceManager(this);
            //graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            ChangeState(GameState.MainTitle);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Work out how much we need to scale our graphics to fill the screen
            float horScaling = graphics.GraphicsDevice.PresentationParameters.BackBufferWidth / baseScreenSize.X;
            float verScaling = graphics.GraphicsDevice.PresentationParameters.BackBufferHeight / baseScreenSize.Y;
            Vector3 screenScalingFactor = new Vector3(horScaling, verScaling, 1);
            m_globalTransformation = Matrix.CreateScale(screenScalingFactor);

            LoadNextLevel();
            camera = new Camera2D();

            m_editor = new LevelEditor(Services, camera);

            m_hudFont = Content.Load<SpriteFont>("Fonts/Hud");

        }

        private void LoadNextLevel()
        {
            // Unloads the content for the current level before loading the next one.
            if (m_level != null)
                m_level.Dispose();

            // Load the level.
            string levelPath = string.Format("Content/Levels/0.txt");
            using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                m_level = new LevelManager(Services, fileStream, 0);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                ChangeState(GameState.Exiting);
                Exit();
            }

            camera.SetFocalPoint(m_level.Player.Position, m_level.Dimension, graphics.GraphicsDevice.Viewport.Bounds);
            camera.Update();


            switch (gameState)
            {
                case GameState.MainTitle:
                    UpdateMainMenu(Keyboard.GetState());
                    break;
                case GameState.LevelEditor:
                    camera.SetFocalPoint(Mouse.GetState().Position.ToVector2() - new Vector2(200, 100), m_editor.Dimension, graphics.GraphicsDevice.Viewport.Bounds);
                    camera.Update();

                    m_editor.Update(gameTime, Keyboard.GetState(), Mouse.GetState());
                    break;
                case GameState.Playing:
                case GameState.PlayerDead:
                case GameState.GameOver:

                    m_level.Update(gameTime, Keyboard.GetState(), GamePad.GetState(PlayerIndex.One), Window.CurrentOrientation); 

                    break;
                case GameState.Exiting:
                    break;
                default:
                    throw new NotSupportedException("Doesn't know what happening!!");
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, camera.ViewMatrix); // m_globalTransformation);

            switch (gameState)
            {
                case GameState.MainTitle:
                    DrawMainMenu();
                    break;
                case GameState.LevelEditor:
                    m_editor.Draw(gameTime, spriteBatch);
                    break;
                case GameState.Playing:
                case GameState.PlayerDead:
                case GameState.GameOver:
                    m_level.Draw(gameTime, spriteBatch);
                    break;
                case GameState.Exiting:
                    break;
                default:
                    throw new NotSupportedException("Doesn't know what happening!!");
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void ChangeState(GameState newState)
        {
            gameState = newState;
            IsMouseVisible = gameState == GameState.MainTitle || gameState == GameState.LevelEditor;

        }

        private void DrawMainMenu()
        {
            DrawShadowedString(m_hudFont, "Press 'P' to play", new Vector2(10, 100), Color.Red);
            DrawShadowedString(m_hudFont, "Press 'E' to create a level", new Vector2(10, 50), Color.Red);

        }

        private void DrawShadowedString(SpriteFont font, string value, Vector2 position, Color color)
        {
            spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
            spriteBatch.DrawString(font, value, position, color);
        }

        private void UpdateMainMenu(KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.P))
            {
                ChangeState(GameState.Playing);
            }
            else if(keyboardState.IsKeyDown(Keys.E))
            {
                ChangeState(GameState.LevelEditor);
            }
        }

    }
}
