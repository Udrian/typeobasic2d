using TypeOBasic2dTest.TypeTest;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Basic2d.Engine.Graphics;
using Xunit;
using System.Collections.Generic;
using System.IO;
using TypeOEngine.Typedeaf.TK.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Contents.ContentExtensions;

namespace TypeOBasic2dTest
{
    public class Canvas2dTest
    {
        [Fact]
        public void DrawLineTest()
        {
            var typeO = Utility.DrawTest((context) =>
            {
                var size = context.Game.Scenes.Canvas.Viewport.Size;

                //Make sure the screen is clear
                context.Game.Scenes.Canvas.Clear(Color.DarkCyan);
                context.Game.Scenes.Canvas.Present();

                var texture = context.Game.Scenes.Canvas.Screenshot();
                Assert.NotNull(texture);
                Assert.Equal(Color.DarkCyan, texture.PixelAt(0, 0));
                Assert.Equal(Color.DarkCyan, texture.PixelAt((int)size.X / 2, (int)size.Y / 2));
                Assert.Equal(Color.DarkCyan, texture.PixelAt((int)size.X - 1, (int)size.Y - 1));

                //Try to draw a line accross the entire screen
                context.Game.Scenes.Canvas.Clear(Color.DarkCyan);
                context.Game.Scenes.Canvas.DrawLine(new Vec2(0, 0), new Vec2(size.X, size.Y), Color.SkyBlue);
                context.Game.Scenes.Canvas.Present();

                texture = context.Game.Scenes.Canvas.Screenshot();
                Assert.NotNull(texture);
                Assert.Equal(Color.SkyBlue, texture.PixelAt(1, 1));
                Assert.Equal(Color.SkyBlue, texture.PixelAt((int)size.X / 2, (int)size.Y / 2));
                Assert.Equal(Color.SkyBlue, texture.PixelAt((int)size.X - 1, (int)size.Y - 1));

                //Try to draw a absolute line
                context.Game.Scenes.Canvas.Clear(Color.DarkCyan);
                context.Game.Scenes.Canvas.DrawLineA(new Vec2(25, 25), new Vec2(10, 10), Color.SkyBlue);
                context.Game.Scenes.Canvas.Present();

                texture = context.Game.Scenes.Canvas.Screenshot();
                Assert.NotNull(texture);
                Assert.Equal(Color.SkyBlue, texture.PixelAt(11, 11));
                Assert.Equal(Color.SkyBlue, texture.PixelAt(25, 25));

                //Try to draw multiple lines
                context.Game.Scenes.Canvas.Clear(Color.DarkCyan);
                context.Game.Scenes.Canvas.DrawLines(new List<Vec2>{ new Vec2(50, 50), new Vec2(55, 40), new Vec2(65,100), new Vec2(25,125)}, Color.SkyBlue);
                context.Game.Scenes.Canvas.Present();

                texture = context.Game.Scenes.Canvas.Screenshot();
                Assert.NotNull(texture);
                Assert.Equal(Color.SkyBlue, texture.PixelAt(51, 49));
                Assert.Equal(Color.SkyBlue, texture.PixelAt(56, 41));
                Assert.Equal(Color.SkyBlue, texture.PixelAt(64, 101));

                context.Exit();
            });
            typeO.Start();
        }

