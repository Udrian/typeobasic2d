using OpenTK.Windowing.GraphicsLibraryFramework;
using TypeD.Models.Data;
using TypeDBasic2d.Models.Data;
using TypeDCore.Models.Data;
using TypeDCore.Models.Interfaces;
using TypeD.Models.Interfaces;
using TypeOBasic2d.Typedeaf.Basic2d.Entities.Drawables.Primitives;
using TypeOEngine.Typedeaf.Basic2d.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Desktop.Engine.Services;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;

namespace TypeDBasic2d.Typedeaf.Core.Entities.Drawables
{
    /// <summary>
    /// Viewer overlay for the polygon points editor. While a polygon points session is open it draws the point
    /// handles and lets points be dragged, inserted on edges, appended in add mode and removed with a middle click.
    /// It suspends the selection overlay's pointer handling while the session is open.
    /// </summary>
    public class PolygonEditor2DDrawable : Drawable2d
    {
        private const double HandleSize = 10;
        private const double PickDistance = 8;
        private const int MinimumPoints = 3;

        private PropertyEditorSession _dragSession;
        private int _dragIndex = -1;
        private List<Vec2> _dragPoints;
        private List<PolygonPoint> _dragColors;
        private bool _suspendedSelection;
        private List<PolygonPoint> _normalizedSource;
        private Vec2? _previewPosition;

        /// <summary>Gets or sets the mouse input service used to read pointer state.</summary>
        public MouseInputService MouseInputService { get; set; }

        /// <summary>Gets or sets the project displayed in the viewer.</summary>
        public Project Project { get; set; }

        /// <summary>Gets or sets the model providing pointer capture and the selection suspension.</summary>
        public IEditorToolModel EditorToolModel { get; set; }

        /// <summary>Gets or sets the registry used to execute editor commands.</summary>
        public IEditorCommandRegistry EditorCommandRegistry { get; set; }

        /// <summary>Gets or sets the delegate that returns the open polygon points session, or <c>null</c>.</summary>
        public Func<PropertyEditorSession> GetSession { get; set; }

        /// <summary>Gets or sets the delegate that finds the outline overlay of a component.</summary>
        public Func<Component, Debug2DDrawable> FindOverlay { get; set; }

        /// <inheritdoc/>
        protected override void Initialize()
        {
            DrawOrder = int.MaxValue;
        }

        /// <inheritdoc/>
        protected override void Cleanup()
        {
            EndDrag(false);
            SetSelectionSuspended(false);
        }

        /// <inheritdoc/>
        public override void Draw(ICanvas canvas)
        {
            var session = GetSession?.Invoke();
            var overlay = session == null ? null : FindOverlay?.Invoke(session.Component);
            if (session == null || overlay?.TypeOObject is not DrawablePolygon polygon || MouseInputService == null)
            {
                if (_dragSession != null)
                    EndDrag(false);
                SetSelectionSuspended(false);
                return;
            }

            SetSelectionSuspended(true);
            var state = PolygonPointsPropertyTemplate.GetState(session);
            var unitsPerPixel = canvas.WorldScale > 0 ? 1 / canvas.WorldScale : 1;
            var points = _dragPoints != null && ReferenceEquals(_dragSession, session)
                ? _dragPoints
                : PolygonEditState.GetPositions(session);

            // Box space starts at the bounds' top-left, which lies at BoundsMin in point space
            Vec2 ToWorld(Vec2 point) => overlay.ToEditorWorld(polygon.LocalToWorld(point - polygon.BoundsMin));
            Vec2 ToLocal(Vec2 world) => polygon.WorldToLocal(overlay.FromEditorWorld(world)) + polygon.BoundsMin;

            var mouse = MouseInputService.MousePosition;
            ProcessPointer(session, state, polygon, overlay, points, mouse, unitsPerPixel, ToWorld, ToLocal);

            // Edits made outside the viewer (the panel text boxes) can also push points into negative space
            if (_dragPoints == null && session.Value is List<PolygonPoint> external && !ReferenceEquals(external, _normalizedSource)
                && polygon.TryNormalize(DrawablePolygon.GetPositions(external), out _, out _))
            {
                _normalizedSource = external;
                Commit(session, DrawablePolygon.GetPositions(external));
                return;
            }

            points = _dragPoints != null && ReferenceEquals(_dragSession, session) ? _dragPoints : points;
            DrawHandles(canvas, state, points, unitsPerPixel, ToWorld);
        }

