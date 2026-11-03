using TypeOEngine.Typedeaf.Core;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Basic2d.Engine;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Interfaces;

namespace TypeOBasic2dTest
{
    public class Entity2dTest
    {
        public class Box : Entity2d
        {
            public Box Child { get; set; }
            public Box CreateChild() => Child = Entities.Create<Box>();

            protected override void Initialize() => base.Initialize();
            protected override void Cleanup() => base.Cleanup();
        }

        public class BoxScene : Scene
        {
            protected override void Initialize() => base.Initialize();
            protected override void Cleanup() { }
            public override void Update(double dt) { }
            public override void Draw() { }
            public override void OnExit(Scene to) { }
            public override void OnEnter(Scene from) { }
        }

        public class BoxGame : Game
        {
            public BoxScene Scene { get; private set; }

            protected override void Initialize()
            {
                base.Initialize();
                Scene = Scenes.SetScene<BoxScene>();
            }

            protected override void Cleanup() { }
            public override void Update(double dt) => Exit();
            public override void Draw() { }
        }

        private static BoxScene StartScene()
        {
            var typeO = Assert.IsType<TypeO>(TypeO.Create<BoxGame>("entity2d-test"));
            typeO.Start();
            return ((BoxGame)typeO.Context.Game).Scene;
        }

        [Fact]
        public void NewEntity_HasNeutralTransform()
        {
            var box = StartScene().Entities.Create<Box>();

            Assert.Equal(Vec2.Zero, box.Position);
            Assert.Equal(Vec2.One, box.Scale);
            Assert.Equal(0, box.Rotation);
            Assert.Equal(Vec2.Zero, box.Size);
            Assert.Equal(Vec2.Zero, box.Origin);
        }

        [Fact]
        public void ScreenBounds_WithoutParent_UsesOwnPositionAndSize()
        {
            var box = StartScene().Entities.Create<Box>();
            box.Position = new Vec2(10, 20);
            box.Size = new Vec2(30, 40);

            Assert.Equal(new Rectangle(new Vec2(10, 20), new Vec2(30, 40)), box.ScreenBounds);
        }

        [Fact]
        public void ScreenBounds_WithParent_IsOffsetByParentScreenPosition()
        {
            var parent = StartScene().Entities.Create<Box>();
            parent.Position = new Vec2(100, 200);
            var child = parent.CreateChild();
            child.Position = new Vec2(5, 6);
            child.Size = new Vec2(7, 8);

            Assert.Same(parent, child.Parent);
            Assert.Equal(new Rectangle(new Vec2(105, 206), new Vec2(7, 8)), child.ScreenBounds);
        }

        [Fact]
        public void ScreenBounds_Setter_StoresPositionRelativeToParent()
        {
            var parent = StartScene().Entities.Create<Box>();
            parent.Position = new Vec2(100, 200);
            var child = parent.CreateChild();

            child.ScreenBounds = new Rectangle(new Vec2(150, 250), new Vec2(10, 20));

            Assert.Equal(new Vec2(50, 50), child.Position);
            Assert.Equal(new Vec2(10, 20), child.Size);
        }

        [Fact]
        public void CreateAnchor_DefaultsToAbsoluteUpperLeft()
        {
            var box = StartScene().Entities.Create<Box>();

            var anchor = box.CreateAnchor(new Vec2(3, 4));

            Assert.Same(box, anchor.Parent);
            Assert.Equal(new Vec2(3, 4), anchor.Position);
            Assert.Equal(Orientation2d.UpperLeft, anchor.Orientation);
            Assert.Equal(OrientationType.Absolute, anchor.OrientationType);
        }

        [Fact]
        public void CreateAnchor_KeepsRequestedOrientation()
        {
            var box = StartScene().Entities.Create<Box>();

            var anchor = box.CreateAnchor(new Vec2(1, 2), Orientation2d.LowerRight, OrientationType.Fraction);

            Assert.Equal(Orientation2d.LowerRight, anchor.Orientation);
            Assert.Equal(OrientationType.Fraction, anchor.OrientationType);
        }

        [Theory]
        [InlineData(Orientation2d.UpperLeft, 110, 220)]
        [InlineData(Orientation2d.UpperRight, 190, 220)]
        [InlineData(Orientation2d.LowerLeft, 110, 280)]
        [InlineData(Orientation2d.LowerRight, 190, 280)]
        public void Anchor_Absolute_ScreenBoundsFollowOrientation(Orientation2d orientation, double x, double y)
        {
            var box = StartScene().Entities.Create<Box>();
            box.Position = new Vec2(100, 200);
            box.Size = new Vec2(100, 100);

            var anchor = box.CreateAnchor(new Vec2(10, 20), orientation, OrientationType.Absolute);

            Assert.Equal(new Rectangle(new Vec2(x, y), Vec2.Zero), anchor.ScreenBounds);
        }

        [Theory]
        [InlineData(Orientation2d.UpperLeft, 125, 250)]
        [InlineData(Orientation2d.UpperRight, 175, 250)]
        [InlineData(Orientation2d.LowerLeft, 125, 250)]
        [InlineData(Orientation2d.LowerRight, 175, 250)]
        public void Anchor_Fraction_ScalesWithParentSize(Orientation2d orientation, double x, double y)
        {
            var box = StartScene().Entities.Create<Box>();
            box.Position = new Vec2(100, 200);
            box.Size = new Vec2(100, 100);

            var anchor = box.CreateAnchor(new Vec2(0.25, 0.5), orientation, OrientationType.Fraction);

            Assert.Equal(new Rectangle(new Vec2(x, y), Vec2.Zero), anchor.ScreenBounds);
        }

        [Fact]
        public void Anchor_ScreenBoundsSetter_StoresPositionRelativeToParent()
        {
            var box = StartScene().Entities.Create<Box>();
            box.Position = new Vec2(100, 200);
            var anchor = box.CreateAnchor(Vec2.Zero);

            anchor.ScreenBounds = new Rectangle(new Vec2(130, 240), Vec2.Zero);

            Assert.Equal(new Vec2(30, 40), anchor.Position);
        }

        [Fact]
        public void Entity_IsAnAnchorForItself()
        {
            var box = StartScene().Entities.Create<Box>();

            Assert.IsAssignableFrom<IAnchor2d>(box);
        }

        [Fact]
        public void EntityListCreate_AppliesTransformArguments()
        {
            var box = StartScene().Entities.Create<Box>(
                new Vec2(1, 2), scale: new Vec2(3, 4), rotation: 45, origin: new Vec2(5, 6), id: "custom");

            Assert.Equal(new Vec2(1, 2), box.Position);
            Assert.Equal(new Vec2(3, 4), box.Scale);
            Assert.Equal(45, box.Rotation);
            Assert.Equal(new Vec2(5, 6), box.Origin);
            Assert.Equal("custom", box.ID);
        }

        [Fact]
        public void EntityListCreate_KeepsDefaultsWhenArgumentsAreOmitted()
        {
            var box = StartScene().Entities.Create<Box>((Vec2?)null);

            Assert.Equal(Vec2.Zero, box.Position);
            Assert.Equal(Vec2.One, box.Scale);
            Assert.Equal(Vec2.Zero, box.Origin);
        }

        [Fact]
        public void EntityListCreate_ByType_ReturnsEntityOfThatType()
        {
            var entity = StartScene().Entities.Create(typeof(Box), new Vec2(7, 8), rotation: 90);

            var box = Assert.IsType<Box>(entity);
            Assert.Equal(new Vec2(7, 8), box.Position);
            Assert.Equal(90, box.Rotation);
        }
    }
}
