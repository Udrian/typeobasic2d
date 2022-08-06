using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Core.Engine.Services;

namespace TypeOEngine.Typedeaf.Basic2d
{
    namespace Engine.Services
    {
        public class BasicCamera2dService : Service
        {
            protected ICanvas Canvas { get; set; }
            public Vec2 Position { get { return Canvas.WorldMatrix.Translation; } set { Canvas.WorldMatrix.Translation = value; }  }

            public void SetCanvas(ICanvas canvas)
            {
                Canvas = canvas;
            }

            protected override void Initialize()
            {
            }

            protected override void Cleanup()
            {
            }
        }
    }
}
