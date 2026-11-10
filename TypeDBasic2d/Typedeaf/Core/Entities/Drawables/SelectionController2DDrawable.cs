using OpenTK.Windowing.GraphicsLibraryFramework;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Interfaces;
using TypeDCore.Models.Data;
using TypeDCore.Models.Data.Hooks;
using TypeDCore.Models.Interfaces;
using TypeOEngine.Typedeaf.Basic2d.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Desktop.Engine.Services;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;

namespace TypeDBasic2d.Typedeaf.Core.Entities.Drawables
{
    /// <summary>
    /// Handles all pointer interaction of the 2D editor overlays: picking the topmost node under the cursor,
    /// click and Ctrl+Click selection, drag selection box, and moving or resizing the selected nodes together,
    /// keeping their relative positions and sizes. It also draws the gizmos of the active tool and the combined
    /// bounds when multiple nodes are selected.
    /// </summary>
    public class SelectionController2DDrawable : Drawable2d
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

        private sealed class DragItem
        {
            public Debug2DDrawable Drawable { get; init; }
            public Vec2 StartPosition { get; init; }
            public Vec2 StartSize { get; init; }
            public Vec2 StartBoundsPos { get; init; }
        }

        private const double GizmoLength = 48;
        private const double HandleRadius = 8;
        private const double MinimumSize = 1;

        /// <summary>
        /// Gets or sets the mouse input service used to read pointer state.
        /// </summary>
        public MouseInputService MouseInputService { get; set; }

        /// <summary>
        /// Gets or sets the project displayed in the viewer.
        /// </summary>
        public Project Project { get; set; }

        /// <summary>
        /// Gets or sets the hook model used to change the selection and apply changes.
        /// </summary>
        public IHookModel HookModel { get; set; }

        /// <summary>
        /// Gets or sets the registry used to execute editor commands.
        /// </summary>
        public IEditorCommandRegistry EditorCommandRegistry { get; set; }

        /// <summary>
        /// Gets or sets the model providing the active editor tool and the display settings.
        /// </summary>
        public IEditorToolModel EditorToolModel { get; set; }

        /// <summary>
        /// Gets or sets the delegate that lists the editor overlays, in hierarchy order (parents before children).
        /// </summary>
        public Func<IReadOnlyList<Debug2DDrawable>> GetDebugDrawables { get; set; }

        private bool AliasesSet { get; set; }
        private double UnitsPerPixel { get; set; } = 1;

        private bool IsBoxSelecting { get; set; }
        private Vec2 BoxStart { get; set; }
        private bool BoxAddsToSelection { get; set; }

        private bool IsDragging { get; set; }
        private EditorTool DragTool { get; set; }
        private Vec2 DragStartMouse { get; set; }
        private bool IsDraggingX { get; set; }
        private bool IsDraggingY { get; set; }
        private ResizeAxes DragResizeAxes { get; set; }
        private (Vec2 Pos, Vec2 Size) DragStartCombined { get; set; }
        private List<DragItem> DragItems { get; set; } = new();
        private Vec2 DragStartOrigin { get; set; }
        private Vec2 PreviewOrigin { get; set; }

        /// <summary>
        /// Draws this controller above everything else.
        /// </summary>
        protected override void Initialize()
        {
            DrawOrder = int.MaxValue;
        }

        /// <summary>
        /// Releases pointer ownership held by this controller.
        /// </summary>
        protected override void Cleanup()
        {
            ReleasePointer();
        }