        private void ProcessPointer(PropertyEditorSession session, PolygonEditState state, DrawablePolygon polygon, Debug2DDrawable overlay,
            List<Vec2> points, Vec2 mouse, double unitsPerPixel, Func<Vec2, Vec2> toWorld, Func<Vec2, Vec2> toLocal)
        {
            if (_dragIndex >= 0 && ReferenceEquals(_dragSession, session))
            {
                if (MouseInputService.IsDown(MouseButton.Left))
                {
                    if (_dragIndex < _dragPoints.Count)
                        _dragPoints[_dragIndex] = toLocal(mouse);
                    _dragPoints = ApplyPreview(polygon, overlay, _dragPoints);
                }
                else
                {
                    EndDrag(true);
                }
                return;
            }

            var pickRadius = PickDistance * unitsPerPixel;
            var left = MouseInputService.IsPressed(MouseButton.Left);
            var remove = MouseInputService.IsPressed(MouseButton.Middle);
            if (!left && !remove)
                return;

            var hit = -1;
            for (var i = 0; i < points.Count; i++)
            {
                var world = toWorld(points[i]);
                if (Distance(world, mouse) <= pickRadius)
                {
                    hit = i;
                    break;
                }
            }

            if (remove)
            {
                if (hit >= 0 && points.Count > MinimumPoints)
                {
                    var next = new List<Vec2>(points);
                    var nextColors = PolygonEditState.GetColors(session);
                    next.RemoveAt(hit);
                    nextColors.RemoveAt(hit);
                    state.SelectedIndex = Math.Min(hit, next.Count - 1);
                    Commit(session, next, nextColors);
                }
                return;
            }

            if (!EditorToolModel?.TryCapturePointer(this) ?? false)
                return;

            if (state.AddMode && hit < 0)
            {
                var next = new List<Vec2>(points) { toLocal(mouse) };
                var nextColors = PolygonEditState.GetColors(session);
                nextColors.Add(PolygonEditState.DefaultStyle);
                state.SelectedIndex = next.Count - 1;
                Commit(session, next, nextColors);
                EditorToolModel?.ReleasePointer(this);
                return;
            }

            var colors = PolygonEditState.GetColors(session);
            if (hit < 0)
            {
                var edge = FindEdge(points, mouse, pickRadius, toWorld);
                if (edge >= 0)
                {
                    points = new List<Vec2>(points);
                    points.Insert(edge + 1, toLocal(mouse));
                    colors.Insert(edge + 1, PolygonEditState.DefaultStyle);
                    hit = edge + 1;
                }
            }

            if (hit < 0)
            {
                EditorToolModel?.ReleasePointer(this);
                return;
            }

            state.SelectedIndex = hit;
            _dragSession = session;
            _dragIndex = hit;
            _dragPoints = new List<Vec2>(points);
            _dragColors = colors;
        }

        private static int FindEdge(List<Vec2> points, Vec2 mouse, double pickRadius, Func<Vec2, Vec2> toWorld)
        {
            var best = -1;
            var bestDistance = pickRadius;
            for (var i = 0; i < points.Count; i++)
            {
                var a = toWorld(points[i]);
                var b = toWorld(points[(i + 1) % points.Count]);
                var distance = DistanceToSegment(mouse, a, b);
                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    best = i;
                }
            }
            return best;
        }

