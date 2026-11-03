using TypeOBasic2dTest.TypeTest;
using TypeOBasic2dTest.TypeTest.Mock;
using TypeOEngine.Typedeaf.Core.Common;

namespace TypeOBasic2dTest
{
    public class BasicCamera2dPositionTest
    {
        [Fact]
        public void Position_RoundTripsThroughCanvasWorldTranslation()
        {
            var typeO = Utility.CreateTypeO((context) =>
            {
                Utility.CreateTKWindow(context.TypeO);
                var canvas = Utility.CreateTKCanvas(context);
                var camera = ((TestGameMock)context.Game).Camera;
                camera.SetCanvas(canvas);

                camera.Position = new Vec2(12, 34);

                Assert.Equal(new Vec2(12, 34), camera.Position);
                Assert.Equal(new Vec3(12, 34, 0), canvas.WorldTranslation);
            });
            typeO.Start();
        }

        [Fact]
        public void SetCanvas_SwitchesTheCanvasTheCameraMoves()
        {
            var typeO = Utility.CreateTypeO((context) =>
            {
                Utility.CreateTKWindow(context.TypeO);
                var first = Utility.CreateTKCanvas(context);
                var second = Utility.CreateTKCanvas(context);
                var camera = ((TestGameMock)context.Game).Camera;

                camera.SetCanvas(first);
                camera.Position = new Vec2(1, 2);
                camera.SetCanvas(second);
                camera.Position = new Vec2(3, 4);

                Assert.Equal(new Vec3(1, 2, 0), first.WorldTranslation);
                Assert.Equal(new Vec3(3, 4, 0), second.WorldTranslation);
            });
            typeO.Start();
        }
    }
}
