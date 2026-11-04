using TypeOEngine.Typedeaf.Basic2d.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Common;

namespace TypeOBasic2dTest
{
    public class Canvas2dClipTest
    {
        private sealed class CameraCanvas : TypeOEngine.Typedeaf.Core.Engine.Graphics.Canvas
        {
            public CameraCanvas() : base(null, new Rectangle(0, 0, 800, 600)) { }
            protected override void Initialize() { }
            protected override void Cleanup() { }
            public override void Clear(Color clearColor) { }
            public override void PreDraw() { }
            public override void PostDraw() { }
            public override void Present() { }
            public override TypeOEngine.Typedeaf.Core.Engine.Contents.Texture Screenshot(Rectangle screenRect) => null;
        }

        [Fact]
        public void ToScreen_WithoutCamera_KeepsTheRectangle()
        {
            var canvas = new CameraCanvas();

            var screen = Canvas2d.ToScreen(canvas, new Rectangle(10, 20, 30, 40));

            Assert.Equal(new Rectangle(10, 20, 30, 40), screen);
        }

        [Fact]
        public void ToScreen_FollowsTheCameraTranslationAndZoom()
        {
            var canvas = new CameraCanvas
            {
                WorldTranslation = new Vec3(100, 50, 0),
                WorldScale = 2
            };

            var screen = Canvas2d.ToScreen(canvas, new Rectangle(110, 70, 30, 40));

            Assert.Equal(new Rectangle(20, 40, 60, 80), screen);
        }

        [Fact]
        public void ToScissor_FlipsYToTheBottomLeftOrigin()
        {
            Assert.Equal((10, 500, 30, 40), Canvas2d.ToScissor(new Vec2(800, 600), new Rectangle(10, 60, 30, 40)));
        }

        [Fact]
        public void ToScissor_ClipsAreasPartlyOutsideTheViewport()
        {
            // Moved past the top-left corner, only the visible part remains
            Assert.Equal((0, 580, 20, 20), Canvas2d.ToScissor(new Vec2(800, 600), new Rectangle(-30, -20, 50, 40)));
            // Moved past the bottom-right corner
            Assert.Equal((780, 0, 20, 10), Canvas2d.ToScissor(new Vec2(800, 600), new Rectangle(780, 590, 50, 40)));
        }

        [Fact]
        public void ToScissor_AreaOutsideTheViewport_ClipsEverything()
        {
            var (_, _, width, height) = Canvas2d.ToScissor(new Vec2(800, 600), new Rectangle(-200, -200, 50, 50));

            Assert.Equal(0, width);
            Assert.Equal(0, height);
        }
    }
}