        /// <summary>
        /// Processes pointer interaction and draws the selection box, the combined selection bounds and the tool gizmos.
        /// </summary>
        /// <param name="canvas">The canvas to draw on.</param>
        public override void Draw(ICanvas canvas)
        {
            if (MouseInputService == null || EditorToolModel == null || GetDebugDrawables == null)
                return;

            if (!AliasesSet)
            {
                MouseInputService.SetKeyAlias(MouseButton.Left, MouseButton.Left);
                MouseInputService.SetKeyAlias(MouseButton.Right, MouseButton.Right);
                AliasesSet = true;
            }

            // Gizmos keep the same size on screen at any zoom of the editor camera
            UnitsPerPixel = canvas.WorldScale > 0 ? 1 / canvas.WorldScale : 1;

            var drawables = GetDebugDrawables();
            var mousePos = MouseInputService.MousePosition;

            // Only the topmost node under the cursor is hovered
            var topmost = IsDragging || IsBoxSelecting ? null : PickTopMost(drawables, mousePos);
            foreach (var drawable in drawables)
                drawable.Hovered = ReferenceEquals(drawable, topmost);

            ProcessPointer(drawables, topmost, mousePos);

            var display = EditorToolModel;
            if (IsBoxSelecting)
                DrawRectangle(canvas, Normalized(BoxStart, mousePos), Color.White);

            // The selection bounding box and the tool gizmos stay visible even when bounding boxes are hidden
            var selected = drawables.Where(d => d.Focused).ToList();
            if (selected.Count == 0)
                return;

            var combined = CombinedBounds(selected);
            if (selected.Count > 1)
                DrawRectangle(canvas, combined, display.SelectedBoundingBoxColor);

            var tool = display.ActiveTool;
            if (tool == EditorTool.Move)
                DrawMoveGizmo(canvas, combined.Pos, combined.Size);
            else if (tool == EditorTool.Resize)
                DrawResizeHandles(canvas, combined.Pos, combined.Size, UnitsPerPixel);
            else if (tool == EditorTool.Origin && selected.Count == 1 && selected[0].TypeOObject is Drawable2d)
                DrawOriginGizmo(canvas, selected[0]);
        }

        /// <summary>
        /// Picks the node to select at a point: the hit node with the highest draw order wins; nodes without a
        /// draw order (entities) fall back to the hierarchy, where the innermost node wins.
        /// </summary>
        /// <param name="drawables">The editor overlays in hierarchy order (parents before children).</param>
        /// <param name="point">The world point to test.</param>
        /// <returns>The topmost overlay at the point, or <c>null</c> if there is none.</returns>
        public static Debug2DDrawable PickTopMost(IEnumerable<Debug2DDrawable> drawables, Vec2 point)
        {
            Debug2DDrawable best = null;
            var bestHasOrder = false;
            var bestOrder = int.MinValue;

            foreach (var drawable in drawables)
            {
                if (drawable.Component == null || !drawable.ContainsPoint(point))
                    continue;

                var hasOrder = drawable.TypeOObject is Drawable2d;
                var order = hasOrder ? ((Drawable2d)drawable.TypeOObject).DrawOrder : 0;

                // Later entries are deeper in the hierarchy, so >= makes the innermost win on equal draw order
                if (best == null
                    || (hasOrder && (!bestHasOrder || order >= bestOrder))
                    || (!hasOrder && !bestHasOrder))
                {
                    best = drawable;
                    bestHasOrder = hasOrder;
                    bestOrder = order;
                }
            }

            return best;
        }

