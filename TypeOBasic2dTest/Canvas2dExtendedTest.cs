using OpenTK.Graphics.OpenGL4;
using TypeOBasic2dTest.TypeTest;
using TypeOEngine.Typedeaf.Basic2d.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities.Interfaces;
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

        private sealed class BatchingScene : Scene
        {
            public override void Update(double dt) { }
            public override void Draw() { }
            protected override void Cleanup() { }
            public override void OnExit(Scene to) { }
            public override void OnEnter(Scene from) { }
        }

        private sealed class RectangleDrawable : IDrawable
        {
            private readonly Vec2 _position;
            private readonly Vec2 _size;
            private readonly Color _color;

            public bool Hidden { get; set; }
            public int DrawOrder { get; set; }

            public RectangleDrawable(Vec2 position, Vec2 size, Color color, int drawOrder)
            {
                _position = position;
                _size = size;
                _color = color;
                DrawOrder = drawOrder;
            }

            public void Draw(ICanvas canvas)
            {
                canvas.DrawRectangle(_position, _size, true, _color);
            }

            public int CompareTo(IDrawable other)
            {
                return DrawOrder.CompareTo(other?.DrawOrder ?? int.MinValue);
            }
        }

        private sealed class CircleDrawable : IDrawable
        {
            public bool Hidden { get; set; }
            public int DrawOrder { get; set; } = 1;

            public void Draw(ICanvas canvas)
            {
                canvas.DrawCircle(new Rectangle(new Vec2(20), new Vec2(30)), true, Color.Blue);
            }

            public int CompareTo(IDrawable other)
            {
                return DrawOrder.CompareTo(other?.DrawOrder ?? int.MinValue);
            }
        }

        private sealed class PolylineDrawable : IDrawable
        {
            private readonly List<Vec2> _points;
            private readonly Color _color;

            public bool Hidden { get; set; }
            public int DrawOrder { get; set; }

            public PolylineDrawable(Vec2 topLeft, Vec2 size, Color color, int drawOrder)
            {
                var topRight = topLeft + new Vec2(size.X, 0);
                var bottomRight = topLeft + size;
                var bottomLeft = topLeft + new Vec2(0, size.Y);
                _points = new List<Vec2> { topLeft, topRight, bottomRight, bottomLeft, topLeft };
                _color = color;
                DrawOrder = drawOrder;
            }

            public void Draw(ICanvas canvas)
            {
                canvas.DrawLines(_points, _color);
            }

            public int CompareTo(IDrawable other)
            {
                return DrawOrder.CompareTo(other?.DrawOrder ?? int.MinValue);
            }
        }

        [Fact]
        public void DrawStackBatchesAdjacentFilledRectanglesAndFlushesBeforeOtherCanvasOperations()
        {
            var typeO = Utility.DrawTest(context =>
            {
                var canvas = context.Game.Scenes.Canvas;
                var scene = context.Game.Scenes.SetScene<BatchingScene>();
                scene.DrawStack.Push(new RectangleDrawable(new Vec2(10), new Vec2(30), Color.Red, 0));
                scene.DrawStack.Push(new CircleDrawable());
                scene.DrawStack.Push(new RectangleDrawable(new Vec2(30), new Vec2(20), Color.Green, 2));

                canvas.Clear(Color.Black);
                scene.DrawStack.Draw(canvas);
                scene.DrawStack.Draw(canvas);
                canvas.Present();

                var texture = canvas.Screenshot();
                Assert.Equal(Color.Red, texture.PixelAt(15, 15));
                Assert.Equal(Color.Blue, texture.PixelAt(25, 25));
                Assert.Equal(Color.Green, texture.PixelAt(35, 35));
                Assert.Equal(Color.Black, texture.PixelAt(60, 35));

                context.Exit();
            });

            typeO.Start();
        }

        [Fact]
        public void DrawStackBatchesTwentyThousandEditorStyleOutlines()
        {
            const int lineCount = 20_000;
            var typeO = Utility.DrawTest(context =>
            {
                var canvas = context.Game.Scenes.Canvas;
                var scene = context.Game.Scenes.SetScene<BatchingScene>();
                for(var i = 0; i < lineCount; i++)
                {
                    var topLeft = new Vec2(2 + (i % 200) * 3, 2 + (i / 200) * 3);
                    var color = i == lineCount - 1 ? Color.Red : Color.SkyBlue;
                    if (i == lineCount - 1)
                        topLeft = new Vec2(500, 100);
                    scene.DrawStack.Push(new PolylineDrawable(topLeft, new Vec2(2), color, i));
                }

                canvas.Clear(Color.Black);
                scene.DrawStack.Draw(canvas);
                scene.DrawStack.Draw(canvas);
                canvas.Present();

                var texture = canvas.Screenshot();
                Assert.Equal(Color.SkyBlue, texture.PixelAt(3, 2));
                Assert.Equal(Color.Red, texture.PixelAt(501, 100));
                Assert.Equal(Color.Black, texture.PixelAt(0, 0));
                Assert.Equal(Color.Black, texture.PixelAt(639, 319));

                context.Exit();
            });

            typeO.Start();
        }

        [Fact]
        public void DrawStackRendersTwentyThousandRectanglesInLargeBatches()
        {
            const int rectangleCount = 20_000;
            var typeO = Utility.DrawTest(context =>
            {
                var canvas = context.Game.Scenes.Canvas;
                var scene = context.Game.Scenes.SetScene<BatchingScene>();
                for(var i = 0; i < rectangleCount; i++)
                {
                    var position = new Vec2(2 + (i % 200) * 3, 2 + (i / 200) * 3);
                    scene.DrawStack.Push(new RectangleDrawable(position, new Vec2(2), Color.SkyBlue, i));
                }

                canvas.Clear(Color.Black);
                scene.DrawStack.Draw(canvas);
                scene.DrawStack.Draw(canvas);
                canvas.Present();

                var texture = canvas.Screenshot();
                Assert.Equal(Color.SkyBlue, texture.PixelAt(3, 3));
                Assert.Equal(Color.SkyBlue, texture.PixelAt(600, 300));
                Assert.Equal(Color.Black, texture.PixelAt(0, 0));
                Assert.Equal(Color.Black, texture.PixelAt(639, 319));

                context.Exit();
            });

            typeO.Start();
        }

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