        [Fact]
        public void DrawPixelTest()
        {
            var typeO = Utility.DrawTest((context) =>
            {
                var size = context.Game.Scenes.Canvas.Viewport.Size;

                //Make sure the screen is clear
                context.Game.Scenes.Canvas.Clear(Color.DarkCyan);
                context.Game.Scenes.Canvas.Present();

                var texture = context.Game.Scenes.Canvas.Screenshot();
                Assert.NotNull(texture);
                Assert.Equal(Color.DarkCyan, texture.PixelAt(0, 0));
                Assert.Equal(Color.DarkCyan, texture.PixelAt((int)size.X-1, 0));
                Assert.Equal(Color.DarkCyan, texture.PixelAt((int)size.X-1, (int)size.Y-1));
                Assert.Equal(Color.DarkCyan, texture.PixelAt(0, (int)size.Y-1));

                //Try to draw pixels in every corner
                context.Game.Scenes.Canvas.Clear(Color.DarkCyan);
                context.Game.Scenes.Canvas.DrawPixel(new Vec2(0, 0), Color.SoftBlack);
                context.Game.Scenes.Canvas.DrawPixel(new Vec2(size.X-1, 0), Color.SkyBlue);
                context.Game.Scenes.Canvas.DrawPixel(new Vec2(size.X-1, size.Y-1), Color.Orange);
                context.Game.Scenes.Canvas.DrawPixel(new Vec2(0, size.Y-1), Color.Cyan);
                context.Game.Scenes.Canvas.Present();

                texture = context.Game.Scenes.Canvas.Screenshot();
                Assert.NotNull(texture);
                Assert.Equal(Color.SoftBlack, texture.PixelAt(0, 0));
                Assert.Equal(Color.SkyBlue, texture.PixelAt((int)size.X-1, 0));
                Assert.Equal(Color.Orange, texture.PixelAt((int)size.X-1, (int)size.Y-1));
                Assert.Equal(Color.Cyan, texture.PixelAt(0, (int)size.Y-1));
                
                //Clear the screen again with another color
                context.Game.Scenes.Canvas.Clear(Color.LightYellow);
                context.Game.Scenes.Canvas.Present();
                
                texture = context.Game.Scenes.Canvas.Screenshot();
                Assert.NotNull(texture);
                Assert.Equal(Color.LightYellow, texture.PixelAt(0, 0));
                Assert.Equal(Color.LightYellow, texture.PixelAt((int)size.X-1, 0));
                Assert.Equal(Color.LightYellow, texture.PixelAt((int)size.X-1, (int)size.Y-1));
                Assert.Equal(Color.LightYellow, texture.PixelAt(0, (int)size.Y-1));

                //Try to draw multiple pixels at the same time in every corner
                context.Game.Scenes.Canvas.Clear(Color.LightYellow);
                context.Game.Scenes.Canvas.DrawPixels(new List<Vec2>() { new Vec2(0, 0), new Vec2(size.X-1, 0), new Vec2(size.X-1, size.Y-1), new Vec2(0, size.Y-1) }, Color.SoftBlack);
                context.Game.Scenes.Canvas.Present();
                
                texture = context.Game.Scenes.Canvas.Screenshot();
                Assert.NotNull(texture);
                Assert.Equal(Color.SoftBlack, texture.PixelAt(0, 0));
                Assert.Equal(Color.SoftBlack, texture.PixelAt((int)size.X-1, 0));
                Assert.Equal(Color.SoftBlack, texture.PixelAt((int)size.X-1, (int)size.Y-1));
                Assert.Equal(Color.SoftBlack, texture.PixelAt(0, (int)size.Y-1));
                
                context.Exit();
            });
            typeO.Start();
        }

        [Fact]
        public void DrawRectangleTest()
        {
            var typeO = Utility.DrawTest((context) =>
            {
                var size = context.Game.Scenes.Canvas.Viewport.Size;

                //Make sure the screen is clear
                context.Game.Scenes.Canvas.Clear(Color.DarkCyan);
                context.Game.Scenes.Canvas.Present();

                var texture = context.Game.Scenes.Canvas.Screenshot();
                Assert.NotNull(texture);
                Assert.Equal(Color.DarkCyan, texture.PixelAt(0, 0));
                Assert.Equal(Color.DarkCyan, texture.PixelAt((int)size.X - 1, 0));
                Assert.Equal(Color.DarkCyan, texture.PixelAt((int)size.X - 1, (int)size.Y - 1));
                Assert.Equal(Color.DarkCyan, texture.PixelAt(0, (int)size.Y - 1));

                //Try to draw a rectangle across the entire screen
                context.Game.Scenes.Canvas.Clear(Color.DarkCyan);
                context.Game.Scenes.Canvas.DrawRectangle(new Vec2(0,0), new Vec2(size.X - 1, size.Y - 1), false, Color.SkyBlue);
                context.Game.Scenes.Canvas.Present();

                texture = context.Game.Scenes.Canvas.Screenshot();
                Assert.NotNull(texture);
                Assert.Equal(Color.SkyBlue, texture.PixelAt(1, 0));
                Assert.Equal(Color.SkyBlue, texture.PixelAt((int)size.X - 1, 0));
                Assert.Equal(Color.SkyBlue, texture.PixelAt((int)size.X - 1, (int)size.Y - 2));
                Assert.Equal(Color.SkyBlue, texture.PixelAt(1, (int)size.Y - 1));

                //Try to draw a filled rectangle across the entire screen
                context.Game.Scenes.Canvas.Clear(Color.DarkCyan);
                context.Game.Scenes.Canvas.DrawRectangle(new Vec2(0,0), new Vec2(size.X - 1, size.Y - 1), true, Color.SkyBlue);
                context.Game.Scenes.Canvas.Present();

                texture = context.Game.Scenes.Canvas.Screenshot();
                Assert.NotNull(texture);
                Assert.Equal(Color.SkyBlue, texture.PixelAt(1, 1));
                Assert.Equal(Color.SkyBlue, texture.PixelAt((int)size.X - 2, 1));
                Assert.Equal(Color.SkyBlue, texture.PixelAt((int)size.X - 2, (int)size.Y - 2));
                Assert.Equal(Color.SkyBlue, texture.PixelAt(1, (int)size.Y - 2));
                Assert.Equal(Color.SkyBlue, texture.PixelAt((int)size.X / 2, (int)size.Y / 2));

                context.Exit();
            });
            typeO.Start();
        }