        private void ProcessPointer(IReadOnlyList<Debug2DDrawable> drawables, Debug2DDrawable topmost, Vec2 mousePos)
        {
            if (IsDragging)
            {
                if (!MouseInputService.IsDown(MouseButton.Left))
                {
                    CommitDrag();
                    ReleasePointer();
                }
                else
                {
                    UpdateDrag(mousePos);
                }
                return;
            }

            if (IsBoxSelecting)
            {
                if (!MouseInputService.IsDown(MouseButton.Left))
                {
                    CommitBoxSelection(drawables, mousePos);
                    ReleasePointer();
                }
                return;
            }

            if (topmost != null && MouseInputService.IsPressed(MouseButton.Right))
            {
                if (!topmost.Focused)
                    Select(new[] { topmost }, topmost);
                HookModel?.Shoot(new ViewerContextMenuRequestedHook { Project = Project, Component = VisibleComponent(topmost.Component) ?? topmost.Component });
                return;
            }

            if (!MouseInputService.IsPressed(MouseButton.Left))
                return;

            var selected = drawables.Where(d => d.Focused).ToList();
            var tool = EditorToolModel.ActiveTool;

            // Ctrl+Click adds to or removes from the selection with any tool
            if (EditorToolModel.MultiSelectModifierDown)
            {
                if (topmost != null)
                    ToggleSelection(drawables, selected, topmost);
                else if (tool == EditorTool.Selection)
                    BeginBoxSelection(mousePos, addToSelection: true);
                return;
            }

            if (tool == EditorTool.Selection)
            {
                if (topmost != null)
                    Select(new[] { topmost }, topmost);
                else
                    BeginBoxSelection(mousePos, addToSelection: false);
            }
            else if (tool == EditorTool.Move)
            {
                var isDraggingX = false;
                var isDraggingY = false;
                var insideSelection = false;
                if (selected.Count > 0)
                {
                    var combined = CombinedBounds(selected);
                    var center = combined.Pos + (combined.Size / 2);
                    var xEnd = center + new Vec2(GizmoLength * UnitsPerPixel, 0);
                    var yEnd = center + new Vec2(0, -GizmoLength * UnitsPerPixel);
                    isDraggingX = DistanceToSegment(mousePos, center, xEnd) <= HandleRadius * UnitsPerPixel;
                    isDraggingY = !isDraggingX && DistanceToSegment(mousePos, center, yEnd) <= HandleRadius * UnitsPerPixel;
                    // Anywhere inside the combined selection bounds starts moving the selection
                    insideSelection = mousePos.X >= combined.Pos.X && mousePos.X <= combined.Pos.X + combined.Size.X
                        && mousePos.Y >= combined.Pos.Y && mousePos.Y <= combined.Pos.Y + combined.Size.Y;
                }

                if (isDraggingX || isDraggingY || insideSelection)
                {
                    BeginDrag(EditorTool.Move, selected, mousePos, isDraggingX, isDraggingY, ResizeAxes.None);
                }
                else if (topmost != null)
                {
                    Select(new[] { topmost }, topmost);
                    BeginDrag(EditorTool.Move, new List<Debug2DDrawable> { topmost }, mousePos, false, false, ResizeAxes.None);
                }
            }
            else if (tool == EditorTool.Resize)
            {
                if (selected.Count > 0)
                {
                    var combined = CombinedBounds(selected);
                    if (TryGetResizeAxes(mousePos, combined.Pos, combined.Size, HandleRadius * UnitsPerPixel, out var axes))
                    {
                        BeginDrag(EditorTool.Resize, selected, mousePos, false, false, axes);
                        return;
                    }
                }

                if (topmost != null)
                {
                    Select(new[] { topmost }, topmost);
                    var (pos, size) = topmost.GetBounds();
                    if (TryGetResizeAxes(mousePos, pos, size, HandleRadius * UnitsPerPixel, out var nodeAxes))
                        BeginDrag(EditorTool.Resize, new List<Debug2DDrawable> { topmost }, mousePos, false, false, nodeAxes);
                }
            }
            else if (tool == EditorTool.Origin)
            {
                if (selected.Count == 1 && selected[0].TypeOObject is Drawable2d && selected[0].ContainsPoint(mousePos))
                    BeginDrag(EditorTool.Origin, selected, mousePos, false, false, ResizeAxes.None);
                else if (topmost != null && !topmost.Focused)
                    Select(new[] { topmost }, topmost);
            }
        }

        private void BeginBoxSelection(Vec2 mousePos, bool addToSelection)
        {
            if (!TryCapturePointer())
                return;

            IsBoxSelecting = true;
            BoxStart = mousePos;
            BoxAddsToSelection = addToSelection;
        }

        private void CommitBoxSelection(IReadOnlyList<Debug2DDrawable> drawables, Vec2 mousePos)
        {
            var box = Normalized(BoxStart, mousePos);
            var hits = drawables.Where(d => d.Component != null && IntersectsBox(box, d.GetBounds())).ToList();
            if (BoxAddsToSelection)
                hits = drawables.Where(d => d.Focused).Concat(hits).Distinct().ToList();

            if (hits.Count > 0)
            {
                Select(hits, hits[^1]);
            }
            else if (!BoxAddsToSelection)
            {
                SelectRoot(drawables);
            }
        }

        private void ToggleSelection(IReadOnlyList<Debug2DDrawable> drawables, List<Debug2DDrawable> selected, Debug2DDrawable topmost)
        {
            var component = VisibleComponent(topmost.Component);
            if (component == null)
                return;

            var components = selected.Select(d => d.Component).Where(c => c != null).Distinct().ToList();
            if (components.Contains(component))
            {
                components.Remove(component);
                if (components.Count == 0)
                {
                    SelectRoot(drawables);
                    return;
                }
                SelectComponents(components, components[^1]);
            }
            else
            {
                components.Add(component);
                SelectComponents(components, component);
            }
        }

        private void Select(IEnumerable<Debug2DDrawable> items, Debug2DDrawable focused)
        {
            var components = items.Select(d => VisibleComponent(d.Component)).Where(c => c != null).Distinct().ToList();
            var focusComponent = VisibleComponent(focused?.Component) ?? components.FirstOrDefault();
            if (focusComponent == null)
                return;
            if (!components.Contains(focusComponent))
                components.Add(focusComponent);
            SelectComponents(components, focusComponent);
        }

