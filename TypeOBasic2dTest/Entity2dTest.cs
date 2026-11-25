using System.Reflection;
using System.Collections.Concurrent;
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
using TypeDBasic2d.Components;
using TypeD.Models.Data;
using TypeD.Models.Data.SaveContexts;
using TypeD.Models.Interfaces;
using TypeD.Commands;
using TypeD.Models.Data.Hooks;
using TypeDCore.Models.Data.Hooks;
using TypeDCore.Models.Data;
using TypeDCore.Models.Interfaces;
using TypeDCore.Models.Data.SettingContexts;
using TypeD.Models.Data.SettingContexts;
using TypeOEngine.Typedeaf.Core.Engine.Services.Input;
using TypeOEngine.Typedeaf.Desktop.Engine.Hardwares.Interfaces;
using TypeOEngine.Typedeaf.Desktop.Engine.Services;
using MouseButton = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;

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

        [Theory]
        [InlineData("Right, Bottom", 40, 20, 10, 20, 80, 40)]
        [InlineData("Left, Top", -40, -20, -30, 0, 80, 40)]
        [InlineData("Right", 40, 0, 10, 10, 80, 40)]
        [InlineData("Top", 0, -20, -10, 0, 80, 40)]
        public void Resize_ShiftScalesUniformlyAroundOppositeHandle(string axes, double dx, double dy,
            double x, double y, double width, double height)
        {
            var scene = StartScene();
            var drawable = scene.Drawables.Create<BoxDrawable>();
            drawable.Position = new Vec2(30, 30);
            drawable.Origin = new Vec2(0.5, 0.5);
            drawable.Size = new Vec2(40, 20);
            var overlay = new Debug2DDrawable { TypeOObject = drawable, Component = new Component() };
            var (controller, model) = BeginResize(new[] { overlay }, axes);
            model.UniformResizeModifierDown = true;

            UpdateResize(controller, new Vec2(dx, dy));

            AssertPoint(new Vec2(width, height), drawable.Size);
            AssertPoint(new Vec2(x, y), overlay.GetBounds().Pos);
        }

        [Fact]
        public void Resize_ShiftCanBeToggledDuringDrag()
        {
            var scene = StartScene();
            var drawable = scene.Drawables.Create<BoxDrawable>();
            drawable.Size = new Vec2(40, 20);
            var overlay = new Debug2DDrawable { TypeOObject = drawable, Component = new Component() };
            var (controller, model) = BeginResize(new[] { overlay }, "Right, Bottom");

            UpdateResize(controller, new Vec2(40, 0));
            AssertPoint(new Vec2(80, 20), drawable.Size);
            model.UniformResizeModifierDown = true;
            UpdateResize(controller, new Vec2(40, 0));
            AssertPoint(new Vec2(72, 36), drawable.Size);
            model.UniformResizeModifierDown = false;
            UpdateResize(controller, new Vec2(40, 0));
            AssertPoint(new Vec2(80, 20), drawable.Size);
        }

        [Fact]
        public void Resize_ShiftPreservesGroupProportionsAndMinimumSize()
        {
            var scene = StartScene();
            var first = scene.Drawables.Create<BoxDrawable>();
            first.Size = new Vec2(4, 8);
            var second = scene.Drawables.Create<BoxDrawable>();
            second.Position = new Vec2(30, 0);
            second.Size = new Vec2(20, 10);
            var overlays = new[] { first, second }
                .Select(drawable => new Debug2DDrawable { TypeOObject = drawable, Component = new Component() }).ToArray();
            var (controller, model) = BeginResize(overlays, "Right");
            model.UniformResizeModifierDown = true;

            UpdateResize(controller, new Vec2(-100, 0));

            AssertPoint(new Vec2(1, 2), first.Size);
            AssertPoint(new Vec2(5, 2.5), second.Size);
            AssertPoint(new Vec2(7.5, 3.75), overlays[1].GetBounds().Pos);
        }

        [Fact]
        public void Resize_ShiftKeepsRotatedDrawableAnchored()
        {
            var scene = StartScene();
            var drawable = scene.Drawables.Create<BoxDrawable>();
            drawable.Position = new Vec2(50, 50);
            drawable.Origin = new Vec2(0.5, 0.5);
            drawable.Size = new Vec2(40, 20);
            drawable.Rotation = 30;
            var overlay = new Debug2DDrawable { TypeOObject = drawable, Component = new Component() };
            var bounds = overlay.GetBounds();
            var (controller, model) = BeginResize(new[] { overlay }, "Right, Bottom");
            model.UniformResizeModifierDown = true;

            UpdateResize(controller, bounds.Size);

            AssertPoint(new Vec2(80, 40), drawable.Size);
            Assert.Equal(bounds.Pos.X, overlay.GetBounds().Pos.X, 4);
            Assert.Equal(bounds.Pos.Y, overlay.GetBounds().Pos.Y, 4);
        }

        [Fact]
        public void Resize_ShiftCommitsEachSelectedComponent()
        {
            var scene = StartScene();
            var overlays = new List<Debug2DDrawable>();
            for (var i = 0; i < 2; i++)
            {
                var drawable = scene.Drawables.Create<BoxDrawable>();
                drawable.Position = new Vec2(i * 20, 0);
                drawable.Size = new Vec2(10, 10);
                var component = new Component();
                component.Properties.Add(new Property { Name = "Position", Value = drawable.Position });
                component.Properties.Add(new Property { Name = "Size", Value = drawable.Size });
                overlays.Add(new Debug2DDrawable { TypeOObject = drawable, Component = component });
            }
            var (controller, model) = BeginResize(overlays.ToArray(), "Right");
            model.UniformResizeModifierDown = true;
            var submitted = new List<ChangePropertyCommandData>();
            controller.EditorCommandRegistry = MakeProxy<IEditorCommandRegistry>((method, args) =>
            {
                Assert.Equal("Execute", method.Name);
                submitted.Add((ChangePropertyCommandData)args[1]);
                return true;
            });

            UpdateResize(controller, new Vec2(30, 0));
            typeof(SelectionController2DDrawable).GetMethod("CommitDrag", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(controller, null);

            Assert.Contains(submitted, change => ReferenceEquals(change.Component, overlays[0].Component) && change.Property.Name == "Size");
            Assert.Contains(submitted, change => ReferenceEquals(change.Component, overlays[1].Component) && change.Property.Name == "Size");
            Assert.All(submitted, change => Assert.Contains(change.Property, change.Component.Properties));
        }

        private static (SelectionController2DDrawable Controller, IEditorToolModel Model) BeginResize(
            Debug2DDrawable[] overlays, string axes)
        {
            var modelType = typeof(IEditorToolModel).Assembly.GetType("TypeDCore.Models.EditorToolModel");
            var model = (IEditorToolModel)Activator.CreateInstance(modelType, true);
            var controller = new SelectionController2DDrawable { EditorToolModel = model };
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var axesType = typeof(SelectionController2DDrawable).GetNestedType("ResizeAxes", BindingFlags.NonPublic);
            typeof(SelectionController2DDrawable).GetMethod("BeginDrag", flags).Invoke(controller,
                new object[] { EditorTool.Resize, overlays, Vec2.Zero, false, false, Enum.Parse(axesType, axes) });
            return (controller, model);
        }

        private static void UpdateResize(SelectionController2DDrawable controller, Vec2 delta) =>
            typeof(SelectionController2DDrawable).GetMethod("UpdateDrag", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(controller, new object[] { delta });

        [Theory]
        [InlineData(0.5)]
        [InlineData(2.0)]
        public void MoveGizmo_CenterMovesSelectionFreelyEvenBetweenObjects(double unitsPerPixel)
        {
            var scene = StartScene();
            var first = scene.Drawables.Create<BoxDrawable>();
            var second = scene.Drawables.Create<BoxDrawable>();
            first.Size = second.Size = new Vec2(10, 10);
            second.Position = new Vec2(100, 0);
            var overlays = new[] { first, second }.Select(drawable =>
            {
                var component = new Component();
                component.Properties.Add(new Property { Name = "Position", Value = drawable.Position });
                return new Debug2DDrawable { TypeOObject = drawable, Component = component, Focused = true };
            }).ToArray();
            var (controller, input, submissions) = CreateGizmoController(EditorTool.Move, unitsPerPixel);
            var center = new Vec2(55, 5);
            var press = center + new Vec2(3 * unitsPerPixel, 0);

            input(true, false);
            ProcessGizmoPointer(controller, overlays, null, press);
            input(true, true);
            ProcessGizmoPointer(controller, overlays, null, press + new Vec2(4, 7));
            input(false, true);
            ProcessGizmoPointer(controller, overlays, null, press + new Vec2(4, 7));

            AssertPoint(new Vec2(4, 7), first.Position);
            AssertPoint(new Vec2(104, 7), second.Position);
            Assert.Equal(2, submissions.Count);
            Assert.True(controller.EditorToolModel.TryCapturePointer(this));
            controller.EditorToolModel.ReleasePointer(this);
        }

        [Theory]
        [InlineData(30, 0, 4, 0)]
        [InlineData(0, -30, 0, 7)]
        public void MoveGizmo_AxisHandlesRemainConstrained(int x, int y, int expectedX, int expectedY)
        {
            var drawable = StartScene().Drawables.Create<BoxDrawable>();
            drawable.Size = new Vec2(10, 10);
            var component = new Component();
            component.Properties.Add(new Property { Name = "Position", Value = Vec2.Zero });
            var overlay = new Debug2DDrawable { TypeOObject = drawable, Component = component, Focused = true };
            var (controller, input, _) = CreateGizmoController(EditorTool.Move, 1);
            var press = new Vec2(5 + x, 5 + y);
            input(true, false);
            ProcessGizmoPointer(controller, new[] { overlay }, null, press);
            input(true, true);
            ProcessGizmoPointer(controller, new[] { overlay }, null, press + new Vec2(4, 7));
            input(false, true);
            ProcessGizmoPointer(controller, new[] { overlay }, null, press + new Vec2(4, 7));

            AssertPoint(new Vec2(expectedX, expectedY), drawable.Position);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(2.0)]
        public void RotateGizmo_RingRotatesAroundPivotEvenOverAnotherObject(double unitsPerPixel)
        {
            var scene = StartScene();
            var drawable = scene.Drawables.Create<BoxDrawable>();
            drawable.Position = new Vec2(100, 100);
            drawable.Size = new Vec2(10, 10);
            var component = new Component();
            component.Properties.Add(new Property { Name = "Rotation", Value = 0.0 });
            var overlay = new Debug2DDrawable { TypeOObject = drawable, Component = component, Focused = true };
            var other = scene.Drawables.Create<BoxDrawable>();
            var otherOverlay = new Debug2DDrawable { TypeOObject = other, Component = new Component() };
            var (controller, input, submissions) = CreateGizmoController(EditorTool.Rotate, unitsPerPixel);
            var radius = 32 * unitsPerPixel;
            var pivot = drawable.Position;

            input(true, false);
            ProcessGizmoPointer(controller, new[] { overlay, otherOverlay }, otherOverlay, pivot + new Vec2(radius, 0));
            input(true, true);
            ProcessGizmoPointer(controller, new[] { overlay, otherOverlay }, null, pivot + new Vec2(0, radius));
            input(false, true);
            ProcessGizmoPointer(controller, new[] { overlay, otherOverlay }, null, pivot + new Vec2(0, radius));

            Assert.Equal(90, drawable.Rotation, 5);
            var change = Assert.Single(submissions);
            Assert.Same(component, change.Component);
            Assert.Equal(90.0, change.Value);
            Assert.True(controller.EditorToolModel.TryCapturePointer(this));
            controller.EditorToolModel.ReleasePointer(this);
        }

        [Fact]
        public void MoveGizmo_SnapsBoundingBoxToAlignmentOfOtherComponents()
        {
            var scene = StartScene();
            var moved = scene.Drawables.Create<BoxDrawable>();
            var other = scene.Drawables.Create<BoxDrawable>();
            moved.Size = other.Size = new Vec2(10, 10);
            other.Position = new Vec2(102, 50);
            var movedComponent = new Component();
            movedComponent.Properties.Add(new Property { Name = "Position", Value = moved.Position });
            var overlay = new Debug2DDrawable { TypeOObject = moved, Component = movedComponent, Focused = true };
            var otherOverlay = new Debug2DDrawable { TypeOObject = other, Component = new Component() };
            var overlays = new[] { overlay, otherOverlay };
            var (controller, input, _) = CreateGizmoController(EditorTool.Move, 1);
            controller.GetDebugDrawables = () => overlays;
            controller.EditorToolModel.SnapToAlignment = true;
            var press = new Vec2(8, 5);

            input(true, false);
            ProcessGizmoPointer(controller, overlays, null, press);
            input(true, true);
            ProcessGizmoPointer(controller, overlays, null, press + new Vec2(96, 3));

            // The centre of the moved box (101) snaps to the left edge of the other box (102); y is out of range
            AssertPoint(new Vec2(97, 3), moved.Position);

            input(false, true);
            ProcessGizmoPointer(controller, overlays, null, press + new Vec2(96, 3));
        }

        private static (SelectionController2DDrawable Controller, Action<bool, bool> SetButtons, List<ChangePropertyCommandData> Submissions)
            CreateGizmoController(EditorTool tool, double unitsPerPixel)
        {
            var down = false;
            var oldDown = false;
            var hardware = MakeProxy<IMouseHardware>((method, args) => method.Name switch
            {
                "CurrentButtonDownEvent" => (MouseButton)args[0] == MouseButton.Left && down,
                "OldButtonUpEvent" => (MouseButton)args[0] != MouseButton.Left || !oldDown,
                _ => throw new InvalidOperationException(method.Name)
            });
            var mouse = new MouseInputService();
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(MouseInputService).GetProperty("MouseHardware", flags).SetValue(mouse, hardware);
            typeof(MouseInputService).GetProperty("KeyConverter", flags).SetValue(mouse, new KeyConverter());
            mouse.SetKeyAlias(MouseButton.Left, MouseButton.Left);
            mouse.SetKeyAlias(MouseButton.Right, MouseButton.Right);
            var modelType = typeof(IEditorToolModel).Assembly.GetType("TypeDCore.Models.EditorToolModel");
            var model = (IEditorToolModel)Activator.CreateInstance(modelType, true);
            model.ActiveTool = tool;
            var submissions = new List<ChangePropertyCommandData>();
            var controller = new SelectionController2DDrawable
            {
                EditorToolModel = model,
                MouseInputService = mouse,
                EditorCommandRegistry = MakeProxy<IEditorCommandRegistry>((_, args) =>
                {
                    submissions.Add((ChangePropertyCommandData)args[1]);
                    return true;
                })
            };
            typeof(SelectionController2DDrawable).GetProperty("UnitsPerPixel", flags).SetValue(controller, unitsPerPixel);
            return (controller, (current, previous) => { down = current; oldDown = previous; }, submissions);
        }

        private static void ProcessGizmoPointer(SelectionController2DDrawable controller, Debug2DDrawable[] drawables,
            Debug2DDrawable topmost, Vec2 position) =>
            typeof(SelectionController2DDrawable).GetMethod("ProcessPointer", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(controller, new object[] { drawables, topmost, position });

        [Fact]
        public void ViewerNudge_MovesSelectionInWorldDirectionAndAccumulatesPendingChanges()
        {
            var scene = StartScene();
            var parent = scene.Entities.Create<Box>();
            parent.Rotation = 90;
            var first = parent.Drawables.Create<BoxDrawable>();
            first.Position = new Vec2(10, 20);
            var second = scene.Drawables.Create<BoxDrawable>();
            second.Position = new Vec2(30, 40);
            var overlays = new[] { first, second }.Select(drawable =>
            {
                var component = new Component();
                component.Properties.Add(new Property { Name = "Position", Value = drawable.Position, Type = typeof(Vec2) });
                return new Debug2DDrawable { TypeOObject = drawable, Component = component };
            }).ToArray();
            var project = CreateTestProject();
            var submissions = new List<ChangePropertyCommandData>();
            var initializer = CreateNudgeInitializer(project, overlays, submissions);

            for (var i = 0; i < 2; i++)
            {
                var hook = new ViewerNudgeHook { Project = project, Components = overlays.Select(o => o.Component).ToArray(), Delta = new Vec2(1, 0) };
                initializer.GetType().GetMethod("ViewerNudge", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(initializer, new object[] { hook });
                Assert.True(hook.Handled);
            }

            Assert.Equal(2, submissions.Count);
            Assert.All(submissions, change => Assert.Single(change.AdditionalChanges));
            AssertPoint(new Vec2(10, 18), (Vec2)overlays[0].Component.Properties[0].Value);
            AssertPoint(new Vec2(32, 40), (Vec2)overlays[1].Component.Properties[0].Value);
        }

        [Fact]
        public void ViewerNudge_DoesNotMoveSelectedChildTwice()
        {
            var scene = StartScene();
            var parent = scene.Entities.Create<Box>();
            var child = parent.Drawables.Create<BoxDrawable>();
            var parentComponent = new Component();
            var childComponent = new Component();
            SetParentComponent(childComponent, parentComponent);
            parentComponent.Properties.Add(new Property { Name = "Position", Value = Vec2.Zero });
            childComponent.Properties.Add(new Property { Name = "Position", Value = Vec2.Zero });
            var overlays = new[]
            {
                new Debug2DDrawable { TypeOObject = parent, Component = parentComponent },
                new Debug2DDrawable { TypeOObject = child, Component = childComponent }
            };
            var project = CreateTestProject();
            var submissions = new List<ChangePropertyCommandData>();
            var initializer = CreateNudgeInitializer(project, overlays, submissions);
            var hook = new ViewerNudgeHook { Project = project, Components = new[] { parentComponent, childComponent }, Delta = new Vec2(1, 0) };

            initializer.GetType().GetMethod("ViewerNudge", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(initializer, new object[] { hook });

            Assert.True(hook.Handled);
            Assert.Same(parentComponent, Assert.Single(submissions).Component);
            Assert.Empty(submissions[0].AdditionalChanges);
            AssertPoint(new Vec2(1, 0), (Vec2)parentComponent.Properties[0].Value);
            AssertPoint(Vec2.Zero, (Vec2)childComponent.Properties[0].Value);
        }

        [Theory]
        [InlineData(-1, 0, 9.5, 20, 11, 20)]
        [InlineData(1, 0, 10.5, 20, 11, 20)]
        [InlineData(0, -1, 10, 19.25, 10, 21)]
        [InlineData(0, 1, 10, 20.25, 10, 21)]
        public void ViewerNudge_ResizeGrowsAndShrinksEachEdge(int x, int y, double px, double py, double width, double height)
        {
            var drawable = StartScene().Drawables.Create<BoxDrawable>();
            drawable.Position = new Vec2(10, 20);
            drawable.Size = new Vec2(10, 20);
            drawable.Origin = new Vec2(0.5, 0.25);
            var component = new Component();
            component.Properties.Add(new Property { Name = "Position", Value = drawable.Position });
            component.Properties.Add(new Property { Name = "Size", Value = drawable.Size });
            component.Properties.Add(new Property { Name = "Origin", Value = drawable.Origin });
            var overlay = new Debug2DDrawable { TypeOObject = drawable, Component = component };
            var project = CreateTestProject();
            var submissions = new List<ChangePropertyCommandData>();
            var initializer = CreateNudgeInitializer(project, new[] { overlay }, submissions);
            var hook = new ViewerNudgeHook { Project = project, Components = new[] { component }, Tool = EditorTool.Resize, Delta = new Vec2(x, y) };

            initializer.GetType().GetMethod("ViewerNudge", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(initializer, new object[] { hook });
            Assert.True(hook.Handled);
            AssertPoint(new Vec2(px, py), (Vec2)component.Properties[0].Value);
            AssertPoint(new Vec2(width, height), (Vec2)component.Properties[1].Value);
            Assert.Single(Assert.Single(submissions).AdditionalChanges);
            AssertPoint(new Vec2(10, 20), drawable.Position);
            AssertPoint(new Vec2(10, 20), drawable.Size);

            hook.Handled = false;
            hook.Shrink = true;
            initializer.GetType().GetMethod("ViewerNudge", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(initializer, new object[] { hook });
            AssertPoint(new Vec2(10, 20), (Vec2)component.Properties[0].Value);
            AssertPoint(new Vec2(10, 20), (Vec2)component.Properties[1].Value);
            hook.Handled = false;
            hook.Delta = new Vec2(x * 100, y * 100);
            initializer.GetType().GetMethod("ViewerNudge", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(initializer, new object[] { hook });
            AssertPoint(new Vec2(x == 0 ? 10 : 1, y == 0 ? 20 : 1), (Vec2)component.Properties[1].Value);
        }

        [Fact]
        public void ViewerNudge_RotatedResizeAndOriginUseCurrentProperties()
        {
            var drawable = StartScene().Drawables.Create<BoxDrawable>();
            drawable.Position = new Vec2(10, 20);
            drawable.Size = new Vec2(10, 20);
            drawable.Origin = new Vec2(0.5, 0.5);
            drawable.Rotation = 90;
            var component = new Component();
            component.Properties.Add(new Property { Name = "Position", Value = drawable.Position });
            component.Properties.Add(new Property { Name = "Size", Value = drawable.Size });
            component.Properties.Add(new Property { Name = "Origin", Value = drawable.Origin });
            component.Properties.Add(new Property { Name = "Rotation", Value = drawable.Rotation });
            var project = CreateTestProject();
            var submissions = new List<ChangePropertyCommandData>();
            var initializer = CreateNudgeInitializer(project, new[] { new Debug2DDrawable { TypeOObject = drawable, Component = component } }, submissions);
            var hook = new ViewerNudgeHook { Project = project, Components = new[] { component }, Tool = EditorTool.Resize, Delta = new Vec2(-2, 0) };
            var nudge = initializer.GetType().GetMethod("ViewerNudge", BindingFlags.Instance | BindingFlags.NonPublic);
            nudge.Invoke(initializer, new object[] { hook });
            AssertPoint(new Vec2(9, 20), (Vec2)component.Properties[0].Value);
            AssertPoint(new Vec2(10, 22), (Vec2)component.Properties[1].Value);

            hook.Handled = false;
            hook.Tool = EditorTool.Origin;
            hook.Delta = new Vec2(1, 0);
            nudge.Invoke(initializer, new object[] { hook });
            hook.Handled = false;
            nudge.Invoke(initializer, new object[] { hook });
            AssertPoint(new Vec2(0.5, 0.5 - 2.0 / 22), (Vec2)component.Properties[2].Value);
            AssertPoint(new Vec2(11, 20), (Vec2)component.Properties[0].Value);
            Assert.Single(submissions[1].AdditionalChanges);
            AssertPoint(new Vec2(0.5, 0.5), drawable.Origin);
            drawable.Position = new Vec2(9, 20);
            drawable.Size = new Vec2(10, 22);
            var before = drawable.WorldCorners;
            drawable.Position = (Vec2)component.Properties[0].Value;
            drawable.Origin = (Vec2)component.Properties[2].Value;
            var after = drawable.WorldCorners;
            AssertPoint(before.TopLeft, after.TopLeft);
            AssertPoint(before.BottomRight, after.BottomRight);

            hook.Handled = false;
            hook.Tool = EditorTool.Rotate;
            hook.Delta = new Vec2(-10, 0);
            nudge.Invoke(initializer, new object[] { hook });
            Assert.Equal(80.0, component.Properties[3].Value);
            Assert.Equal(90, drawable.Rotation);
        }

        [Fact]
        public void ViewerNudge_RejectsResizeOfAutoSizedEntity()
        {
            var entity = StartScene().Entities.Create<Box>();
            var component = new Component();
            component.Properties.Add(new Property { Name = "Position", Value = Vec2.Zero });
            component.Properties.Add(new Property { Name = "Size", Value = new Vec2(10, 10) });
            var project = CreateTestProject();
            var submissions = new List<ChangePropertyCommandData>();
            var initializer = CreateNudgeInitializer(project, new[] { new Debug2DDrawable { TypeOObject = entity, Component = component } }, submissions);
            var logs = new List<string>();
            var logger = MakeProxy<ILogModel>((method, args) => { logs.Add((string)args[0]); return null; });
            var resources = MakeProxy<IResourceModel>((method, _) =>
                method.IsGenericMethod && method.GetGenericArguments()[0] == typeof(ILogModel) ? logger : null);
            initializer.GetType().GetProperty("Resources").SetValue(initializer, resources);
            var hook = new ViewerNudgeHook { Project = project, Components = new[] { component }, Tool = EditorTool.Resize, Delta = new Vec2(1, 0) };

            initializer.GetType().GetMethod("ViewerNudge", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(initializer, new object[] { hook });

            Assert.Empty(submissions);
            Assert.Contains("auto-sized", Assert.Single(logs));
        }

        [Fact]
        public void ViewerNudge_OptionsExposeAndValidateShiftStep()
        {
            var viewerSettings = new ViewerSettingContext();
            var otherSettings = new Dictionary<Type, object>
            {
                [typeof(ViewerSettingContext)] = viewerSettings,
                [typeof(MainWindowSettingContext)] = new MainWindowSettingContext(),
                [typeof(OutputSettingContext)] = new OutputSettingContext()
            };
            var saved = 0;
            var settings = MakeProxy<ISettingModel>((method, args) =>
            {
                if (method.Name == "GetContext")
                    return otherSettings[method.GetGenericArguments()[0]];
                if (method.Name == "SetContext")
                {
                    Assert.Same(viewerSettings, args[0]);
                    saved++;
                    return null;
                }
                throw new InvalidOperationException(method.Name);
            });
            var logs = new List<string>();
            var logger = MakeProxy<ILogModel>((method, args) => { logs.Add((string)args[0]); return null; });
            var resources = MakeProxy<IResourceModel>((method, _) =>
                method.IsGenericMethod && method.GetGenericArguments()[0] == typeof(ILogModel) ? logger : null);
            var type = typeof(ViewerSettingContext).Assembly.GetType("TypeDCore.TypeDCoreInitializer");
            var initializer = Activator.CreateInstance(type, true);
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            type.GetProperty("SettingModel", flags).SetValue(initializer, settings);
            type.GetProperty("Resources").SetValue(initializer, resources);
            var hook = new OptionsHook { Level = SettingLevel.User };
            type.GetMethod("OptionsWindowOpened", flags).Invoke(initializer, new object[] { hook });

            var option = Assert.Single(hook.Items, item => item.Title == "Shift Move Step");
            Assert.Equal("Viewer", option.Category);
            option.Set(23);
            Assert.Equal(23, viewerSettings.ShiftMoveStep.Value);
            Assert.Equal(1, saved);
            var jsonOptions = TypeD.Helpers.JSON.CreateOptions();
            var json = System.Text.Json.JsonSerializer.Serialize(viewerSettings, jsonOptions);
            var reloaded = System.Text.Json.JsonSerializer.Deserialize<ViewerSettingContext>(json, jsonOptions);
            Assert.Equal(23, reloaded.ShiftMoveStep.Value);
            option.Set(0);
            Assert.Equal(23, viewerSettings.ShiftMoveStep.Value);
            Assert.Single(logs);
            Assert.Equal(1, saved);
        }

        private static object CreateNudgeInitializer(Project project, Debug2DDrawable[] overlays, List<ChangePropertyCommandData> submissions)
        {
            var type = typeof(Debug2DDrawable).Assembly.GetType("TypeDBasic2d.TypeDBasic2dInitializer");
            var initializer = Activator.CreateInstance(type, true);
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            type.GetProperty("Project", flags).SetValue(initializer, project);
            type.GetProperty("DebugDrawablesMap", flags).SetValue(initializer,
                new ConcurrentDictionary<string, Debug2DDrawable>(overlays.Select((overlay, i) =>
                    new KeyValuePair<string, Debug2DDrawable>(i.ToString(), overlay))));
            type.GetProperty("EditorCommandRegistry", flags).SetValue(initializer,
                MakeProxy<IEditorCommandRegistry>((method, args) =>
                {
                    Assert.Equal("component.change-property", args[0]);
                    var change = (ChangePropertyCommandData)args[1];
                    submissions.Add(change);
                    foreach (var item in new[] { change }.Concat(change.AdditionalChanges))
                        item.Property.Value = item.Value;
                    return true;
                }));
            return initializer;
        }

        [Theory]
        [InlineData("Forward", 1, "ACBD")]
        [InlineData("Front", 1, "ACDB")]
        [InlineData("Backward", 2, "ACBD")]
        [InlineData("Back", 2, "CABD")]
        public void DrawOrderPlanner_MovesExactlyPastRequestedOverlapsWithTiedOrders(string move, int selected, string expected)
        {
            var scene = StartScene();
            var overlays = CreateOrderOverlays(scene, 0, 0, 0, 0);

            var changes = PlanOrder(overlays[selected].Component, overlays, move);

            Assert.NotNull(changes);
            ApplyOrderChanges(overlays, changes);
            var order = scene.DrawStack.GetDrawablesInDrawOrder()
                .Select(drawable => overlays.Single(overlay => ReferenceEquals(overlay.TypeOObject, drawable)).Component.ClassName);
            Assert.Equal(expected, string.Concat(order));
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void DrawOrderPlanner_HandlesIntegerLimitsAndAdjacentOrders(int order)
        {
            var scene = StartScene();
            var overlays = CreateOrderOverlays(scene, order, order, order);
            var changes = PlanOrder(overlays[1].Component, overlays, "Forward");

            Assert.NotNull(changes);
            ApplyOrderChanges(overlays, changes);
            var actual = scene.DrawStack.GetDrawablesInDrawOrder()
                .Select(drawable => overlays.Single(overlay => ReferenceEquals(overlay.TypeOObject, drawable)).Component.ClassName);
            Assert.Equal("ACB", string.Concat(actual));
        }

        [Fact]
        public void DrawOrderPlanner_SkipsNonOverlapsAndOtherDrawStacks()
        {
            var scene = StartScene();
            var overlays = CreateOrderOverlays(scene, 0, 1, 2, 3);
            ((BoxDrawable)overlays[1].TypeOObject).Position = new Vec2(100, 100);
            var otherStack = scene.CreateDrawStack("other", "Other");
            var other = otherStack.DrawableManager.Create<BoxDrawable>();
            other.Size = new Vec2(10, 10);
            var otherComponent = OrderComponent("Other", 0);
            overlays.Add(new Debug2DDrawable { TypeOObject = other, Component = otherComponent });

            var changes = PlanOrder(overlays[0].Component, overlays, "Forward");

            Assert.NotNull(changes);
            ApplyOrderChanges(overlays, changes);
            var order = scene.DrawStack.GetDrawablesInDrawOrder()
                .Select(drawable => overlays.Single(overlay => ReferenceEquals(overlay.TypeOObject, drawable)).Component.ClassName);
            Assert.Equal("BCAD", string.Concat(order));
            Assert.Equal(0, other.DrawOrder);
        }

        [Fact]
        public void DrawOrderPlanner_DoesNotTreatRotatedBoundingBoxAsOverlap()
        {
            var scene = StartScene();
            var overlays = CreateOrderOverlays(scene, 0, 1);
            var first = (BoxDrawable)overlays[0].TypeOObject;
            first.Size = new Vec2(10, 2);
            first.Rotation = 45;
            var second = (BoxDrawable)overlays[1].TypeOObject;
            second.Position = new Vec2(-1, 6);
            second.Size = new Vec2(1, 1);

            Assert.Null(PlanOrder(overlays[0].Component, overlays, "Forward"));
        }

        [Fact]
        public void DrawOrderPlanner_RestoringTiedOrdersRestoresOriginalRenderingOrder()
        {
            var scene = StartScene();
            var overlays = CreateOrderOverlays(scene, 0, 0, 0, 0);
            var changes = PlanOrder(overlays[1].Component, overlays, "Front");
            ApplyOrderChanges(overlays, changes);
            Assert.Equal("ACDB", RenderOrder(scene, overlays));

            foreach (var overlay in overlays)
                ((BoxDrawable)overlay.TypeOObject).DrawOrder = 0;
            Assert.Equal("ABCD", RenderOrder(scene, overlays));
            ApplyOrderChanges(overlays, changes);
            Assert.Equal("ACDB", RenderOrder(scene, overlays));
        }

        [Fact]
        public void DrawStack_ReaddingDrawablePlacesItLastAmongTies()
        {
            var scene = StartScene();
            var overlays = CreateOrderOverlays(scene, 0, 0, 0);
            var drawable = (BoxDrawable)overlays[1].TypeOObject;
            foreach (var overlay in overlays)
                ((BoxDrawable)overlay.TypeOObject).Hidden = true;
            scene.DrawStack.Pop(drawable);
            scene.DrawStack.Draw(null);
            Assert.Equal("AC", RenderOrder(scene, overlays));

            scene.DrawStack.Push(drawable);
            scene.DrawStack.Draw(null);
            Assert.Equal("ACB", RenderOrder(scene, overlays));
        }

        [Fact]
        public void DrawOrderPlanner_DoesNotChangeReadOnlyTarget()
        {
            var scene = StartScene();
            var overlays = CreateOrderOverlays(scene, 0, 1);
            overlays[0].Component.ReadOnly = true;
            Assert.Null(PlanOrder(overlays[0].Component, overlays, "Forward"));
        }

        [Fact]
        public void ChangePropertyCommand_AdditionalChangesUndoAndRedoTogether()
        {
            var scene = StartScene();
            var overlays = CreateOrderOverlays(scene, 0, 0, 0, 0);
            var changes = PlanOrder(overlays[1].Component, overlays, "Forward");
            Assert.True(changes.Count > 1);
            var historyType = typeof(Project).Assembly.GetType("TypeD.Models.CommandHistoryModel");
            var history = (ICommandHistoryModel)Activator.CreateInstance(historyType, true);
            var saveContext = new ComponentSaveContext { Components = overlays.Select(overlay => overlay.Component).ToList() };
            var services = new Dictionary<Type, object>
            {
                [typeof(ICommandHistoryModel)] = history,
                [typeof(ISaveModel)] = MakeProxy<ISaveModel>((method, _) => method.Name == "GetSaveContext" ? saveContext : null),
                [typeof(IHookModel)] = MakeProxy<IHookModel>((_, args) => args.FirstOrDefault()),
                [typeof(IEditorCommandRegistry)] = MakeProxy<IEditorCommandRegistry>((_, _) => null)
            };
            var resources = MakeProxy<IResourceModel>((method, _) =>
                method.IsGenericMethod && services.TryGetValue(method.GetGenericArguments()[0], out var service) ? service : null);
            var commandType = typeof(TypeDCore.Models.Data.Hooks.ViewerHitTestHook).Assembly.GetType("TypeDCore.Commands.ChangePropertyCommand");
            var command = (CustomCommand)Activator.CreateInstance(commandType, resources);
            var first = changes[0];
            first.AdditionalChanges = changes.Skip(1).ToList();
            var before = changes.Select(change => change.Property.Value).ToArray();

            Assert.True(command.CanExecute(first));
            var additional = first.AdditionalChanges;
            first.AdditionalChanges = new[] { new ChangePropertyCommandData
            {
                Project = first.Project, Component = first.Component, Property = first.Property, Value = first.Value
            } };
            Assert.False(command.CanExecute(first));
            first.AdditionalChanges = additional;
            command.Execute(first);
            Assert.Equal(changes.Select(change => change.Value), changes.Select(change => change.Property.Value));
            Assert.Single(history.Entries);
            Assert.True(history.Undo());
            Assert.Equal(before, changes.Select(change => change.Property.Value));
            Assert.True(history.Redo());
            Assert.Equal(changes.Select(change => change.Value), changes.Select(change => change.Property.Value));
        }

        [Theory]
        [InlineData(false, "_Forward", 1, "ACBD")]
        [InlineData(false, "_Front", 1, "ACDB")]
        [InlineData(false, "_Backward", 2, "ACBD")]
        [InlineData(false, "_Back", 2, "CABD")]
        [InlineData(true, "_Forward", 1, "ACBD")]
        [InlineData(true, "_Front", 1, "ACDB")]
        [InlineData(true, "_Backward", 2, "ACBD")]
        [InlineData(true, "_Back", 2, "CABD")]
        public void DrawOrderMenu_ExecutesForExplorerAndViewer(bool viewer, string action, int selected, string expected)
        {
            var scene = StartScene();
            var overlays = CreateOrderOverlays(scene, 0, 0, 0, 0);
            var component = overlays[selected].Component;
            component.BaseInheritedComponent = Basic2dComponent.DrawableRectangleComponent();
            var project = PlanOrder(component, overlays, "Front")?[0].Project
                ?? PlanOrder(component, overlays, "Back")[0].Project;
            var initializerType = typeof(Debug2DDrawable).Assembly.GetType("TypeDBasic2d.TypeDBasic2dInitializer");
            var initializer = Activator.CreateInstance(initializerType, true);
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            initializerType.GetProperty("DebugDrawablesMap", flags).SetValue(initializer,
                new ConcurrentDictionary<string, Debug2DDrawable>(overlays.Select((overlay, i) =>
                    new KeyValuePair<string, Debug2DDrawable>(i.ToString(), overlay))));
            ChangePropertyCommandData submitted = null;
            initializerType.GetProperty("EditorCommandRegistry", flags).SetValue(initializer,
                MakeProxy<IEditorCommandRegistry>((method, args) =>
                {
                    if (method.Name != "Execute")
                        throw new InvalidOperationException(method.Name);
                    Assert.Equal("component.change-property", args[0]);
                    submitted = (ChangePropertyCommandData)args[1];
                    return true;
                }));
            ComponentContextMenuHook hook = viewer ? new ViewerComponentContextMenuHook() : new ComponentContextMenuHook();
            hook.Project = project;
            hook.SelectedComponent = component;
            hook.Menu = new TypeD.View.Menu();
            initializerType.GetMethod("ComponentContextMenuOpened", flags).Invoke(initializer, new object[] { hook });

            var submenu = Assert.Single(hook.Menu.Items, item => item.Name == "_Send to...");
            Assert.Equal(new[] { "_Forward", "_Front", "_Backward", "_Back" }, submenu.Items.Select(item => item.Name));
            var item = Assert.Single(submenu.Items, item => item.Name == action);
            Assert.True(item.IsEnabled);
            item.Click(null);
            Assert.NotNull(submitted);
            ApplyOrderChanges(overlays, new[] { submitted }.Concat(submitted.AdditionalChanges).ToList());
            Assert.Equal(expected, RenderOrder(scene, overlays));
        }

        [Theory]
        [InlineData("Drawable2d", false)]
        [InlineData("DrawableRectangle", true)]
        [InlineData("DrawableCircle", false)]
        [InlineData("DrawableTriangle", true)]
        [InlineData("DrawablePolygon", false)]
        [InlineData("DrawableTexture", true)]
        [InlineData("DrawableFont", false)]
        public void DrawOrderMenu_RecognizesBuiltInDrawableMetadataWithoutViewer(string name, bool viewer)
        {
            var definition = name switch
            {
                "Drawable2d" => Basic2dComponent.Drawable2dComponent(),
                "DrawableRectangle" => Basic2dComponent.DrawableRectangleComponent(),
                "DrawableCircle" => Basic2dComponent.DrawableCircleComponent(),
                "DrawableTriangle" => Basic2dComponent.DrawableTriangleComponent(),
                "DrawablePolygon" => Basic2dComponent.DrawablePolygonComponent(),
                "DrawableTexture" => Basic2dComponent.DrawableTextureComponent(),
                "DrawableFont" => Basic2dComponent.DrawableFontComponent(),
                _ => throw new ArgumentOutOfRangeException(nameof(name))
            };
            var component = new Component { ClassName = "CustomDrawable", BaseInheritedComponent = definition };
            var initializerType = typeof(Debug2DDrawable).Assembly.GetType("TypeDBasic2d.TypeDBasic2dInitializer");
            var initializer = Activator.CreateInstance(initializerType, true);
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            initializerType.GetProperty("DebugDrawablesMap", flags).SetValue(initializer,
                new ConcurrentDictionary<string, Debug2DDrawable>());
            ComponentContextMenuHook hook = viewer ? new ViewerComponentContextMenuHook() : new ComponentContextMenuHook();
            hook.SelectedComponent = component;
            hook.Menu = new TypeD.View.Menu();
            initializerType.GetMethod("ComponentContextMenuOpened", flags).Invoke(initializer, new object[] { hook });

            var submenu = Assert.Single(hook.Menu.Items, item => item.Name == "_Send to...");
            Assert.All(submenu.Items, item => Assert.False(item.IsEnabled));
        }

        [Fact]
        public void DrawOrderMenu_DoesNotAppearForEntity()
        {
            var initializerType = typeof(Debug2DDrawable).Assembly.GetType("TypeDBasic2d.TypeDBasic2dInitializer");
            var initializer = Activator.CreateInstance(initializerType, true);
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            initializerType.GetProperty("DebugDrawablesMap", flags).SetValue(initializer,
                new ConcurrentDictionary<string, Debug2DDrawable>());
            var hook = new ComponentContextMenuHook
            {
                SelectedComponent = new Component { ClassName = "CustomEntity", BaseInheritedComponent = Basic2dComponent.Entity2dComponent() },
                Menu = new TypeD.View.Menu()
            };
            initializerType.GetMethod("ComponentContextMenuOpened", flags).Invoke(initializer, new object[] { hook });
            Assert.DoesNotContain(hook.Menu.Items, item => item.Name == "_Send to...");
        }

        private static string RenderOrder(BoxScene scene, List<Debug2DDrawable> overlays) =>
            string.Concat(scene.DrawStack.GetDrawablesInDrawOrder()
                .Select(drawable => overlays.Single(overlay => ReferenceEquals(overlay.TypeOObject, drawable)).Component.ClassName));

        public class TestInterfaceProxy : DispatchProxy
        {
            public Func<MethodInfo, object[], object> Handler { get; set; }
            protected override object Invoke(MethodInfo targetMethod, object[] args) => Handler(targetMethod, args);
        }

        private static T MakeProxy<T>(Func<MethodInfo, object[], object> handler) where T : class
        {
            var proxy = DispatchProxy.Create<T, TestInterfaceProxy>();
            ((TestInterfaceProxy)(object)proxy).Handler = handler;
            return proxy;
        }

        private static List<Debug2DDrawable> CreateOrderOverlays(BoxScene scene, params int[] orders)
        {
            var overlays = new List<Debug2DDrawable>();
            for (var i = 0; i < orders.Length; i++)
            {
                var drawable = scene.Drawables.Create<BoxDrawable>();
                drawable.Size = new Vec2(10, 10);
                drawable.DrawOrder = orders[i];
                overlays.Add(new Debug2DDrawable { TypeOObject = drawable, Component = OrderComponent(((char)('A' + i)).ToString(), orders[i]) });
            }
            scene.DrawStack.Draw(null);
            return overlays;
        }

        private static Component OrderComponent(string name, int order)
        {
            var component = new Component { ClassName = name };
            component.Properties.Add(new Property { Name = "DrawOrder", Type = typeof(int), Value = order });
            return component;
        }

        private static List<ChangePropertyCommandData> PlanOrder(Component component, List<Debug2DDrawable> overlays, string move)
        {
            var assembly = typeof(Debug2DDrawable).Assembly;
            var planner = assembly.GetType("TypeDBasic2d.Typedeaf.Core.Entities.Drawables.DrawOrderPlanner");
            var moveType = assembly.GetType("TypeDBasic2d.Typedeaf.Core.Entities.Drawables.DrawOrderMove");
            var project = CreateTestProject();
            return (List<ChangePropertyCommandData>)planner.GetMethod("Plan", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, new object[] { project, component, overlays, Enum.Parse(moveType, move) });
        }

        private static Project CreateTestProject()
        {
            var constructor = typeof(Project).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single();
            var dto = System.Text.Json.JsonSerializer.Deserialize("{\"ProjectName\":\"OrderTest\"}", constructor.GetParameters()[1].ParameterType);
            return (Project)constructor.Invoke(new[] { Path.GetTempPath(), dto });
        }

        private static void ApplyOrderChanges(List<Debug2DDrawable> overlays, List<ChangePropertyCommandData> changes)
        {
            foreach (var change in changes)
            {
                change.Property.Value = change.Value;
                var overlay = overlays.Single(item => ReferenceEquals(item.Component, change.Component));
                ((BoxDrawable)overlay.TypeOObject).DrawOrder = (int)change.Value;
            }
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
