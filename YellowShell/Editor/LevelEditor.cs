using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using YellowShell.Display;

namespace YellowShell.Editor
{
    class LevelEditor
    {
    
        public static readonly int BUTTON_SPACING = 10;

        public const String EDIT_LEVEL_FILE = "edit_level.txt";
        public const int MAP_TOP = 100;
        public const int MAP_WIDTH = 24;
        public const int MAP_HEIGTH = 16;

        public ContentManager Content { get; private set; }
        public Vector2 Dimension
        {
            get
            {
                return m_dimension;
            }
        }

        private List<GameObjectButton> m_buttons;
        private int m_selectedButtonIndex;

        private TileEditor[,] m_tiles;
        private Vector2 m_dimension;
        private Camera2D m_camera;



        public LevelEditor(IServiceProvider serviceProvider, Camera2D camera)
        {
            m_camera = camera;
            m_dimension = new Vector2(MAP_WIDTH * TileEditor.WIDTH, MAP_HEIGTH * TileEditor.HEIGHT + MAP_TOP);
            Content = new ContentManager(serviceProvider, "Content");


            m_selectedButtonIndex = -1;
            m_tiles = new TileEditor[24, 16];
            m_buttons = new List<GameObjectButton>();

            m_buttons.Add(new GameObjectButton(Content, ObjectType.DoorMissile, new Vector2(BUTTON_SPACING, 10)));
            m_buttons.Add(new GameObjectButton(Content, ObjectType.DoorStandard, new Vector2(GameObjectButton.WIDTH + BUTTON_SPACING * 2, 10)));
            m_buttons.Add(new GameObjectButton(Content, ObjectType.Enemy, new Vector2(GameObjectButton.WIDTH * 2 + BUTTON_SPACING * 3, 10)));
            m_buttons.Add(new GameObjectButton(Content, ObjectType.Exit, new Vector2(GameObjectButton.WIDTH * 3 + BUTTON_SPACING * 4, 10)));
            m_buttons.Add(new GameObjectButton(Content, ObjectType.Missile_PU, new Vector2(GameObjectButton.WIDTH * 4 + BUTTON_SPACING * 5, 10)));
            m_buttons.Add(new GameObjectButton(Content, ObjectType.Player, new Vector2(GameObjectButton.WIDTH * 5 + BUTTON_SPACING * 6, 10)));
            m_buttons.Add(new GameObjectButton(Content, ObjectType.Tile, new Vector2(GameObjectButton.WIDTH * 6 + BUTTON_SPACING * 7, 10)));

            LoadMap();
            
        }

        private void LoadMap()
        {
            for (int y = 0; y < MAP_HEIGTH; ++y)
            {
                for (int x = 0; x < MAP_WIDTH; ++x)
                {
                    m_tiles[x, y] = new TileEditor(Content, new Vector2(x, y));
                }
            }
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState, MouseState mouseState)
        {
            HandleInput(mouseState, keyboardState);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawButtons(spriteBatch);
            DrawMap(spriteBatch);
        }

        private void DrawButtons(SpriteBatch spriteBatch)
        {
            foreach (GameObjectButton button in m_buttons)
            {
                button.Draw(spriteBatch);
            }
        }

        private void DrawMap(SpriteBatch spriteBatch)
        {
            for (int y = 0; y < MAP_HEIGTH; ++y)
            {
                for (int x = 0; x < MAP_WIDTH; ++x)
                {
                    m_tiles[x, y].Draw(spriteBatch);
                }
            }
        }

        private void HandleInput(MouseState mouseState, KeyboardState keyboardState)
        {
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                Point mousePosition = mouseState.Position + m_camera.Position;
                if (mouseState.Position.Y < MAP_TOP)
                {
                    for (int i = 0; i < m_buttons.Count; i++)
                    {
                        if (m_buttons[i].ButtonRectangle.Contains(mousePosition))
                        {
                            m_selectedButtonIndex = i;
                            break;
                        }
                    }
                }
                else if (m_selectedButtonIndex != -1)
                {
                    Point tileClicked = new Point(mousePosition.X / TileEditor.WIDTH, (mousePosition.Y - 100) / TileEditor.HEIGHT);

                    if (tileClicked.X >= 0 && tileClicked.X < MAP_WIDTH && tileClicked.Y >= 0 && tileClicked.Y < MAP_HEIGTH)
                    {
                        m_tiles[tileClicked.X, tileClicked.Y].SetObject(m_buttons[m_selectedButtonIndex].ObjectType);
                    }
                }
            }
            else if (mouseState.RightButton == ButtonState.Pressed)
            {
                Point mousePosition = mouseState.Position + m_camera.Position;
                if (mouseState.Position.Y > MAP_TOP)
                {
                    Point tileClicked = new Point(mousePosition.X / TileEditor.WIDTH, (mousePosition.Y - 100) / TileEditor.HEIGHT);

                    if (tileClicked.X >= 0 && tileClicked.X < MAP_WIDTH && tileClicked.Y >= 0 && tileClicked.Y < MAP_HEIGTH)
                    {
                        m_tiles[tileClicked.X, tileClicked.Y].SetObject(ObjectType.Empty);
                    }
                }
            }

            if (keyboardState.IsKeyDown(Keys.F1))
            {
                SaveLevel();
            }
        }

        private void SaveLevel()
        {
            StorageDevice device = getStorageDevice();

            IAsyncResult result = device.BeginOpenContainer("SaveLevel", null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            StorageContainer container = device.EndOpenContainer(result);

            // Close the wait handle.
            result.AsyncWaitHandle.Close();

            Stream file = null;

            file = File.Create(EDIT_LEVEL_FILE);

            //file = container.OpenFile(EDIT_LEVEL_FILE, FileMode.Open);

            String leveltext = "";
            for (int y       = 0; y < MAP_HEIGTH; ++y)
            {
                for (int x = 0; x < MAP_WIDTH; ++x)
                {
                    switch (m_tiles[x, y].ObjectInside)
                    {
                        case ObjectType.Empty:
                            leveltext +=  ".";
                            break;
                        case ObjectType.Exit:
                            leveltext += "X";
                            break;
                        case ObjectType.Player:
                            leveltext += "1";
                            break;
                        case ObjectType.Enemy:
                            leveltext += "E";
                            break;
                        case ObjectType.Tile:
                            leveltext += "#";
                            break;
                        case ObjectType.DoorStandard:
                            leveltext += "D";
                            break;
                        case ObjectType.DoorMissile:
                            leveltext += "F";
                            break;
                        case ObjectType.Missile_PU:
                            leveltext += "M";
                            break;
                        default:
                            break;
                    }
                }
                leveltext += "\n";
            }
            file.Write(System.Text.Encoding.ASCII.GetBytes(leveltext), 0, (MAP_HEIGTH * MAP_WIDTH) + MAP_HEIGTH);
            file.Close();

            // Dispose the container.
            container.Dispose();
        }

        private StorageDevice getStorageDevice()
        {
            IAsyncResult result = StorageDevice.BeginShowSelector(PlayerIndex.One, null, null);

            result.AsyncWaitHandle.WaitOne();
            StorageDevice device = StorageDevice.EndShowSelector(result);
            result.AsyncWaitHandle.Close();
            return device;
        }
    }
}