        private void SelectComponents(IReadOnlyList<Component> components, Component focused)
        {
            HookModel?.Shoot(new ComponentFocusHook
            {
                Project = Project,
                Component = focused,
                Root = false,
                Selection = components
            });
        }

        // An empty selection falls back to the opened root component, like when the component is opened
        private void SelectRoot(IReadOnlyList<Debug2DDrawable> drawables)
        {
            var component = drawables.Select(d => d.Component).FirstOrDefault(c => c != null);
            if (component == null)
                return;
            while (component.ParentComponent != null)
                component = component.ParentComponent;
            SelectComponents(new[] { component }, component);
        }

        private static Component VisibleComponent(Component component)
        {
            var target = component;
            while (target != null && !target.IsVisible)
                target = target.ParentComponent;
            return target ?? component;
        }

        private void BeginDrag(EditorTool tool, IEnumerable<Debug2DDrawable> items, Vec2 mousePos, bool isDraggingX, bool isDraggingY, ResizeAxes axes)
        {
            var dragItems = items.Where(d => d.Component?.IsVisible != false).ToList();
            if (dragItems.Count == 0)
                return;

            if (!TryCapturePointer())
                return;

            IsDragging = true;
            DragTool = tool;
            DragStartMouse = mousePos;
            IsDraggingX = isDraggingX;
            IsDraggingY = isDraggingY;
            DragResizeAxes = axes;
            DragItems = dragItems.Select(d => new DragItem
            {
                Drawable = d,
                StartPosition = d.GetObjectPosition(),
                StartSize = d.GetObjectSize(),
                StartBoundsPos = d.GetBounds().Pos
            }).ToList();
            DragStartCombined = CombinedBounds(dragItems);
            DragStartOrigin = dragItems[0].GetObjectOrigin();
            PreviewOrigin = DragStartOrigin;
        }

        private void UpdateDrag(Vec2 mousePos)
        {
            var delta = mousePos - DragStartMouse;

            if (DragTool == EditorTool.Move)
            {
                if (IsDraggingX)
                    delta = new Vec2(delta.X, 0);
                else if (IsDraggingY)
                    delta = new Vec2(0, delta.Y);

                foreach (var item in DragItems)
                    item.Drawable.SetObjectPosition(item.StartPosition + delta);
            }
            else if (DragTool == EditorTool.Resize)
            {
                // Resize the combined bounds, then map every node into the new rectangle so that
                // the nodes keep their relative positions and sizes
                var rectPos = DragStartCombined.Pos;
                var rectSize = DragStartCombined.Size;

                if (DragResizeAxes.HasFlag(ResizeAxes.Left))
                {
                    var width = Math.Max(MinimumSize, DragStartCombined.Size.X - delta.X);
                    rectPos.X = DragStartCombined.Pos.X + DragStartCombined.Size.X - width;
                    rectSize.X = width;
                }
                else if (DragResizeAxes.HasFlag(ResizeAxes.Right))
                {
                    rectSize.X = Math.Max(MinimumSize, DragStartCombined.Size.X + delta.X);
                }

                if (DragResizeAxes.HasFlag(ResizeAxes.Top))
                {
                    var height = Math.Max(MinimumSize, DragStartCombined.Size.Y - delta.Y);
                    rectPos.Y = DragStartCombined.Pos.Y + DragStartCombined.Size.Y - height;
                    rectSize.Y = height;
                }
                else if (DragResizeAxes.HasFlag(ResizeAxes.Bottom))
                {
                    rectSize.Y = Math.Max(MinimumSize, DragStartCombined.Size.Y + delta.Y);
                }

                var scale = new Vec2(
                    DragStartCombined.Size.X > 0 ? rectSize.X / DragStartCombined.Size.X : 1,
                    DragStartCombined.Size.Y > 0 ? rectSize.Y / DragStartCombined.Size.Y : 1);

                foreach (var item in DragItems)
                {
                    var newBoundsPos = rectPos + new Vec2(
                        (item.StartBoundsPos.X - DragStartCombined.Pos.X) * scale.X,
                        (item.StartBoundsPos.Y - DragStartCombined.Pos.Y) * scale.Y);
                    var boundsDelta = newBoundsPos - item.StartBoundsPos;
                    item.Drawable.SetObjectPosition(item.StartPosition + boundsDelta);
                    item.Drawable.SetObjectSize(new Vec2(
                        Math.Max(MinimumSize, item.StartSize.X * scale.X),
                        Math.Max(MinimumSize, item.StartSize.Y * scale.Y)));
                }
            }
            else if (DragTool == EditorTool.Origin && DragItems[0].Drawable.TypeOObject is Drawable2d drawable)
            {
                var boundingBox = drawable.BoundingBox;
                var parentPosition = (drawable.Parent as Entity2d)?.ScreenBounds.Pos ?? Vec2.Zero;
                var screenBoundingBoxPos = boundingBox.Pos + parentPosition;

                // Convert to normalized origin coordinates (0 to 1 is within bounds, can be outside)
                var relativePos = mousePos - screenBoundingBoxPos;
                PreviewOrigin = new Vec2(
                    boundingBox.Size.X > 0 ? relativePos.X / boundingBox.Size.X : 0,
                    boundingBox.Size.Y > 0 ? relativePos.Y / boundingBox.Size.Y : 0);
            }

            foreach (var item in DragItems)
                item.Drawable.UpdateLines();
        }

