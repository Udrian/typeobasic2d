using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Engine.Services;

namespace TypeOEngine.Typedeaf.Basic2d
{
    namespace Services
    {
        public class BasicCamera2dService : Service
        {
            protected Canvas Canvas { get; set; }
            public Vec2 Position { get { return Canvas.WorldMatrix.Translation; } set { Canvas.WorldMatrix.Translation = value; }  }

            public void SetCanvas(Canvas canvas)
            {
                Canvas = canvas;
            }

            public override void Initialize()
            {
            }

            public override void Cleanup()
            {
            }
        }
    }
}
