using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace YellowShell.Editor
{
    enum ObjectType
    {
        Empty = -1, // Use only for TileEditor

        Exit = 0,

        Player = 1,

        Enemy = 2,

        Tile = 3,

        DoorStandard = 4, 

        DoorMissile = 5,

        Missile_PU = 6,
    }
    class GameObjectButton
    {
        public const int WIDTH = 64;
        public const int HEIGHT = 64;
        public static readonly Point SIZE = new Point(WIDTH, HEIGHT);
        public Texture2D Texture { get; private set; }
        public Rectangle ButtonRectangle { get; private set; }
        public ObjectType ObjectType { get; private set; }

        private ContentManager m_content;



        public GameObjectButton(ContentManager content, ObjectType type, Vector2 position)
        {
            m_content = content;
            ObjectType = type;

            ButtonRectangle = new Rectangle(position.ToPoint(), SIZE);

            LoadContent();
        }

        public void LoadContent()
        {
            if (ObjectType == ObjectType.Empty)
            {
                throw new System.Exception("You should not have an Empty type GameObject");
            }

            Texture = m_content.Load<Texture2D>("Sprites/Editor/Button/" + ObjectType + "_Button");
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, ButtonRectangle, Color.White);
        }
    }
}
