using OpenTK.Graphics.OpenGL4;
using TypeOBasic2dTest.TypeTest;
using TypeOEngine.Typedeaf.Basic2d.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Contents.ContentExtensions;
using TypeOEngine.Typedeaf.TK.Contents;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;
using GL = OpenTK.Graphics.OpenGL4.GL;

namespace TypeOBasic2dTest
{
    public class Canvas2dExtendedTest
    {
        private static readonly string TexturePath = Path.Combine("Mock", "Resources", "texture.png");
        private static readonly string FontPath = Path.Combine("Mock", "Resources", "Lato-Black.ttf");

        [Fact]
        public void DrawRectangleA_FillsAreaBetweenTwoPoints()
        {
            var typeO = Utility.DrawTest((context) =>
            {
                var canvas = context.Game.Scenes.Canvas;

                canvas.Clear(Color.DarkCyan);
                canvas.DrawRectangleA(new Vec2(10, 10), new Vec2(40, 30), true, Color.SkyBlue);
                canvas.Present();

                var texture = canvas.Screenshot();
                Assert.Equal(Color.SkyBlue, texture.PixelAt(25, 20));
                Assert.Equal(Color.DarkCyan, texture.PixelAt(50, 50));
                Assert.Equal(Color.DarkCyan, texture.PixelAt(5, 5));

                context.Exit();
            });
            typeO.Start();
        }

        [Fact]
        public void DrawRectangle_WithRectangle_FillsThatRectangle()
        {
            var typeO = Utility.DrawTest((context) =>
            {
                var canvas = context.Game.Scenes.Canvas;

                canvas.Clear(Color.DarkCyan);
                canvas.DrawRectangle(new Rectangle(new Vec2(10, 10), new Vec2(30, 20)), true, Color.SkyBlue);
                canvas.Present();

                var texture = canvas.Screenshot();
                Assert.Equal(Color.SkyBlue, texture.PixelAt(25, 20));
                Assert.Equal(Color.DarkCyan, texture.PixelAt(50, 50));

                context.Exit();
            });
            typeO.Start();
        }

        [Fact]
        public void DrawRectangle_ClipsToEntityBounds()
        {
            var typeO = Utility.DrawTest((context) =>
            {
                var canvas = context.Game.Scenes.Canvas;
                var clip = new Entity2d
                {
                    Position = new Vec2(10, 10),
                    Size = new Vec2(20, 20),
                    SizeMode = SizeMode.Clip
                };
                var viewport = new int[4];
                GL.GetInteger(GetPName.Viewport, viewport);
                var framebuffer = GL.GenFramebuffer();
                var texture = GL.GenTexture();
                var depthRenderbuffer = GL.GenRenderbuffer();
                GL.GetInteger(GetPName.RenderbufferBinding, out var previousRenderbuffer);

                try
                {
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
                    GL.BindTexture(TextureTarget.Texture2D, texture);
                    GL.TexImage2D(
                        TextureTarget.Texture2D,
                        0,
                        PixelInternalFormat.Rgba,
                        64,
                        64,
                        0,
                        PixelFormat.Rgba,
                        PixelType.UnsignedByte,
                        IntPtr.Zero);
                    GL.FramebufferTexture2D(
                        FramebufferTarget.Framebuffer,
                        FramebufferAttachment.ColorAttachment0,
                        TextureTarget.Texture2D,
                        texture,
                        0);
                    GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthRenderbuffer);
                    GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, 64, 64);
                    GL.FramebufferRenderbuffer(
                        FramebufferTarget.Framebuffer,
                        FramebufferAttachment.DepthAttachment,
                        RenderbufferTarget.Renderbuffer,
                        depthRenderbuffer);
                    GL.Viewport(0, 0, 64, 64);
                    canvas.DrawRectangle(Vec2.Zero, new Vec2(40, 40), true, Color.SkyBlue, anchor: clip);
                    GL.GetFramebufferAttachmentParameter(
                        FramebufferTarget.Framebuffer,
                        FramebufferAttachment.StencilAttachment,
                        FramebufferParameterName.FramebufferAttachmentObjectType,
                        out var stencilAttachmentType);
                    Assert.Equal((int)FramebufferAttachmentObjectType.None, stencilAttachmentType);
                    GL.GetFramebufferAttachmentParameter(
                        FramebufferTarget.Framebuffer,
                        FramebufferAttachment.DepthAttachment,
                        FramebufferParameterName.FramebufferAttachmentObjectName,
                        out var restoredDepthRenderbuffer);
                    Assert.Equal(depthRenderbuffer, restoredDepthRenderbuffer);
                    Assert.Equal(FramebufferErrorCode.FramebufferComplete, GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer));
                }
                finally
                {
                    GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, previousRenderbuffer);
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                    GL.Viewport(viewport[0], viewport[1], viewport[2], viewport[3]);
                    GL.DeleteRenderbuffer(depthRenderbuffer);
                    GL.DeleteFramebuffer(framebuffer);
                    GL.DeleteTexture(texture);
                }
                context.Exit();
            });
            typeO.Start();
        }

        [Fact]
        public void DrawImage_WithScale_StretchesTexture()
        {
            var typeO = Utility.DrawTest((context) =>
            {
                var canvas = context.Game.Scenes.Canvas;
                var tkTexture = context.Game.ContentLoader.LoadContent<TKTexture>(TexturePath);

                canvas.Clear(Color.DarkCyan);
                canvas.DrawImage(tkTexture, new Vec2(5), scale: new Vec2(2));
                canvas.Present();

                var texture = canvas.Screenshot();
                var corner = new Color(128, 128, 128);
                Assert.Equal(corner, texture.PixelAt(6, 6));
                Assert.Equal(corner, texture.PixelAt(6 + 76, 6 + 76));
                Assert.Equal(Color.DarkCyan, texture.PixelAt(6 + 90, 6 + 90));

                context.Exit();
            });
            typeO.Start();
        }

        [Fact]
        public void DrawableTexture_Draw_RendersTextureAtItsPosition()
        {
            var typeO = Utility.DrawTest((context) =>
            {
                var canvas = context.Game.Scenes.Canvas;
                var tkTexture = context.Game.ContentLoader.LoadContent<TKTexture>(TexturePath);
                var drawable = new DrawableTexture { Texture = tkTexture, Position = new Vec2(5) };

                canvas.Clear(Color.DarkCyan);
                drawable.Draw(canvas);
                canvas.Present();

                var texture = canvas.Screenshot();
                Assert.Equal(new Color(128, 128, 128), texture.PixelAt(6, 6));
                Assert.Equal(new Color(64, 64, 64), texture.PixelAt(6 + 19, 6 + 19));
                Assert.Equal(Color.DarkCyan, texture.PixelAt(6 + 60, 6 + 60));

                context.Exit();
            });
            typeO.Start();
        }

        [Fact]
        public void DrawableFont_Draw_RendersText()
        {
            var typeO = Utility.DrawTest((context) =>
            {
                var canvas = context.Game.Scenes.Canvas;
                var tkFont = context.Game.ContentLoader.LoadContent<TKFont>(FontPath, 32);
                var drawable = new DrawableFont { Font = tkFont, Text = "Hello World!" };

                canvas.Clear(Color.DarkCyan);
                canvas.PreDraw();
                drawable.Draw(canvas);
                canvas.Present();

                var texture = canvas.Screenshot();
                Assert.Equal(Color.White, texture.PixelAt(5, 15));

                context.Exit();
            });
            typeO.Start();
        }
    }
}