        [Fact]
        public void DrawImageTest()
        {
            var typeO = Utility.DrawTest((context) =>
            {
                //Load texture
                var tkTexture = context.Game.ContentLoader.LoadContent<TKTexture>(Path.Combine("Mock", "Resources", "texture.png"));
                Assert.NotNull(tkTexture);

                var size = context.Game.Scenes.Canvas.Viewport.Size;

                //Make sure the screen is clear
                context.Game.Scenes.Canvas.Clear(Color.DarkCyan);
                context.Game.Scenes.Canvas.Present();

                var texture = context.Game.Scenes.Canvas.Screenshot();
                Assert.NotNull(texture);
                Assert.Equal(Color.DarkCyan, texture.PixelAt(0, 0));
                Assert.Equal(Color.DarkCyan, texture.PixelAt((int)size.X - 1, 0));
                Assert.Equal(Color.DarkCyan, texture.PixelAt((int)size.X - 1, (int)size.Y - 1));
                Assert.Equal(Color.DarkCyan, texture.PixelAt(0, (int)size.Y - 1));

                //Try to draw the texture
                context.Game.Scenes.Canvas.Clear(Color.DarkCyan);
                context.Game.Scenes.Canvas.DrawImage(tkTexture, new Vec2(5));
                context.Game.Scenes.Canvas.Present();

                texture = context.Game.Scenes.Canvas.Screenshot();
                Assert.NotNull(texture);
                var textureCornerColor = new Color(128, 128, 128);
                var textureMiddleColor = new Color(64, 64, 64);
                Assert.Equal(textureCornerColor, texture.PixelAt(6, 6));
                Assert.Equal(textureCornerColor, texture.PixelAt(6+38, 6));
                Assert.Equal(textureCornerColor, texture.PixelAt(6+38, 6+38));
                Assert.Equal(textureCornerColor, texture.PixelAt(6, 6+38));
                Assert.Equal(textureMiddleColor, texture.PixelAt(6+19, 6+19));

                //Try to draw the texture with a color overlay
                context.Game.Scenes.Canvas.Clear(Color.DarkSkyBlue);
                context.Game.Scenes.Canvas.DrawImage(tkTexture, new Vec2(5), color: Color.WoodBrown);
                context.Game.Scenes.Canvas.Present();

                textureCornerColor = new Color(97, 77, 54);
                textureMiddleColor = new Color(48, 39, 27);
                texture = context.Game.Scenes.Canvas.Screenshot();
                Assert.NotNull(texture);
                Assert.Equal(textureCornerColor, texture.PixelAt(6, 6));
                Assert.Equal(textureCornerColor, texture.PixelAt(6 + 38, 6));
                Assert.Equal(textureCornerColor, texture.PixelAt(6 + 38, 6 + 38));
                Assert.Equal(textureCornerColor, texture.PixelAt(6, 6 + 38));
                Assert.Equal(textureMiddleColor, texture.PixelAt(6 + 19, 6 + 19));

                context.Exit();
            });
            typeO.Start();
        }

        [Fact]
        public void DrawTextTest()
        {
            var typeO = Utility.DrawTest((context) =>
            {
                //Load font
                var tkFont = context.Game.ContentLoader.LoadContent<TKFont>(Path.Combine("Mock", "Resources", "Lato-Black.ttf"), 32);
                Assert.NotNull(tkFont);

                var size = context.Game.Scenes.Canvas.Viewport.Size;

                //Make sure the screen is clear
                context.Game.Scenes.Canvas.Clear(Color.DarkCyan);
                context.Game.Scenes.Canvas.Present();

                var texture = context.Game.Scenes.Canvas.Screenshot();
                Assert.NotNull(texture);
                Assert.Equal(Color.DarkCyan, texture.PixelAt(0, 0));
                Assert.Equal(Color.DarkCyan, texture.PixelAt((int)size.X - 1, 0));
                Assert.Equal(Color.DarkCyan, texture.PixelAt((int)size.X - 1, (int)size.Y - 1));
                Assert.Equal(Color.DarkCyan, texture.PixelAt(0, (int)size.Y - 1));

                //Try to draw the text
                context.Game.Scenes.Canvas.Clear(Color.DarkCyan);
                context.Game.Scenes.Canvas.PreDraw();
                context.Game.Scenes.Canvas.DrawText("Hello World!", tkFont, new Vec2(0, 0));
                context.Game.Scenes.Canvas.Present();

                texture = context.Game.Scenes.Canvas.Screenshot();
                Assert.NotNull(texture);
                Assert.Equal(Color.White, texture.PixelAt(5, 10));

                context.Exit();
            });
            typeO.Start();
        }
    }
}