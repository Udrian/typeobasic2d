using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenTK.Windowing.GraphicsLibraryFramework;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.Models.Data;
using TypeDCore.Models.Data.Hooks;
using TypeDCore.Models.Interfaces;
using TypeOEngine.Typedeaf.Basic2d.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Desktop.Engine.Services;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;

namespace TypeDBasic2d.Typedeaf.Core.Entities.Drawables
{
    public class Debug2DDrawable : Drawable2d
    {
        [Flags]
        private enum ResizeAxes
        {
            None = 0,
            Left = 1,
            Right = 2,
            Top = 4,
            Bottom = 8
        }

        private const double GizmoLength = 48;
        private const double HandleRadius = 8;
        private const double MinimumSize = 1;
        private static readonly object PointerSync = new();
        private static readonly ConditionalWeakTable<MouseInputService, object> MouseServicesWithLeftButtonAlias = new();
        private static Debug2DDrawable PointerOwner;

        public override Vec2 Size { get; set; }
        private List<Vec2> Lines { get; set; }

        public bool Focused { get; set; }
        public MouseInputService MouseInputService { get; set; }
        public TypeOObject TypeOObject { get; set; }
        public Component Component { get; set; }
        public Project Project { get; set; }
        public IHookModel HookModel { get; set; }
        public IEditorCommandRegistry EditorCommandRegistry { get; set; }
        public IEditorToolModel EditorToolModel { get; set; }

        private Vec2 DragStartMouse { get; set; }
        private Vec2 DragStartPosition { get; set; }
        private Vec2 DragStartSize { get; set; }
        private bool IsDragging { get; set; }
        private EditorTool DragTool { get; set; }
        private bool IsDraggingX { get; set; }
        private bool IsDraggingY { get; set; }
        private ResizeAxes DragResizeAxes { get; set; }

        public override void Draw(ICanvas canvas)
        {
            if (MouseInputService == null)
                return;

            MouseServicesWithLeftButtonAlias.GetValue(
                MouseInputService,
                service =>
                {
                    service.SetKeyAlias(MouseButton.Left, MouseButton.Left);
                    service.SetKeyAlias(MouseButton.Right, MouseButton.Right);
                    return new object();
                });

            var mousePos = MouseInputService.MousePosition;
            var pos = GetPos();
            var isHovered = Contains(pos, Size, mousePos);
            ProcessPointer(mousePos, pos, isHovered);

            var drawColor = Focused ? Color.Green : Color.Red;
            if (isHovered)
                drawColor = Color.MarineBlue;

            UpdateLines();
            canvas.DrawLines(Lines, drawColor);

            if (!Focused || EditorToolModel == null)
                return;

            if (EditorToolModel.ActiveTool == EditorTool.Move)
                DrawMoveGizmo(canvas, pos, Size);
            else if (EditorToolModel.ActiveTool == EditorTool.Resize)
                DrawResizeHandles(canvas, pos, Size);
        }

        protected override void Cleanup()
        {
            ReleasePointer();
        }

        protected override void Initialize()
        {
            DrawOrder = int.MaxValue;
            Lines = new List<Vec2> { new(), new(), new(), new(), new() };
            Focused = false;
            UpdateLines();
        }

        public Vec2 ScreenPosition => GetPos();

        public bool ContainsPoint(Vec2 point) => Contains(GetPos(), Size, point);

        public void UpdateLines()
        {
            if (Lines == null)
                return;

            var pos = GetPos();
            Lines[0] = pos;
            Lines[1] = pos + new Vec2(Size.X, 0);
            Lines[2] = pos + Size;
            Lines[3] = pos + new Vec2(0, Size.Y);
            Lines[4] = pos;
        }

        private void ProcessPointer(Vec2 mousePos, Vec2 boundsPos, bool isHovered)
        {
            if (IsDragging)
            {
                if (!MouseInputService.IsDown(MouseButton.Left))
                {
                    CommitChanges();
                    ReleasePointer();
                }
                else
                {
                    UpdateDrag(mousePos);
                }
                return;
            }

            if (isHovered && MouseInputService.IsPressed(MouseButton.Right))
            {
                SelectComponent();
                HookModel?.Shoot(new ViewerContextMenuRequestedHook { Project = Project, Component = Component });
                return;
            }

            if (!MouseInputService.IsPressed(MouseButton.Left))
                return;

            var tool = EditorToolModel?.ActiveTool ?? EditorTool.Selection;
            var isDraggingX = false;
            var isDraggingY = false;
            var resizeAxes = ResizeAxes.None;
            if (tool == EditorTool.Move && Focused)
            {
                var center = boundsPos + (Size / 2);
                var xEnd = center + new Vec2(GizmoLength, 0);
                var yEnd = center + new Vec2(0, -GizmoLength);
                isDraggingX = DistanceToSegment(mousePos, center, xEnd) <= HandleRadius;
                isDraggingY = !isDraggingX && DistanceToSegment(mousePos, center, yEnd) <= HandleRadius;
            }
            else if (tool == EditorTool.Resize)
            {
                TryGetResizeAxes(mousePos, boundsPos, Size, out resizeAxes);
            }

            if (!isHovered && !isDraggingX && !isDraggingY && resizeAxes == ResizeAxes.None)
                return;

            SelectComponent();
            if (tool == EditorTool.Selection || Component?.IsVisible == false)
                return;

            if (!TryCapturePointer())
                return;

            IsDragging = true;
            DragTool = tool;
            IsDraggingX = isDraggingX;
            IsDraggingY = isDraggingY;
            DragResizeAxes = resizeAxes;
            DragStartMouse = mousePos;
            DragStartPosition = GetObjectPosition();
            DragStartSize = GetObjectSize();
        }