        private void CommitDrag()
        {
            foreach (var item in DragItems)
            {
                var position = item.Drawable.GetObjectPosition();
                if (position != item.StartPosition)
                    ExecutePropertyChange(item.Drawable.Component, "Position", position);
                var size = item.Drawable.GetObjectSize();
                if (size != item.StartSize)
                    ExecutePropertyChange(item.Drawable.Component, "Size", size);
            }
            if (DragTool == EditorTool.Origin && PreviewOrigin != DragStartOrigin)
                ExecutePropertyChange(DragItems[0].Drawable.Component, "Origin", PreviewOrigin);
        }

        private void ExecutePropertyChange(Component component, string propertyName, Vec2 value)
        {
            var property = component?.Properties.FirstOrDefault(candidate => candidate.Name == propertyName);
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
                throw new InvalidOperationException($"Could not apply the {propertyName} change for '{component.FullName}'.");
        }

        private bool TryCapturePointer()
        {
            return EditorToolModel == null || EditorToolModel.TryCapturePointer(this);
        }

        private void ReleasePointer()
        {
            IsDragging = false;
            IsBoxSelecting = false;
            IsDraggingX = false;
            IsDraggingY = false;
            DragResizeAxes = ResizeAxes.None;
            DragItems = new List<DragItem>();
            EditorToolModel?.ReleasePointer(this);
        }

        private static (Vec2 Pos, Vec2 Size) CombinedBounds(IReadOnlyList<Debug2DDrawable> drawables)
        {
            var min = new Vec2(double.MaxValue, double.MaxValue);
            var max = new Vec2(double.MinValue, double.MinValue);
            foreach (var drawable in drawables)
            {
                var (pos, size) = drawable.GetBounds();
                min = new Vec2(Math.Min(min.X, pos.X), Math.Min(min.Y, pos.Y));
                max = new Vec2(Math.Max(max.X, pos.X + size.X), Math.Max(max.Y, pos.Y + size.Y));
            }
            return (min, max - min);
        }

