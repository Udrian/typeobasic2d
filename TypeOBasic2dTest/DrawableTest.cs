using TypeOBasic2dTest.TypeTest.Mock;
using TypeOEngine.Typedeaf.Core;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Core.Entities;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;

namespace TypeOBasic2dTest
{
    public class DrawableTest
    {
        public class Owner : Entity
        {
            protected override void Initialize() => base.Initialize();
            protected override void Cleanup() => base.Cleanup();
        }

        public class OwnerScene : Scene
        {
            protected override void Initialize() => base.Initialize();
            protected override void Cleanup() { }
            public override void Update(double dt) { }
            public override void Draw() { }
            public override void OnExit(Scene to) { }
            public override void OnEnter(Scene from) { }
        }

        public class OwnerGame : Game
        {
            public OwnerScene Scene { get; private set; }

            protected override void Initialize()
            {
                base.Initialize();
                Scene = Scenes.SetScene<OwnerScene>();
            }

            protected override void Cleanup() { }
            public override void Update(double dt) => Exit();
            public override void Draw() { }
        }

        private static Owner StartOwner()
        {
            var typeO = Assert.IsType<TypeO>(TypeO.Create<OwnerGame>("drawable-test"));
            typeO.Start();
            return ((OwnerGame)typeO.Context.Game).Scene.Entities.Create<Owner>(pushToUpdateLoop: false, pushToDrawStack: false);
        }

        [Fact]
        public void DrawableTexture_HasNeutralDefaults()
        {
            var drawable = new DrawableTexture();

            Assert.Equal(Vec2.Zero, drawable.Position);
            Assert.Equal(Vec2.One, drawable.Scale);
            Assert.Equal(0, drawable.Rotation);
            Assert.Equal(Color.White, drawable.Color);
            Assert.Equal(Flipped.None, drawable.Flipped);
            Assert.Null(drawable.Texture);
        }

        [Fact]
        public void DrawableTexture_Size_FollowsTexture_WhenNotSet()
        {
            var drawable = new DrawableTexture();
            Assert.Equal(Vec2.Zero, drawable.Size);

            drawable.Texture = new StubTexture(new Vec2i(16, 32));
            Assert.Equal(new Vec2(16, 32), drawable.Size);
        }

        [Fact]
        public void DrawableTexture_Size_CanBeSetExplicitly()
        {
            var drawable = new DrawableTexture();
            drawable.Texture = new StubTexture(new Vec2i(16, 32));

            drawable.Size = new Vec2(8, 16);
            Assert.Equal(new Vec2(8, 16), drawable.Size);
        }

        [Fact]
        public void DrawableTexture_Origin_DefaultIsZero()
        {
            var drawable = new DrawableTexture();
            Assert.Equal(Vec2.Zero, drawable.Origin);
        }

        [Fact]
        public void DrawableTexture_BoundingBox_IncludesOriginAndScale()
        {
            var drawable = new DrawableTexture
            {
                Texture = new StubTexture(new Vec2i(10, 10)),
                Position = new Vec2(20, 20),
                Origin = new Vec2(0.5, 0.5),
                Scale = new Vec2(2, 2)
            };

            var box = drawable.BoundingBox;
            Assert.Equal(new Vec2(15, 15), box.Pos);
            Assert.Equal(new Vec2(20, 20), box.Size);
        }

        [Fact]
        public void DrawableTexture_Draw_WithoutTexture_DoesNothing()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableTexture>();

            drawable.Draw(null);
        }