        private void UpdateDrag(Vec2 mousePos)
        {
            var delta = mousePos - DragStartMouse;

            if (DragTool == EditorTool.Move)
            {
                var position = DragStartPosition;
                if (!IsDraggingX && !IsDraggingY)
                    position += delta;
                else if (IsDraggingX)
                    position.X += delta.X;
                else
                    position.Y += delta.Y;

                SetObjectPosition(position);
            }
            else if (DragTool == EditorTool.Resize)
            {
                var position = DragStartPosition;
                var size = DragStartSize;

                if (DragResizeAxes.HasFlag(ResizeAxes.Left))
                {
                    var width = Math.Max(MinimumSize, DragStartSize.X - delta.X);
                    position.X = DragStartPosition.X + DragStartSize.X - width;
                    size.X = width;
                }
                else if (DragResizeAxes.HasFlag(ResizeAxes.Right))
                {
                    size.X = Math.Max(MinimumSize, DragStartSize.X + delta.X);
                }

                if (DragResizeAxes.HasFlag(ResizeAxes.Top))
                {
                    var height = Math.Max(MinimumSize, DragStartSize.Y - delta.Y);
                    position.Y = DragStartPosition.Y + DragStartSize.Y - height;
                    size.Y = height;
                }
                else if (DragResizeAxes.HasFlag(ResizeAxes.Bottom))
                {
                    size.Y = Math.Max(MinimumSize, DragStartSize.Y + delta.Y);
                }

                SetObjectPosition(position);
                SetObjectSize(size);
            }

            UpdateLines();
        }

        private void CommitChanges()
        {
            var position = GetObjectPosition();
            var size = GetObjectSize();
            if (position != DragStartPosition)
                ExecutePropertyChange("Position", position);
            if (size != DragStartSize)
                ExecutePropertyChange("Size", size);
        }

        private void ExecutePropertyChange(string propertyName, Vec2 value)
        {
            var property = Component?.Properties.FirstOrDefault(candidate => candidate.Name == propertyName);
            if (property == null)
                return;

            var executed = EditorCommandRegistry?.Execute(
                "component.change-property",
                new ChangePropertyCommandData
                {
                    Project = Project,
                    Property = property,
                    Value = value
                });
            if (executed != true)
                throw new InvalidOperationException($"Could not apply the {propertyName} change for '{Component.FullName}'.");
        }

        private void SelectComponent()
        {
            var target = Component;
            while (target != null && !target.IsVisible)
                target = target.ParentComponent;

            HookModel?.Shoot(new ComponentFocusHook
            {
                Project = Project,
                Component = target ?? Component,
                Root = false
            });
        }

        private bool TryCapturePointer()
        {
            lock (PointerSync)
            {
                if (PointerOwner != null)
                    return false;

                if (EditorToolModel != null && !EditorToolModel.TryCapturePointer(this))
                    return false;

                PointerOwner = this;
                return true;
            }
        }

        private void ReleasePointer()
        {
            if (!IsDragging && !ReferenceEquals(PointerOwner, this))
                return;

            IsDragging = false;
            IsDraggingX = false;
            IsDraggingY = false;
            DragResizeAxes = ResizeAxes.None;
            EditorToolModel?.ReleasePointer(this);
            lock (PointerSync)
            {
                if (ReferenceEquals(PointerOwner, this))
                    PointerOwner = null;
            }
        }

        private void DrawMoveGizmo(ICanvas canvas, Vec2 pos, Vec2 size)
        {
            var center = pos + (size / 2);
            DrawArrow(canvas, center, center + new Vec2(GizmoLength, 0), Color.Green);
            DrawArrow(canvas, center, center + new Vec2(0, -GizmoLength), Color.Red);
        }

