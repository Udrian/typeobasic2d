using System.Collections.Generic;
using TypeOEngine.Typedeaf.Basic2d;
using TypeOEngine.Typedeaf.Basic2d.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Basic2d.Engine.Services;
using TypeOEngine.Typedeaf.Core;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities;
using Xunit;

namespace TypeOBasic2dTest
{
    public class BasicCamera2dTest
    {
        public string GameName { get; set; } = "test";

        public class TestGame : Game
        {
            public SceneList Scenes { get; set; }

            public BasicCamera2dService Camera { get; set; }
            public TestCanvas Canvas { get; set; }

            public override void Initialize()
            {
                Scenes = CreateSceneHandler();

                Canvas = new TestCanvas();
                Camera.SetCanvas(Canvas);
            }

            public override void Update(double dt)
            {
                Exit();
            }

            public override void Draw()
            {
            }

            public override void Cleanup()
            {
            }
        }

        public class TestCanvas : TypeObject, ICanvas2d
        {
            public IWindow Window { get; set; }
            public Rectangle Viewport { get; set; }
            public Matrix WorldMatrix { get; set; }

            protected override void Initialize()
            {
            }

            protected override void Cleanup()
            {
            }

            public void Clear(Color clearColor)
            {
            }

            public void DrawImage(Texture texture, Vec2 pos, IAnchor2d anchor = null)
            {
            }

            public void DrawImage(Texture texture, Vec2 pos, Vec2? scale = null, double rotation = 0, Vec2? origin = null, Color? color = null, Flipped flipped = Flipped.None, Rectangle? source = null, IAnchor2d anchor = null)
            {
            }

            public void DrawLine(Vec2 from, Vec2 size, Color color, IAnchor2d anchor = null)
            {
            }

            public void DrawLineE(Vec2 from, Vec2 to, Color color, IAnchor2d anchor = null)
            {
            }

            public void DrawLines(List<Vec2> points, Color color, IAnchor2d anchor = null)
            {
            }

            public void DrawPixel(Vec2 point, Color color, IAnchor2d anchor = null)
            {
            }

            public void DrawPixels(List<Vec2> points, Color color, IAnchor2d anchor = null)
            {
            }

            public void DrawRectangle(Rectangle rectangle, bool filled, Color color, IAnchor2d anchor = null)
            {
            }

            public void DrawRectangle(Vec2 from, Vec2 size, bool filled, Color color, IAnchor2d anchor = null)
            {
            }

            public void DrawRectangleE(Vec2 from, Vec2 to, bool filled, Color color, IAnchor2d anchor = null)
            {
            }

            public void DrawText(Font font, string text, Vec2 pos, IAnchor2d anchor = null)
            {
            }

            public void DrawText(Font font, string text, Vec2 pos, Vec2? scale = null, double rotation = 0, Vec2? origin = null, Color? color = null, Flipped flipped = Flipped.None, Rectangle? source = null, IAnchor2d anchor = null)
            {
            }

            public void Present()
            {
            }
        }

        [Fact]
        public void LoadBasicCamera2dService()
        {
            var typeO = TypeO.Create<TestGame>(GameName)
                 .LoadModule<Basic2dModule>() as TypeO;
            typeO.Start();

            var game = typeO.Context.Game as TestGame;

            Assert.NotEmpty(typeO.Context.Services);
            Assert.NotNull(typeO.Context.Services[typeof(BasicCamera2dService)]);

            Assert.NotNull(game.Camera);
            Assert.IsType<BasicCamera2dService>(game.Camera);

            Assert.NotNull(game.Canvas);
            Assert.IsType<TestCanvas>(game.Canvas);
        }

        [Fact]
        public void SetCameraPosition()
        {
            var typeO = TypeO.Create<TestGame>(GameName)
                 .LoadModule<Basic2dModule>() as TypeO;
            typeO.Start();

            var game = typeO.Context.Game as TestGame;

            Assert.Equal(game.Camera.Position, Vec2.Zero);
            Assert.Equal(game.Canvas.WorldMatrix.Translation, Vec2.Zero);

            var newPos = new Vec2(15, 25);
            game.Camera.Position = newPos;

            Assert.Equal(game.Camera.Position, newPos);
            Assert.Equal(game.Canvas.WorldMatrix.Translation, newPos);
        }
    }
}
