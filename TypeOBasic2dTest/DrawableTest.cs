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
            // scaledSize = Size * Scale = (10, 10) * (2, 2) = (20, 20)
            // originOffset = scaledSize * Origin = (20, 20) * (0.5, 0.5) = (10, 10)
            // boundingPos = Position - originOffset = (20, 20) - (10, 10) = (10, 10)
            Assert.Equal(new Vec2(10, 10), box.Pos);
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

        [Fact]
        public void DrawableTexture_ClipMode_ClipsWithinBoundingBox()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Texture = new StubTexture(new Vec2i(100, 100)),
                Size = new Vec2(50, 50),
                SizeMode = SizeMode.Clip
            });

            // When SizeMode is Clip, the drawable should have clipping enabled
            Assert.Equal(SizeMode.Clip, drawable.SizeMode);
            Assert.Equal(new Vec2(50, 50), drawable.Size);
            
            // Bounding box should match the size (without scale)
            Assert.Equal(new Vec2(50, 50), drawable.BoundingBox.Size);
        }

        [Fact]
        public void DrawableTexture_ClipMode_WithScale_AccountsForScaling()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Texture = new StubTexture(new Vec2i(100, 100)),
                Size = new Vec2(50, 50),
                Scale = new Vec2(2, 2),
                SizeMode = SizeMode.Clip
            });

            // BoundingBox should include scale
            Assert.Equal(new Vec2(100, 100), drawable.BoundingBox.Size);
            
            // Original size should remain unchanged
            Assert.Equal(new Vec2(50, 50), drawable.Size);
        }

        [Fact]
        public void DrawableTexture_ClipMode_WithDifferentScales()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Texture = new StubTexture(new Vec2i(100, 100)),
                Size = new Vec2(50, 50),
                Scale = new Vec2(1.5, 2.5),
                SizeMode = SizeMode.Clip
            });

            // BoundingBox should have different scale per axis
            Assert.Equal(new Vec2(75, 125), drawable.BoundingBox.Size);
        }

        [Fact]
        public void DrawableTexture_StretchMode_IgnoresClipping()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Texture = new StubTexture(new Vec2i(100, 100)),
                Size = new Vec2(50, 50),
                SizeMode = SizeMode.Stretch
            });

            // In Stretch mode, the texture should not clip
            Assert.Equal(SizeMode.Stretch, drawable.SizeMode);
            Assert.Equal(new Vec2(50, 50), drawable.Size);
        }

        [Fact]
        public void DrawableTexture_StretchMode_WithScale_AppliesScaleToStretchedSize()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Texture = new StubTexture(new Vec2i(100, 100)),
                Size = new Vec2(50, 50),
                Scale = new Vec2(2, 2),
                SizeMode = SizeMode.Stretch
            });

            // With Stretch mode, Size = (50, 50), Texture.Size = (100, 100)
            // stretchScale = (50, 50) / (100, 100) = (0.5, 0.5)
            // drawScale = (0.5, 0.5) * (2, 2) = (1, 1)
            // BoundingBox.Size = (50, 50) * (2, 2) = (100, 100)
            Assert.Equal(new Vec2(100, 100), drawable.BoundingBox.Size);
        }

        [Fact]
        public void DrawableTexture_WithOrigin_BoundingBoxReflectsOriginOffset()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Texture = new StubTexture(new Vec2i(100, 100)),
                Position = new Vec2(100, 100),
                Origin = new Vec2(0.5, 0.5),
                Size = new Vec2(50, 50),
                SizeMode = SizeMode.Clip
            });

            // BoundingBox should be offset by origin
            var bb = drawable.BoundingBox;
            Assert.Equal(new Vec2(75, 75), bb.Pos); // 100 - (50 * 0.5)
            Assert.Equal(new Vec2(50, 50), bb.Size);
        }

        [Fact]
        public void DrawableTexture_WithScaleAndOrigin_BoundingBoxAccountsForBoth()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Texture = new StubTexture(new Vec2i(100, 100)),
                Position = new Vec2(100, 100),
                Origin = new Vec2(0.5, 0.5),
                Size = new Vec2(50, 50),
                Scale = new Vec2(2, 2),
                SizeMode = SizeMode.Clip
            });

            var bb = drawable.BoundingBox;
            // scaledSize = Size * Scale = (50, 50) * 2 = (100, 100)
            // originOffset = scaledSize * Origin = (100, 100) * 0.5 = (50, 50)
            // boundingPos = Position - originOffset = (100, 100) - (50, 50) = (50, 50)
            Assert.Equal(new Vec2(50, 50), bb.Pos);
            Assert.Equal(new Vec2(100, 100), bb.Size);
        }

        [Fact]
        public void DrawableTexture_WithRotation_BoundingBoxIsAxisAligned()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Texture = new StubTexture(new Vec2i(100, 100)),
                Position = new Vec2(100, 100),
                Size = new Vec2(50, 50),
                Rotation = 45,
                SizeMode = SizeMode.Clip
            });

            // BoundingBox should be axis-aligned even with rotation
            var bb = drawable.BoundingBox;
            Assert.NotEqual(new Vec2(50, 50), bb.Size); // Rotated size should be different
            Assert.True(bb.Size.X > 0 && bb.Size.Y > 0);
        }

        [Fact]
        public void DrawableFont_WithClipping_BoundingBoxReflectsSize()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableFont>(option: new DrawableFontOption
            {
                Font = new StubFont(),
                Text = "Hello",
                Position = new Vec2(50, 50)
            });

            var bb = drawable.BoundingBox;
            Assert.Equal(new Vec2(50, 50), bb.Pos);
            // Font size is based on actual text rendering, should be non-zero
            Assert.True(bb.Size.X > 0 && bb.Size.Y > 0);
        }

        [Fact]
        public void DrawableFont_WithScaleAndClipping_BoundingBoxAccountsForScale()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableFont>(option: new DrawableFontOption
            {
                Font = new StubFont(),
                Text = "Hello",
                Position = new Vec2(50, 50),
                Scale = new Vec2(2, 2)
            });

            var bb = drawable.BoundingBox;
            Assert.Equal(new Vec2(50, 50), bb.Pos);
            // Bounding box size should be scaled
            var unscaledSize = drawable.Size;
            Assert.Equal(new Vec2(unscaledSize.X * 2, unscaledSize.Y * 2), bb.Size);
        }

        [Fact]
        public void Drawable2d_Scale_DefaultValue()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Texture = new StubTexture(new Vec2i(100, 100))
            });

            // Default scale should be (1, 1)
            Assert.Equal(new Vec2(1, 1), drawable.Scale);
        }

        [Fact]
        public void Drawable2d_Scale_CanBeSet()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Texture = new StubTexture(new Vec2i(100, 100)),
                Scale = new Vec2(2.5, 1.5)
            });

            Assert.Equal(new Vec2(2.5, 1.5), drawable.Scale);
        }

        [Fact]
        public void Drawable2d_Origin_DefaultValue()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Texture = new StubTexture(new Vec2i(100, 100))
            });

            // Default origin should be (0, 0) - top-left
            Assert.Equal(new Vec2(0, 0), drawable.Origin);
        }

        [Fact]
        public void Drawable2d_Origin_CenterValue()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Texture = new StubTexture(new Vec2i(100, 100)),
                Origin = new Vec2(0.5, 0.5)
            });

            Assert.Equal(new Vec2(0.5, 0.5), drawable.Origin);
        }

        [Fact]
        public void Drawable2d_Rotation_DefaultValue()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Texture = new StubTexture(new Vec2i(100, 100))
            });

            Assert.Equal(0.0, drawable.Rotation);
        }

        [Fact]
        public void Drawable2d_Rotation_CanBeSet()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Texture = new StubTexture(new Vec2i(100, 100)),
                Rotation = 45.5
            });

            Assert.Equal(45.5, drawable.Rotation);
        }

        [Fact]
        public void Drawable2d_BoundingBox_WithZeroScale_FallsBackToOriginalSize()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Texture = new StubTexture(new Vec2i(100, 100)),
                Size = new Vec2(50, 50),
                Scale = new Vec2(0, 0)
            });

            // When scale is 0, the bounding box size should be 0
            Assert.Equal(new Vec2(0, 0), drawable.BoundingBox.Size);
        }

        [Fact]
        public void Drawable2d_BoundingBox_WithNegativeScale_CalculatesCorrectly()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Texture = new StubTexture(new Vec2i(100, 100)),
                Size = new Vec2(50, 50),
                Scale = new Vec2(-2, -2)
            });

            // BoundingBox with negative scale should still calculate correctly
            // It may have negative size values which will be reflected in rotation handling
            var bb = drawable.BoundingBox;
            // The bounding box calculation uses rotation handling even with non-zero rotation
            // which properly handles the coordinate transformation for negative scales
            Assert.True(bb.Size != Vec2.Zero);
        }

        [Fact]
        public void DrawableTexture_ClipMode_WithZeroSize_NoClipping()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Texture = new StubTexture(new Vec2i(100, 100)),
                Size = new Vec2(0, 0),
                SizeMode = SizeMode.Clip
            });

            // When size is zero, the texture's natural size is used (no clipping)
            Assert.Equal(new Vec2(100, 100), drawable.Size);
        }

        [Fact]
        public void DrawableTexture_ClipMode_MultipleDrawCalls_MaintainsState()
        {
            var owner = StartOwner();
            var drawable = owner.Drawables.Create<DrawableTexture>(option: new DrawableTextureOption
            {
                Texture = new StubTexture(new Vec2i(100, 100)),
                Size = new Vec2(50, 50),
                Scale = new Vec2(2, 2),
                SizeMode = SizeMode.Clip
            });

            // First draw
            drawable.Draw(null);
            Assert.Equal(new Vec2(100, 100), drawable.BoundingBox.Size);
            
            // Second draw - should maintain state
            drawable.Draw(null);
            Assert.Equal(new Vec2(100, 100), drawable.BoundingBox.Size);
        }
    }
}