        private static (Vec2 Pos, Vec2 Size) Normalized(Vec2 start, Vec2 end)
        {
            var pos = new Vec2(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y));
            var size = new Vec2(Math.Abs(end.X - start.X), Math.Abs(end.Y - start.Y));
            return (pos, size);
        }

        private static bool IntersectsBox((Vec2 Pos, Vec2 Size) box, (Vec2 Pos, Vec2 Size) bounds)
        {
            return bounds.Pos.X <= box.Pos.X + box.Size.X && bounds.Pos.X + bounds.Size.X >= box.Pos.X
                && bounds.Pos.Y <= box.Pos.Y + box.Size.Y && bounds.Pos.Y + bounds.Size.Y >= box.Pos.Y;
        }

        private static void DrawRectangle(ICanvas canvas, (Vec2 Pos, Vec2 Size) rectangle, Color color)
        {
            var topLeft = rectangle.Pos;
            var bottomRight = rectangle.Pos + rectangle.Size;
            canvas.DrawLineA(topLeft, new Vec2(bottomRight.X, topLeft.Y), color);
            canvas.DrawLineA(new Vec2(bottomRight.X, topLeft.Y), bottomRight, color);
            canvas.DrawLineA(bottomRight, new Vec2(topLeft.X, bottomRight.Y), color);
            canvas.DrawLineA(new Vec2(topLeft.X, bottomRight.Y), topLeft, color);
        }

        private void DrawMoveGizmo(ICanvas canvas, Vec2 pos, Vec2 size)
        {
            var center = pos + (size / 2);
            var length = GizmoLength * UnitsPerPixel;
            DrawArrow(canvas, center, center + new Vec2(length, 0), Color.Green, UnitsPerPixel);
            DrawArrow(canvas, center, center + new Vec2(0, -length), Color.Red, UnitsPerPixel);
        }

        private static void DrawArrow(ICanvas canvas, Vec2 start, Vec2 end, Color color, double unitsPerPixel)
        {
            canvas.DrawLineA(start, end, color);
            var direction = (end - start).Normalize();
            var perpendicular = new Vec2(-direction.Y, direction.X);
            var arrowBase = end - direction * 9 * unitsPerPixel;
            canvas.DrawLineA(end, arrowBase + perpendicular * 5 * unitsPerPixel, color);
            canvas.DrawLineA(end, arrowBase - perpendicular * 5 * unitsPerPixel, color);
        }

        private static void DrawResizeHandles(ICanvas canvas, Vec2 pos, Vec2 size, double unitsPerPixel)
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
                var halfSize = new Vec2(3 * unitsPerPixel, 3 * unitsPerPixel);
                DrawRectangle(canvas, (handle - halfSize, halfSize * 2), Color.White);
            }
        }

        private void DrawOriginGizmo(ICanvas canvas, Debug2DDrawable selected)
        {
            if (selected.TypeOObject is not Drawable2d drawable)
                return;

            var (pos, size) = selected.GetBounds();
            // Use preview origin during drag, otherwise use the actual origin
            var origin = IsDragging && DragTool == EditorTool.Origin ? PreviewOrigin : drawable.Origin;
            var originScreenPos = pos + (size * origin);

            // Draw a circle around the origin point
            var circleRadius = 12 * UnitsPerPixel;
            DrawCircle(canvas, originScreenPos, circleRadius, Color.Yellow);

            // Draw a cross at the origin point
            var crossSize = 8 * UnitsPerPixel;
            canvas.DrawLineA(originScreenPos - new Vec2(crossSize, 0), originScreenPos + new Vec2(crossSize, 0), Color.Yellow);
            canvas.DrawLineA(originScreenPos - new Vec2(0, crossSize), originScreenPos + new Vec2(0, crossSize), Color.Yellow);
        }

        private static void DrawCircle(ICanvas canvas, Vec2 center, double radius, Color color)
        {
            const int segments = 24;
            var angle = 0.0;
            var angleStep = 2 * Math.PI / segments;
            var previousPoint = center + new Vec2(Math.Cos(angle) * radius, Math.Sin(angle) * radius);

            for (int i = 1; i <= segments; i++)
            {
                angle = i * angleStep;
                var currentPoint = center + new Vec2(Math.Cos(angle) * radius, Math.Sin(angle) * radius);
                canvas.DrawLineA(previousPoint, currentPoint, color);
                previousPoint = currentPoint;
            }
        }

        private static bool TryGetResizeAxes(Vec2 mousePos, Vec2 pos, Vec2 size, double handleRadius, out ResizeAxes axes)
        {
            axes = ResizeAxes.None;
            var left = Math.Abs(mousePos.X - pos.X) <= handleRadius;
            var right = Math.Abs(mousePos.X - (pos.X + size.X)) <= handleRadius;
            var top = Math.Abs(mousePos.Y - pos.Y) <= handleRadius;
            var bottom = Math.Abs(mousePos.Y - (pos.Y + size.Y)) <= handleRadius;

            if (left && mousePos.Y >= pos.Y - handleRadius && mousePos.Y <= pos.Y + size.Y + handleRadius)
                axes |= ResizeAxes.Left;
            else if (right && mousePos.Y >= pos.Y - handleRadius && mousePos.Y <= pos.Y + size.Y + handleRadius)
                axes |= ResizeAxes.Right;

            if (top && mousePos.X >= pos.X - handleRadius && mousePos.X <= pos.X + size.X + handleRadius)
                axes |= ResizeAxes.Top;
            else if (bottom && mousePos.X >= pos.X - handleRadius && mousePos.X <= pos.X + size.X + handleRadius)
                axes |= ResizeAxes.Bottom;

            return axes != ResizeAxes.None;
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
    }
}
