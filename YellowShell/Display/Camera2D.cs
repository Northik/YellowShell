using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YellowShell.Display
{
    public class Camera2D
    {
        public Point Position
        {
            get
            {
                return m_position.ToPoint();
            }
        }

        private Vector2 m_position;

        public Matrix ViewMatrix
        {
            get;
            private set;
        }

        public Camera2D()
        {
            m_position = new Vector2();
        }

        public void SetFocalPoint(Vector2 focalPosition, Vector2 mapDimension, Rectangle viewportBound)
        {
            var temp = m_position;
            m_position = new Vector2(focalPosition.X - viewportBound.Width / 2, focalPosition.Y - viewportBound.Height / 2);    
            m_position = new Vector2(focalPosition.X - temp.X, focalPosition.Y);

            if (m_position.X < 0)
                m_position.X = 0;
            if (m_position.X > mapDimension.X - viewportBound.Width)
                m_position.X = mapDimension.X - viewportBound.Width;

            if (m_position.Y < 0)
                m_position.Y = 0;
            if (m_position.Y > mapDimension.Y - viewportBound.Height)
                m_position.Y = mapDimension.Y - viewportBound.Height;
        }

        public void Update()
        {
            ViewMatrix = Matrix.CreateTranslation(new Vector3(-m_position, 0));
        }

    }
}
