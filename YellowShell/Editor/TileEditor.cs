

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace YellowShell.Editor
{
    class TileEditor
    {
        public ObjectType ObjectInside { get; private set; }
        public static Texture2D Texture { get; private set; }
        public Vector2 Position { get; private set; }

        public const int WIDTH = 64;
        public const int HEIGHT = 32;

        public static readonly Vector2 SIZE = new Vector2(WIDTH, HEIGHT);


        private static bool m_textureInitialize = false;
        private ContentManager m_content;
        private static Dictionary<ObjectType, Texture2D> m_objectTexture;

        
        public TileEditor(ContentManager content, Vector2 position)
        {
            m_content = content;

            ObjectInside = ObjectType.Empty;
            Position = position;

            LoadContent();
        }

        private void LoadContent()
        {
            if (!m_textureInitialize)
            {
                Texture = m_content.Load<Texture2D>("Sprites/Editor/EmptyTile");

                m_objectTexture = new Dictionary<ObjectType, Texture2D>();
                m_objectTexture.Add(ObjectType.DoorMissile, m_content.Load<Texture2D>("Sprites/Door/Door_2"));
                m_objectTexture.Add(ObjectType.DoorStandard, m_content.Load<Texture2D>("Sprites/Door/Door_1"));
                m_objectTexture.Add(ObjectType.Enemy, m_content.Load<Texture2D>("Sprites/Monster/Idle"));
                m_objectTexture.Add(ObjectType.Exit, m_content.Load<Texture2D>("Sprites/Tiles/Exit"));
                m_objectTexture.Add(ObjectType.Missile_PU, m_content.Load<Texture2D>("Sprites/PowerUp/Missile_PU"));
                m_objectTexture.Add(ObjectType.Player, m_content.Load<Texture2D>("Sprites/Player/Idle"));
                m_objectTexture.Add(ObjectType.Tile, m_content.Load<Texture2D>("Sprites/Tiles/Tile_A0"));

                m_textureInitialize = true;
            }

        }

        public void SetObject(ObjectType type)
        {
            if (ObjectInside == ObjectType.Empty || type == ObjectType.Empty)
            {
                ObjectInside = type;
            }
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 position = Position * SIZE;
            position.Y += LevelEditor.MAP_TOP;
            spriteBatch.Draw(Texture, position, Color.White);

            if (ObjectInside != ObjectType.Empty)
            {
                DrawGameObject(spriteBatch);
            }
        }

        private void DrawGameObject(SpriteBatch spriteBatch)
        {
            if (ObjectInside != ObjectType.Empty)
            { 
                Vector2 position = Position * SIZE;
                position.Y += LevelEditor.MAP_TOP - (m_objectTexture[ObjectInside].Height - HEIGHT);

                spriteBatch.Draw(m_objectTexture[ObjectInside], position, Color.White);
                }
        }


    }
}