        [Fact]
        public void DrawableTextureOption_AppliesPositionOriginAndTexture()
        {
            var owner = StartOwner();
            var texture = new StubTexture(new Vec2i(4, 4));

            var drawable = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Position = new Vec2(9, 8),
                Origin = new Vec2(0.5, 0.5),
                Texture = texture
            });

            Assert.Equal(new Vec2(9, 8), drawable.Position);
            Assert.Equal(new Vec2(0.5, 0.5), drawable.Origin);
            Assert.Same(texture, drawable.Texture);
        }

        [Fact]
        public void DrawableTextureOption_AppliesSizeAndSizeMode()
        {
            var owner = StartOwner();
            var texture = new StubTexture(new Vec2i(4, 4));

            var drawable = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Texture = texture,
                Size = new Vec2(8, 8),
                SizeMode = SizeMode.Stretch
            });

            Assert.Same(texture, drawable.Texture);
            Assert.Equal(new Vec2(8, 8), drawable.Size);
            Assert.Equal(SizeMode.Stretch, drawable.SizeMode);
        }

        [Fact]
        public void DrawableTexture_HasDefaultSizeModeClip()
        {
            var drawable = new DrawableTexture();
            Assert.Equal(SizeMode.Clip, drawable.SizeMode);
        }

        [Fact]
        public void DrawableFont_HasNeutralDefaults()
        {
            var drawable = new DrawableFont();

            Assert.Equal(Vec2.One, drawable.Scale);
            Assert.Equal(0, drawable.Rotation);
            Assert.Equal(Color.White, drawable.Color);
            Assert.Equal(Flipped.None, drawable.Flipped);
            Assert.Empty(drawable.Lines);
            Assert.Equal(Vec2.Zero, drawable.Size);
        }

        [Fact]
        public void DrawableFont_SingleLine_MeasuresWholeText()
        {
            var drawable = new DrawableFont { Font = new StubFont(), Text = "Hello" };

            Assert.Equal(new[] { "Hello" }, drawable.Lines);
            Assert.Equal(new Vec2(50, 20), drawable.Size);
        }

        [Fact]
        public void DrawableFont_MultiLine_SplitsOnNewlineAndUsesWidestLine()
        {
            var drawable = new DrawableFont { Font = new StubFont(), Text = "ab\ncdef\ngh" };

            Assert.Equal(new[] { "ab", "cdef", "gh" }, drawable.Lines);
            Assert.Equal(new Vec2(40, 60), drawable.Size);
        }

        [Fact]
        public void DrawableFont_ChangingTextOrFont_RecomputesLines()
        {
            var drawable = new DrawableFont { Font = new StubFont(), Text = "abc" };

            drawable.Text = "x\ny";
            Assert.Equal(new[] { "x", "y" }, drawable.Lines);
            Assert.Equal(new Vec2(10, 40), drawable.Size);

            drawable.Text = null;
            Assert.Empty(drawable.Lines);
            Assert.Equal(Vec2.Zero, drawable.Size);
        }

        [Fact]
        public void DrawableFont_WithoutFont_HasNoLines()
        {
            var drawable = new DrawableFont { Text = "abc" };

            Assert.Empty(drawable.Lines);
            Assert.Equal(Vec2.Zero, drawable.Size);
        }

        [Fact]
        public void DrawableFont_Draw_WithoutFontOrText_DoesNothing()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableFont>();

            drawable.Draw(null);
        }

        [Fact]
        public void DrawableFontOption_AppliesAllValues()
        {
            var owner = StartOwner();
            var font = new StubFont();

            var drawable = owner.Drawables.Create<DrawableFont>(option: new DrawableFontOption
            {
                Position = new Vec2(1, 2),
                Font = font,
                Text = "hi",
                Scale = new Vec2(2, 2),
                Rotation = 30,
                Color = Color.Orange,
                Flipped = Flipped.Both
            });

            Assert.Equal(new Vec2(1, 2), drawable.Position);
            Assert.Same(font, drawable.Font);
            Assert.Equal("hi", drawable.Text);
            Assert.Equal(new Vec2(2, 2), drawable.Scale);
            Assert.Equal(30, drawable.Rotation);
            Assert.Equal(Color.Orange, drawable.Color);
            Assert.Equal(Flipped.Both, drawable.Flipped);
            Assert.Equal(new Vec2(20, 20), drawable.Size);
        }

        [Fact]
        public void DrawableFontOption_WithoutValues_KeepsDefaults()
        {
            var owner = StartOwner();

            var drawable = owner.Drawables.Create<DrawableFont>(option: new DrawableFontOption());

            Assert.Equal(Vec2.One, drawable.Scale);
            Assert.Equal(Color.White, drawable.Color);
            Assert.Equal(Flipped.None, drawable.Flipped);
            Assert.Null(drawable.Text);
        }

        [Fact]
        public void DrawableTexture_Flipped_CanBeSetToHorizontal()
        {
            var drawable = new DrawableTexture();
            
            drawable.Flipped = Flipped.Horizontal;
            
            Assert.Equal(Flipped.Horizontal, drawable.Flipped);
        }

        [Fact]
        public void DrawableTexture_Flipped_CanBeSetToVertical()
        {
            var drawable = new DrawableTexture();
            
            drawable.Flipped = Flipped.Vertical;
            
            Assert.Equal(Flipped.Vertical, drawable.Flipped);
        }

        [Fact]
        public void DrawableTexture_Flipped_CanBeSetToBoth()
        {
            var drawable = new DrawableTexture();
            
            drawable.Flipped = Flipped.Both;
            
            Assert.Equal(Flipped.Both, drawable.Flipped);
        }

        [Fact]
        public void DrawableTexture_Draw_WithFlippedTexture_Works()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Texture = new StubTexture(new Vec2i(16, 16)),
                Flipped = Flipped.Horizontal
            });

            drawable.Draw(null);
            
            Assert.Equal(Flipped.Horizontal, drawable.Flipped);
        }

        [Fact]
        public void DrawableTexture_Draw_WithBothFlips_Works()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Texture = new StubTexture(new Vec2i(16, 16)),
                Flipped = Flipped.Both
            });

            drawable.Draw(null);
            
            Assert.Equal(Flipped.Both, drawable.Flipped);
        }

        [Fact]
        public void DrawableTextureOption_AppliesFlipped()
        {
            var owner = StartOwner();
            var texture = new StubTexture(new Vec2i(4, 4));

            var drawableH = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Texture = texture,
                Flipped = Flipped.Horizontal
            });

            var drawableV = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Texture = texture,
                Flipped = Flipped.Vertical
            });

            var drawableB = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Texture = texture,
                Flipped = Flipped.Both
            });

            Assert.Equal(Flipped.Horizontal, drawableH.Flipped);
            Assert.Equal(Flipped.Vertical, drawableV.Flipped);
            Assert.Equal(Flipped.Both, drawableB.Flipped);
        }

        [Fact]
        public void DrawableFont_Flipped_CanBeSetToHorizontal()
        {
            var drawable = new DrawableFont();
            
            drawable.Flipped = Flipped.Horizontal;
            
            Assert.Equal(Flipped.Horizontal, drawable.Flipped);
        }

        [Fact]
        public void DrawableFont_Flipped_CanBeSetToVertical()
        {
            var drawable = new DrawableFont();
            
            drawable.Flipped = Flipped.Vertical;
            
            Assert.Equal(Flipped.Vertical, drawable.Flipped);
        }

        [Fact]
        public void DrawableFont_Flipped_CanBeSetToBoth()
        {
            var drawable = new DrawableFont();
            
            drawable.Flipped = Flipped.Both;
            
            Assert.Equal(Flipped.Both, drawable.Flipped);
        }

        [Fact]
        public void DrawableFont_Draw_WithFlippedText_Works()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableFont>(option: new DrawableFontOption
            {
                Font = new StubFont(),
                Text = "Hello",
                Flipped = Flipped.Horizontal
            });

            drawable.Draw(null);
            
            Assert.Equal(Flipped.Horizontal, drawable.Flipped);
        }

        [Fact]
        public void DrawableFont_Draw_WithBothFlips_Works()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableFont>(option: new DrawableFontOption
            {
                Font = new StubFont(),
                Text = "Hello",
                Flipped = Flipped.Both
            });

            drawable.Draw(null);
            
            Assert.Equal(Flipped.Both, drawable.Flipped);
        }

        [Fact]
        public void DrawableFontOption_AppliesFlipped()
        {
            var owner = StartOwner();
            var font = new StubFont();

            var drawableH = owner.Drawables.Create<DrawableFont>(option: new DrawableFontOption
            {
                Font = font,
                Text = "Test",
                Flipped = Flipped.Horizontal
            });

            var drawableV = owner.Drawables.Create<DrawableFont>(option: new DrawableFontOption
            {
                Font = font,
                Text = "Test",
                Flipped = Flipped.Vertical
            });

            var drawableB = owner.Drawables.Create<DrawableFont>(option: new DrawableFontOption
            {
                Font = font,
                Text = "Test",
                Flipped = Flipped.Both
            });

            Assert.Equal(Flipped.Horizontal, drawableH.Flipped);
            Assert.Equal(Flipped.Vertical, drawableV.Flipped);
            Assert.Equal(Flipped.Both, drawableB.Flipped);
        }

        [Fact]
        public void DrawableTexture_Flipped_PreservesAcrossMultipleDraw_Calls()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Texture = new StubTexture(new Vec2i(16, 16)),
                Flipped = Flipped.Vertical
            });

            drawable.Draw(null);
            Assert.Equal(Flipped.Vertical, drawable.Flipped);
            
            drawable.Draw(null);
            Assert.Equal(Flipped.Vertical, drawable.Flipped);
        }

        [Fact]
        public void DrawableFont_Flipped_PreservesAcrossMultipleDraw_Calls()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableFont>(option: new DrawableFontOption
            {
                Font = new StubFont(),
                Text = "Hello",
                Flipped = Flipped.Horizontal
            });

            drawable.Draw(null);
            Assert.Equal(Flipped.Horizontal, drawable.Flipped);
            
            drawable.Draw(null);
            Assert.Equal(Flipped.Horizontal, drawable.Flipped);
        }
    }
}
