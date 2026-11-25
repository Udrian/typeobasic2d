using System.Reflection;
using TypeOEngine.Typedeaf.Core;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Basic2d.Engine;
using TypeOEngine.Typedeaf.Basic2d.Engine.Graphics;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Interfaces;
using TypeDBasic2d.Typedeaf.Core.Entities.Drawables;

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

        public class BoxDrawable : Drawable2d
        {
            public override void Draw(ICanvas canvas) { }
            protected override void Initialize() { }
            protected override void Cleanup() { }
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

        private static void SetSize(Entity2d entity, Vec2 size)
        {
            entity.SizeMode = SizeMode.None;
            entity.Size = size;
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
            Assert.Equal(SizeMode.Auto, box.SizeMode);
        }

        [Fact]
        public void AutoSize_UsesChildBoundsAndUpdatesAfterChildrenChange()
        {
            var parent = StartScene().Entities.Create<Box>();
            var first = parent.Drawables.Create<BoxDrawable>();
            first.Position = new Vec2(10, 20);
            first.Size = new Vec2(30, 40);

            Assert.Equal(new Vec2(30, 40), parent.Size);
            Assert.Equal(new Rectangle(new Vec2(10, 20), new Vec2(30, 40)), parent.ScreenBounds);

            var second = parent.Drawables.Create<BoxDrawable>();
            second.Position = new Vec2(-5, 8);
            second.Size = new Vec2(12, 50);

            Assert.Equal(new Vec2(45, 52), parent.Size);
            second.Size = new Vec2(12, 60);
            Assert.Equal(new Vec2(45, 60), parent.Size);
        }

        [Fact]
        public void AutoSize_IncludesNestedEntityBounds()
        {
            var parent = StartScene().Entities.Create<Box>();
            var child = parent.CreateChild();
            child.Position = new Vec2(10, 15);
            var drawable = child.Drawables.Create<BoxDrawable>();
            drawable.Position = new Vec2(2, 3);
            drawable.Size = new Vec2(20, 30);
            parent.Entities.Update(0);

            Assert.Single(parent.Entities.ListAll());
            Assert.Equal(20, parent.Size.X, 5);
            Assert.Equal(30, parent.Size.Y, 5);
        }

        [Fact]
        public void StretchMode_FitsChildBoundsToEntitySize()
        {
            var parent = StartScene().Entities.Create<Box>();
            parent.SizeMode = SizeMode.Stretch;
            parent.Size = new Vec2(100, 50);
            var drawable = parent.Drawables.Create<BoxDrawable>();
            drawable.Position = new Vec2(10, 20);
            drawable.Size = new Vec2(20, 10);

            Assert.Equal(new Rectangle(Vec2.Zero, new Vec2(100, 50)), drawable.WorldBounds);
        }

        [Fact]
        public void StretchMode_ConvertsWorldDeltaToStretchedChildSpace()
        {
            var parent = StartScene().Entities.Create<Box>();
            parent.SizeMode = SizeMode.Stretch;
            parent.Size = new Vec2(100, 50);
            var drawable = parent.Drawables.Create<BoxDrawable>();
            drawable.Position = new Vec2(10, 20);
            drawable.Size = new Vec2(20, 10);

            var localDelta = drawable.WorldDeltaToLocal(new Vec2(50, 25));

            Assert.Equal(10, localDelta.X, 5);
            Assert.Equal(5, localDelta.Y, 5);
        }

        [Fact]
        public void StretchMode_FitsNestedEntityContentToEntitySize()
        {
            var parent = StartScene().Entities.Create<Box>();
            parent.SizeMode = SizeMode.Stretch;
            parent.Size = new Vec2(100, 50);
            var child = parent.CreateChild();
            child.Position = new Vec2(10, 20);
            var drawable = child.Drawables.Create<BoxDrawable>();
            drawable.Position = new Vec2(2, 3);
            drawable.Size = new Vec2(20, 10);
            parent.Entities.Update(0);

            Assert.Equal(new Rectangle(Vec2.Zero, new Vec2(100, 50)), drawable.WorldBounds);
        }

        [Fact]
        public void NoneMode_KeepsExplicitSizeWithoutConstrainingChildren()
        {
            var parent = StartScene().Entities.Create<Box>();
            parent.SizeMode = SizeMode.None;
            parent.Size = new Vec2(40, 50);
            var drawable = parent.Drawables.Create<BoxDrawable>();
            drawable.Position = new Vec2(10, 20);
            drawable.Size = new Vec2(80, 90);

            Assert.Equal(new Vec2(40, 50), parent.Size);
            Assert.Equal(new Rectangle(new Vec2(10, 20), new Vec2(80, 90)), drawable.WorldBounds);
        }

        [Fact]
        public void ClipPolygon_IntersectsRotatedEntityBounds()
        {
            var parent = StartScene().Entities.Create<Box>();
            parent.Position = new Vec2(20, 30);
            parent.Rotation = 45;
            parent.SizeMode = SizeMode.Clip;
            parent.Size = new Vec2(30, 30);
            var child = parent.CreateChild();
            child.Position = new Vec2(5, 5);
            child.Rotation = -20;
            child.SizeMode = SizeMode.Clip;
            child.Size = new Vec2(20, 20);
            parent.Entities.Update(0);

            var polygon = Canvas2d.GetClipPolygon(new[] { parent, child });

            Assert.True(polygon.Count >= 3);
            Assert.All(polygon, point =>
            {
                Assert.True(IsWithinQuad(point, parent.WorldCorners));
                Assert.True(IsWithinQuad(point, child.WorldCorners));
            });
        }

        [Fact]
        public void ScreenBounds_WithoutParent_UsesOwnPositionAndSize()
        {
            var box = StartScene().Entities.Create<Box>();
            box.Position = new Vec2(10, 20);
            SetSize(box, new Vec2(30, 40));

            Assert.Equal(new Rectangle(new Vec2(10, 20), new Vec2(30, 40)), box.ScreenBounds);
        }

        [Fact]
        public void ScreenBounds_WithParent_IsOffsetByParentScreenPosition()
        {
            var parent = StartScene().Entities.Create<Box>();
            parent.Position = new Vec2(100, 200);
            var child = parent.CreateChild();
            child.Position = new Vec2(5, 6);
            SetSize(child, new Vec2(7, 8));

            Assert.Same(parent, child.Parent);
            Assert.Equal(new Rectangle(new Vec2(105, 206), new Vec2(7, 8)), child.ScreenBounds);
        }

        [Fact]
        public void ScreenBounds_InheritsParentScale()
        {
            var parent = StartScene().Entities.Create<Box>();
            parent.Position = new Vec2(100, 200);
            parent.Scale = new Vec2(2, 3);
            var child = parent.CreateChild();
            child.Position = new Vec2(5, 6);
            SetSize(child, new Vec2(7, 8));

            Assert.Equal(new Rectangle(new Vec2(110, 218), new Vec2(14, 24)), child.ScreenBounds);
        }

        [Fact]
        public void ScreenBounds_InheritsParentRotation()
        {
            var parent = StartScene().Entities.Create<Box>();
            parent.Position = new Vec2(100, 200);
            parent.Rotation = 90;
            var child = parent.CreateChild();
            child.Position = new Vec2(10, 0);
            SetSize(child, new Vec2(4, 6));

            Assert.Equal(94, child.ScreenBounds.Pos.X, 5);
            Assert.Equal(210, child.ScreenBounds.Pos.Y, 5);
            Assert.Equal(6, child.ScreenBounds.Size.X, 5);
            Assert.Equal(4, child.ScreenBounds.Size.Y, 5);
        }

        [Fact]
        public void ScreenBounds_UsesParentSizeAndOrigin()
        {
            var parent = StartScene().Entities.Create<Box>();
            parent.Position = new Vec2(100, 200);
            SetSize(parent, new Vec2(100, 80));
            parent.Origin = new Vec2(0.5, 0.5);
            var child = parent.CreateChild();
            SetSize(child, Vec2.One);

            Assert.Equal(new Rectangle(new Vec2(50, 160), Vec2.One), child.ScreenBounds);
        }

        [Fact]
        public void DrawableWorldBounds_InheritsParentScaleAndRotation()
        {
            var parent = StartScene().Entities.Create<Box>();
            parent.Position = new Vec2(100, 200);
            parent.Scale = new Vec2(2, 3);
            parent.Rotation = 90;
            var drawable = parent.Drawables.Create<BoxDrawable>();
            drawable.Position = new Vec2(5, 6);
            drawable.Size = new Vec2(10, 20);

            var bounds = drawable.WorldBounds;

            Assert.Equal(22, bounds.Pos.X, 5);
            Assert.Equal(210, bounds.Pos.Y, 5);
            Assert.Equal(60, bounds.Size.X, 5);
            Assert.Equal(20, bounds.Size.Y, 5);
        }

        [Fact]
        public void DebugDrawable_UsesRotatedEntityCornersForOutline()
        {
            var parent = StartScene().Entities.Create<Box>();
            parent.Position = new Vec2(100, 200);
            parent.Rotation = 90;
            var child = parent.CreateChild();
            child.Position = new Vec2(10, 0);
            SetSize(child, new Vec2(4, 6));
            var debugDrawable = parent.Drawables.Create<Debug2DDrawable>();
            debugDrawable.TypeOObject = child;

            debugDrawable.UpdateLines();
            var linesProperty = typeof(Debug2DDrawable).GetProperty("Lines", BindingFlags.Instance | BindingFlags.NonPublic);
            var lines = Assert.IsType<List<Vec2>>(linesProperty.GetValue(debugDrawable));

            AssertPoint(new Vec2(100, 210), lines[0]);
            AssertPoint(new Vec2(100, 214), lines[1]);
            AssertPoint(new Vec2(94, 214), lines[2]);
            AssertPoint(new Vec2(94, 210), lines[3]);
            Assert.Equal(lines[0], lines[4]);
        }

        public class CameraCanvasProxy : System.Reflection.DispatchProxy
        {
            private readonly Dictionary<string, object> values = new();

            protected override object Invoke(System.Reflection.MethodInfo targetMethod, object[] args)
            {
                if (targetMethod.Name.StartsWith("set_"))
                {
                    values[targetMethod.Name[4..]] = args[0];
                    return null;
                }
                if (targetMethod.Name.StartsWith("get_"))
                    return values.TryGetValue(targetMethod.Name[4..], out var value) ? value : null;
                throw new NotSupportedException(targetMethod.Name);
            }
        }

        [Fact]
        public void DebugDrawable_MapsIgnoreCameraObjectsThroughTheCamera()
        {
            var scene = StartScene();
            var stack = scene.CreateDrawStack("hud", "Hud", 0, true);
            var box = scene.Entities.Create<Box>("box", drawStack: stack);
            box.Position = new Vec2(10, 20);
            SetSize(box, new Vec2(30, 40));
            var canvas = System.Reflection.DispatchProxy.Create<TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces.ICanvas, CameraCanvasProxy>();
            canvas.WorldTranslation = new TypeOEngine.Typedeaf.Core.Common.Vec3(100, 200, 0);
            canvas.WorldScale = 2;
            var overlay = scene.Drawables.Create<Debug2DDrawable>();
            overlay.TypeOObject = box;
            overlay.CameraCanvas = canvas;

            var (pos, size) = overlay.GetBounds();
            AssertPoint(new Vec2(105, 210), pos);
            AssertPoint(new Vec2(15, 20), size);
            Assert.True(overlay.ContainsPoint(new Vec2(110, 220)));
            Assert.False(overlay.ContainsPoint(new Vec2(10, 20)));
            AssertPoint(new Vec2(10, 20), overlay.FromEditorWorld(overlay.ToEditorWorld(new Vec2(10, 20))));
        }

        [Fact]
        public void DebugDrawable_HitTestUsesRotatedEntityCorners()
        {
            var parent = StartScene().Entities.Create<Box>();
            parent.Position = new Vec2(100, 200);
            parent.Rotation = 45;
            var child = parent.CreateChild();
            SetSize(child, new Vec2(10, 10));
            var debugDrawable = parent.Drawables.Create<Debug2DDrawable>();
            debugDrawable.TypeOObject = child;

            Assert.True(debugDrawable.ContainsPoint(new Vec2(100, 207)));
            Assert.False(debugDrawable.ContainsPoint(new Vec2(93, 200)));
        }

        [Fact]
        public void PickTopMost_ResolvesHiddenChildToVisibleParent()
        {
            var parent = StartScene().Entities.Create<Box>();
            parent.Position = new Vec2(100, 200);
            SetSize(parent, new Vec2(10, 10));
            var child = parent.CreateChild();
            child.Position = new Vec2(30, 0);
            SetSize(child, new Vec2(10, 10));

            var rootComponent = new TypeD.Models.Data.Component();
            var visibleComponent = new TypeD.Models.Data.Component();
            var hiddenComponent = new TypeD.Models.Data.Component();
            SetParentComponent(visibleComponent, rootComponent);
            SetParentComponent(hiddenComponent, visibleComponent);
            visibleComponent.DefinitionChildren.Add(hiddenComponent);

            var visibleOverlay = parent.Drawables.Create<Debug2DDrawable>();
            visibleOverlay.TypeOObject = parent;
            visibleOverlay.Component = visibleComponent;
            var hiddenOverlay = parent.Drawables.Create<Debug2DDrawable>();
            hiddenOverlay.TypeOObject = child;
            hiddenOverlay.Component = hiddenComponent;

            var picked = SelectionController2DDrawable.PickTopMost(
                new[] { visibleOverlay, hiddenOverlay },
                new Vec2(135, 205));

            Assert.Same(visibleOverlay, picked);
        }

        private static void SetParentComponent(TypeD.Models.Data.Component component, TypeD.Models.Data.Component parent)
        {
            typeof(TypeD.Models.Data.Component)
                .GetProperty(nameof(TypeD.Models.Data.Component.ParentComponent))
                .GetSetMethod(true)
                .Invoke(component, new object[] { parent });
        }

        private static void AssertPoint(Vec2 expected, Vec2 actual)
        {
            Assert.Equal(expected.X, actual.X, 5);
            Assert.Equal(expected.Y, actual.Y, 5);
        }

        private static bool IsWithinQuad(
            Vec2 point,
            (Vec2 TopLeft, Vec2 TopRight, Vec2 BottomRight, Vec2 BottomLeft) corners)
        {
            var vertices = new[] { corners.TopLeft, corners.TopRight, corners.BottomRight, corners.BottomLeft };
            var sign = 0;
            for (var i = 0; i < vertices.Length; i++)
            {
                var edge = vertices[(i + 1) % vertices.Length] - vertices[i];
                var relative = point - vertices[i];
                var cross = edge.X * relative.Y - edge.Y * relative.X;
                if (Math.Abs(cross) < 1e-5)
                    continue;

                var currentSign = Math.Sign(cross);
                if (sign != 0 && currentSign != sign)
                    return false;
                sign = currentSign;
            }

            return true;
        }

        [Fact]
        public void ChildModelMatrix_RefreshesWhenParentChanges()
        {
            var parent = StartScene().Entities.Create<Box>();
            parent.Position = new Vec2(100, 200);
            var child = parent.CreateChild();
            child.Position = new Vec2(5, 6);
            SetSize(child, Vec2.One);

            Assert.Equal(new Vec2(105, 206), child.ScreenBounds.Pos);
            parent.Position = new Vec2(200, 300);
            Assert.Equal(new Vec2(205, 306), child.ScreenBounds.Pos);
        }

        [Fact]
        public void WorldDeltaToLocal_AccountsForParentScaleAndRotation()
        {
            var parent = StartScene().Entities.Create<Box>();
            parent.Scale = new Vec2(2, 3);
            parent.Rotation = 90;

            var localDelta = parent.WorldDeltaToLocal(new Vec2(-18, 10));

            Assert.Equal(5, localDelta.X, 5);
            Assert.Equal(6, localDelta.Y, 5);
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
            SetSize(box, new Vec2(100, 100));

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
            SetSize(box, new Vec2(100, 100));

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