        private static void DrawArrow(ICanvas canvas, Vec2 start, Vec2 end, Color color)
        {
            canvas.DrawLineA(start, end, color);
            var direction = (end - start).Normalize();
            var perpendicular = new Vec2(-direction.Y, direction.X);
            var arrowBase = end - direction * 9;
            canvas.DrawLineA(end, arrowBase + perpendicular * 5, color);
            canvas.DrawLineA(end, arrowBase - perpendicular * 5, color);
        }

        private static void DrawResizeHandles(ICanvas canvas, Vec2 pos, Vec2 size)
        {
            var handles = new[]
            {
                pos,
                pos + new Vec2(size.X / 2, 0),
                pos + new Vec2(size.X, 0),
                pos + new Vec2(size.X, size.Y / 2),
                pos + size,
                pos + new Vec2(size.X / 2, size.Y),
                pos + new Vec2(0, size.Y),
                pos + new Vec2(0, size.Y / 2)
            };

            foreach (var handle in handles)
            {
                var topLeft = handle - new Vec2(3, 3);
                var bottomRight = handle + new Vec2(3, 3);
                canvas.DrawLineA(topLeft, new Vec2(bottomRight.X, topLeft.Y), Color.White);
                canvas.DrawLineA(new Vec2(bottomRight.X, topLeft.Y), bottomRight, Color.White);
                canvas.DrawLineA(bottomRight, new Vec2(topLeft.X, bottomRight.Y), Color.White);
                canvas.DrawLineA(new Vec2(topLeft.X, bottomRight.Y), topLeft, Color.White);
            }
        }

        private static bool TryGetResizeAxes(Vec2 mousePos, Vec2 pos, Vec2 size, out ResizeAxes axes)
        {
            axes = ResizeAxes.None;
            var left = Math.Abs(mousePos.X - pos.X) <= HandleRadius;
            var right = Math.Abs(mousePos.X - (pos.X + size.X)) <= HandleRadius;
            var top = Math.Abs(mousePos.Y - pos.Y) <= HandleRadius;
            var bottom = Math.Abs(mousePos.Y - (pos.Y + size.Y)) <= HandleRadius;

            if (left && mousePos.Y >= pos.Y - HandleRadius && mousePos.Y <= pos.Y + size.Y + HandleRadius)
                axes |= ResizeAxes.Left;
            else if (right && mousePos.Y >= pos.Y - HandleRadius && mousePos.Y <= pos.Y + size.Y + HandleRadius)
                axes |= ResizeAxes.Right;

            if (top && mousePos.X >= pos.X - HandleRadius && mousePos.X <= pos.X + size.X + HandleRadius)
                axes |= ResizeAxes.Top;
            else if (bottom && mousePos.X >= pos.X - HandleRadius && mousePos.X <= pos.X + size.X + HandleRadius)
                axes |= ResizeAxes.Bottom;

            return axes != ResizeAxes.None;
        }

        private static bool Contains(Vec2 pos, Vec2 size, Vec2 point)
        {
            return point.X >= pos.X && point.X <= pos.X + size.X
                && point.Y >= pos.Y && point.Y <= pos.Y + size.Y;
        }

        private static double DistanceToSegment(Vec2 point, Vec2 start, Vec2 end)
        {
            var segment = end - start;
            var lengthSquared = segment.LengthSquared();
            if (lengthSquared <= 0)
                return point.Distance(start);

            var amount = Math.Max(0, Math.Min(1, (point - start).Dot(segment) / lengthSquared));
            return point.Distance(start + segment * amount);
        }

        private Vec2 GetObjectPosition()
        {
            if (TypeOObject is Entity2d entity)
                return entity.Position;
            if (TypeOObject is Drawable2d drawable)
                return drawable.Position;
            return Position;
        }

        private Vec2 GetObjectSize()
        {
            if (TypeOObject is Entity2d entity)
                return entity.Size;
            if (TypeOObject is Drawable2d drawable)
                return drawable.Size;
            return Size;
        }

        private void SetObjectPosition(Vec2 position)
        {
            if (TypeOObject is Entity2d entity)
                entity.Position = position;
            else if (TypeOObject is Drawable2d drawable)
                drawable.Position = position;
            Position = position;
        }

        private void SetObjectSize(Vec2 size)
        {
            if (TypeOObject is Entity2d entity)
                entity.Size = size;
            else if (TypeOObject is Drawable2d drawable)
                drawable.Size = size;
            Size = size;
        }

        private Vec2 GetPos()
        {
            if (TypeOObject is Entity2d entity)
                return entity.ScreenBounds.Pos;
            if (TypeOObject is Drawable2d drawable)
            {
                var parentPosition = (drawable.Parent as Entity2d)?.ScreenBounds.Pos ?? Vec2.Zero;
                return drawable.Position + parentPosition;
            }

            return Position;
        }
    }
}