        private static double Distance(Vec2 a, Vec2 b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private static double DistanceToSegment(Vec2 p, Vec2 a, Vec2 b)
        {
            var dx = b.X - a.X;
            var dy = b.Y - a.Y;
            var lengthSquared = dx * dx + dy * dy;
            var t = lengthSquared == 0 ? 0 : Math.Clamp(((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lengthSquared, 0, 1);
            return Distance(p, new Vec2(a.X + t * dx, a.Y + t * dy));
        }

        // The preview is applied to the live object; the change is committed as one undoable step when the drag ends.
        // Points pushed into negative space are normalized here too, so the polygon does not jump while dragging.
        private List<Vec2> ApplyPreview(DrawablePolygon polygon, Debug2DDrawable overlay, List<Vec2> points)
        {
            if (polygon.TryNormalize(points, out var normalized, out var position))
            {
                points = normalized;
                polygon.Position = position;
                overlay.Position = position;
                _previewPosition = position;
            }
            polygon.Points = PolygonEditState.Combine(points, _dragColors);
            overlay.Size = polygon.Size;
            overlay.UpdateLines();
            return points;
        }

        private void EndDrag(bool commit)
        {
            var session = _dragSession;
            var points = _dragPoints;
            var colors = _dragColors;
            _dragSession = null;
            _dragPoints = null;
            _dragColors = null;
            _dragIndex = -1;
            EditorToolModel?.ReleasePointer(this);
            if (commit && session != null && session.IsOpen && points != null)
                Commit(session, points, colors);
            _previewPosition = null;
        }

        private void Commit(PropertyEditorSession session, List<Vec2> positions, List<PolygonPoint> colors = null)
        {
            var property = FindProperty(session, session.Property.Name);
            if (property == null)
                return;

            var polygon = FindOverlay?.Invoke(session.Component)?.TypeOObject as DrawablePolygon;
            Vec2? position = _previewPosition;
            if (polygon != null && polygon.TryNormalize(positions, out var normalized, out var shifted))
            {
                positions = normalized;
                position = shifted;
            }

            var points = PolygonEditState.Combine(positions, colors ?? PolygonEditState.GetColors(session));
            var positionProperty = FindProperty(session, nameof(DrawablePolygon.Position));
            var positionChanged = position.HasValue && positionProperty != null;
            if (!positionChanged && session.Value is List<PolygonPoint> current && current.SequenceEqual(points))
                return;

            if (positionChanged)
                Execute(session, positionProperty, position.Value);
            Execute(session, property, points);
        }

        private static Property FindProperty(PropertyEditorSession session, string name)
        {
            return session.Component.Properties.FirstOrDefault(candidate => candidate.Name == name);
        }

        private void Execute(PropertyEditorSession session, Property property, object value)
        {
            var executed = EditorCommandRegistry?.Execute(
                "component.change-property",
                new ChangePropertyCommandData
                {
                    Project = Project,
                    Property = property,
                    Value = value
                });
            if (executed != true)
                throw new InvalidOperationException($"Could not apply the {property.Name} change for '{session.Component.FullName}'.");
        }

        private void SetSelectionSuspended(bool suspended)
        {
            if (_suspendedSelection == suspended || EditorToolModel == null)
                return;
            _suspendedSelection = suspended;
            EditorToolModel.SelectionInputSuspended = suspended;
        }

        private static void DrawHandles(ICanvas canvas, PolygonEditState state, List<Vec2> points, double unitsPerPixel, Func<Vec2, Vec2> toWorld)
        {
            if (points.Count == 0)
                return;

            var outline = new List<Vec2>(points.Count + 1);
            foreach (var point in points)
                outline.Add(toWorld(point));
            outline.Add(outline[0]);
            canvas.DrawLines(outline, Color.Cyan);

            var size = HandleSize * unitsPerPixel;
            for (var i = 0; i < points.Count; i++)
            {
                var center = outline[i];
                var selected = i == state.SelectedIndex;
                canvas.DrawRectangle(new Vec2(center.X - size / 2, center.Y - size / 2), new Vec2(size, size), true, selected ? Color.Yellow : Color.White);
            }
        }
    }
}
