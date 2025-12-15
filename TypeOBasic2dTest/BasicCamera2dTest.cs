using TypeOBasic2dTest.TypeTest;
using TypeOBasic2dTest.TypeTest.Mock;
using TypeOEngine.Typedeaf.Basic2d.Engine.Services;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.TK.Engine.Graphics;
using Xunit;

namespace TypeOBasic2dTest
{
    public class BasicCamera2dTest
    {
        [Fact]
        public void LoadBasicCamera2dService()
        {
            var typeO = Utility.CreateTypeO((context) =>
            {
                var window = Utility.CreateTKWindow(context.TypeO);
                var canvas = Utility.CreateTKCanvas(context);

                var game = context.Game as TestGameMock;
                game.Camera.SetCanvas(canvas);
            });
            typeO.Start();

            var game = typeO.Context.Game as TestGameMock;

            Assert.NotEmpty(typeO.Context.Services);
            Assert.NotNull(typeO.Context.Services[typeof(BasicCamera2dService)]);

            Assert.NotNull(game.Camera);
            Assert.IsType<BasicCamera2dService>(game.Camera);

            Assert.NotNull(typeO.Context.Game.MainWindow.Canvas);
            Assert.IsType<TKCanvas>(typeO.Context.Game.MainWindow.Canvas);
        }

        [Fact]
        public void SetCameraPosition()
        {
            var typeO = Utility.CreateTypeO((context) =>
            {
                var window = Utility.CreateTKWindow(context.TypeO);
                var canvas = Utility.CreateTKCanvas(context);

                var game = context.Game as TestGameMock;
                game.Camera.SetCanvas(canvas);
            });
            typeO.Start();

            var game = typeO.Context.Game as TestGameMock;

            Assert.Equal(game.Camera.Position, Vec2.Zero);
            Assert.Equal(typeO.Context.Game.MainWindow.Canvas.WorldTranslation, Vec3.Zero);

            var newPos = new Vec2(15, 25);
            game.Camera.Position = newPos;

            Assert.Equal(game.Camera.Position, newPos);
            Assert.Equal(typeO.Context.Game.MainWindow.Canvas.WorldTranslation, new Vec3(newPos.X, newPos.Y, 0));
        }
    }
}
