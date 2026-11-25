using TypeD.Models.Data;
using TypeDCore.Models.Data;
using TypeDCore.Models.Interfaces;
using TypeOEngine.Typedeaf.Basic2d.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;

namespace TypeDBasic2d.Typedeaf.Core.Entities.Drawables
{
    /// <summary>
    /// Editor overlay that outlines a 2D component. All pointer interaction is handled by
    /// <see cref="SelectionController2DDrawable"/>, which sets the selection and hover state on this drawable.
    /// </summary>
    public class Debug2DDrawable : Drawable2d
    {
        private List<Vec2> Lines { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the outlined object is selected.
        /// </summary>
        public bool Focused { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the outlined object is the topmost one under the cursor.
        /// </summary>
        public bool Hovered { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the outlined object is a parent of a selected node.
        /// </summary>
        public bool IsParentOfSelected { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the outlined object is a child of a selected node.
        /// </summary>
        public bool IsChildOfSelected { get; set; }

        /// <summary>
        /// Gets or sets the object being outlined.
        /// </summary>
        public TypeOObject TypeOObject { get; set; }

        /// <summary>
        /// Gets or sets the component the outlined object belongs to.
        /// </summary>
        public Component Component { get; set; }

        /// <summary>
        /// Gets or sets the model providing the active editor tool and the display settings.
        /// </summary>
        public IEditorToolModel EditorToolModel { get; set; }

        /// <summary>
        /// Draws the outline when it is visible for the current display settings.
        /// </summary>
        /// <param name="canvas">The canvas to draw on.</param>
        public override void Draw(ICanvas canvas)
        {
            var display = EditorToolModel;
            // The selected bounding boxes stay visible even when bounding boxes are hidden
            if (display != null && !display.ShowBoundingBoxes && !Focused)
                return;
            if (!IsVisibleForDisplayMode(display))
                return;

            UpdateLines();
            canvas.DrawLines(Lines, GetDrawColor(display));
        }

        /// <summary>
        /// Sets up the outline points and draws this overlay above everything else.
        /// </summary>
        protected override void Initialize()
        {
            DrawOrder = int.MaxValue;
            Lines = new List<Vec2> { new(), new(), new(), new(), new() };
            Focused = false;
            UpdateLines();
        }

        /// <summary>
        /// Cleans up the overlay.
        /// </summary>
        protected override void Cleanup()
        {
        }

        /// <summary>
        /// Gets the top-left position of the outlined area on screen.
        /// </summary>
        public Vec2 ScreenPosition => GetBounds().Pos;

        /// <summary>
        /// Determines whether a screen point lies inside the outlined area.
        /// </summary>
        /// <param name="point">The screen point to test.</param>
        /// <returns><c>true</c> if the point is inside the area; otherwise <c>false</c>.</returns>
        public bool ContainsPoint(Vec2 point)
        {
            var (topLeft, topRight, bottomRight, bottomLeft) = GetWorldCorners();
            var area = 0.0;
            var hasPositiveCross = false;
            var hasNegativeCross = false;

            for (var i = 0; i < 4; i++)
            {
                var start = i switch
                {
                    0 => topLeft,
                    1 => topRight,
                    2 => bottomRight,
                    _ => bottomLeft
                };
                var end = i switch
                {
                    0 => topRight,
                    1 => bottomRight,
                    2 => bottomLeft,
                    _ => topLeft
                };
                var edgeX = end.X - start.X;
                var edgeY = end.Y - start.Y;
                var pointX = point.X - start.X;
                var pointY = point.Y - start.Y;
                var cross = edgeX * pointY - edgeY * pointX;
                area += start.X * end.Y - start.Y * end.X;

                if (Math.Abs(cross) <= 1e-8
                    && pointX * (point.X - end.X) + pointY * (point.Y - end.Y) <= 1e-8)
                {
                    return true;
                }

                hasPositiveCross |= cross > 0;
                hasNegativeCross |= cross < 0;
            }

            if (Math.Abs(area) <= 1e-8)
            {
                var (pos, size) = GetBounds();
                return point.X >= pos.X && point.X <= pos.X + size.X
                    && point.Y >= pos.Y && point.Y <= pos.Y + size.Y;
            }

            return !(hasPositiveCross && hasNegativeCross);
        }

        /// <summary>
        /// Gets the outlined area, which accounts for the position, origin, scale and rotation of drawables.
        /// </summary>
        /// <returns>The top-left position and the size of the area.</returns>
        public (Vec2 Pos, Vec2 Size) GetBounds()
        {
            Vec2 pos;
            Vec2 size;
            if (TypeOObject is Entity2d entity)
                (pos, size) = (entity.ScreenBounds.Pos, entity.ScreenBounds.Size);
            else if (TypeOObject is Drawable2d drawable)
                (pos, size) = (drawable.WorldBounds.Pos, drawable.WorldBounds.Size);
            else
                (pos, size) = (Position, Size);

            var scale = CameraScale;
            return (ToEditorWorld(pos), scale == 1 ? size : new Vec2(size.X / scale, size.Y / scale));
        }

        /// <summary>
        /// Gets the draw stack the outlined object is drawn on.
        /// </summary>
        public DrawStack DrawStack => TypeOObject switch
        {
            Entity2d entity => entity.DrawStack,
            Drawable2d drawable => drawable.DrawStack,
            _ => null
        };

        /// <summary>
        /// Gets or sets the canvas whose camera is used to map objects on draw stacks that ignore the camera.
        /// </summary>
        public ICanvas CameraCanvas { get; set; }

        private bool IgnoresCamera => DrawStack?.IgnoreCamera == true && CameraCanvas != null;

        private double CameraScale => IgnoresCamera && CameraCanvas.WorldScale > 0 ? CameraCanvas.WorldScale : 1;

        /// <summary>
        /// Maps a point of the outlined object to editor world space, where the overlays are drawn and picked.
        /// Objects on a draw stack that ignores the camera are in screen space, which moves with the camera.
        /// </summary>
        /// <param name="point">The point in the space of the outlined object.</param>
        /// <returns>The point in editor world space.</returns>
        public Vec2 ToEditorWorld(Vec2 point)
        {
            if (!IgnoresCamera)
                return point;

            var scale = CameraScale;
            var translation = CameraCanvas.WorldTranslation;
            return new Vec2(translation.X + point.X / scale, translation.Y + point.Y / scale);
        }

        /// <summary>
        /// Maps a point in editor world space to the space of the outlined object.
        /// </summary>
        /// <param name="point">The point in editor world space.</param>
        /// <returns>The point in the space of the outlined object.</returns>
        public Vec2 FromEditorWorld(Vec2 point)
        {
            if (!IgnoresCamera)
                return point;

            var scale = CameraScale;
            var translation = CameraCanvas.WorldTranslation;
            return new Vec2((point.X - translation.X) * scale, (point.Y - translation.Y) * scale);
        }

        /// <summary>
        /// Gets the origin, relative to the outlined drawable, that is located at a point in editor world space.
        /// </summary>
        /// <param name="worldPosition">The point in editor world space.</param>
        /// <returns>The origin as a fraction of the drawable size.</returns>
        internal Vec2 GetOriginAtWorldPosition(Vec2 worldPosition)
        {
            return TypeOObject is Drawable2d drawable
                ? drawable.GetOriginAtWorldPosition(FromEditorWorld(worldPosition))
                : Vec2.Zero;
        }

        /// <summary>
        /// Recomputes the outline points from the current position and size.
        /// </summary>
        public void UpdateLines()
        {
            if (Lines == null)
                return;

            var (topLeft, topRight, bottomRight, bottomLeft) = GetWorldCorners();
            Lines[0] = topLeft;
            Lines[1] = topRight;
            Lines[2] = bottomRight;
            Lines[3] = bottomLeft;
            Lines[4] = topLeft;
        }

        internal (Vec2 TopLeft, Vec2 TopRight, Vec2 BottomRight, Vec2 BottomLeft) GetWorldCorners()
        {
            if (TypeOObject is Entity2d entity)
                return ToEditorWorld(entity.WorldCorners);
            if (TypeOObject is Drawable2d drawable)
                return ToEditorWorld(drawable.WorldCorners);

            var (pos, size) = GetBounds();
            return (pos, pos + new Vec2(size.X, 0), pos + size, pos + new Vec2(0, size.Y));
        }

        private (Vec2, Vec2, Vec2, Vec2) ToEditorWorld((Vec2 TopLeft, Vec2 TopRight, Vec2 BottomRight, Vec2 BottomLeft) corners)
        {
            return (ToEditorWorld(corners.TopLeft), ToEditorWorld(corners.TopRight), ToEditorWorld(corners.BottomRight), ToEditorWorld(corners.BottomLeft));
        }

        private bool IsVisibleForDisplayMode(IEditorToolModel display)
        {
            if (Focused)
                return true;

            var mode = display?.BoundingBoxDisplayMode ?? BoundingBoxDisplayMode.All;
            return mode switch
            {
                BoundingBoxDisplayMode.All => true,
                BoundingBoxDisplayMode.SelectedWithParents => IsParentOfSelected,
                BoundingBoxDisplayMode.SelectedWithParentsAndChildren => IsParentOfSelected || IsChildOfSelected,
                _ => false
            };
        }

        private Color GetDrawColor(IEditorToolModel display)
        {
            if (Hovered)
                return Color.MarineBlue;
            if (Focused)
                return display?.SelectedBoundingBoxColor ?? Color.Green;
            if (IsParentOfSelected)
                return display?.ParentBoundingBoxColor ?? Color.Yellow;
            if (IsChildOfSelected)
                return display?.ChildBoundingBoxColor ?? Color.Cyan;
            return display?.UnselectedBoundingBoxColor ?? Color.Red;
        }

        internal Vec2 GetObjectPosition()
        {
            if (TypeOObject is Entity2d entity)
                return entity.Position;
            if (TypeOObject is Drawable2d drawable)
                return drawable.Position;
            return Position;
        }

        internal Vec2 GetObjectSize()
        {
            if (TypeOObject is Entity2d entity)
                return entity.Size;
            if (TypeOObject is Drawable2d drawable)
                return drawable.Size;
            return Size;
        }

        internal void SetObjectPosition(Vec2 position)
        {
            if (TypeOObject is Entity2d entity)
                entity.Position = position;
            else if (TypeOObject is Drawable2d drawable)
                drawable.Position = position;
            Position = position;
        }

        internal void SetObjectSize(Vec2 size)
        {
            if (TypeOObject is Entity2d entity)
                entity.Size = size;
            else if (TypeOObject is Drawable2d drawable)
                drawable.Size = size;
            Size = size;
        }

        internal void SetObjectRotation(double rotation)
        {
            if (TypeOObject is Entity2d entity)
                entity.Rotation = rotation;
            else if (TypeOObject is Drawable2d drawable)
                drawable.Rotation = rotation;
            Rotation = rotation;
        }

        internal Vec2 GetObjectOrigin()
        {
            if (TypeOObject is Drawable2d drawable)
                return drawable.Origin;
            return Vec2.Zero;
        }

        internal double GetObjectRotation()
        {
            if (TypeOObject is Entity2d entity)
                return entity.Rotation;
            if (TypeOObject is Drawable2d drawable)
                return drawable.Rotation;
            return 0;
        }

        internal Vec2 GetObjectPivot()
        {
            if (TypeOObject is Entity2d entity)
                return ToEditorWorld(entity.WorldPosition);
            if (TypeOObject is Drawable2d drawable)
                return ToEditorWorld(drawable.WorldPosition);
            return ToEditorWorld(Position);
        }

        internal Vec2 GetObjectLocalDelta(Vec2 worldDelta)
        {
            var scale = CameraScale;
            if (scale != 1)
                worldDelta = new Vec2(worldDelta.X * scale, worldDelta.Y * scale);

            if (TypeOObject is Entity2d entity && entity.Parent is Entity2d entityParent)
                return entityParent.WorldDeltaToLocal(worldDelta);
            if (TypeOObject is Drawable2d drawable)
                return drawable.WorldDeltaToLocal(worldDelta);
            return worldDelta;
        }
    }
}
