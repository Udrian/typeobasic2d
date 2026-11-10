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
        public Vec2 ScreenPosition => GetPos();

        /// <summary>
        /// Determines whether a screen point lies inside the outlined area.
        /// </summary>
        /// <param name="point">The screen point to test.</param>
        /// <returns><c>true</c> if the point is inside the area; otherwise <c>false</c>.</returns>
        public bool ContainsPoint(Vec2 point)
        {
            var (pos, size) = GetBounds();
            return point.X >= pos.X && point.X <= pos.X + size.X
                && point.Y >= pos.Y && point.Y <= pos.Y + size.Y;
        }

        /// <summary>
        /// Gets the outlined area, which accounts for the position, origin, scale and rotation of drawables.
        /// </summary>
        /// <returns>The top-left position and the size of the area.</returns>
        public (Vec2 Pos, Vec2 Size) GetBounds()
        {
            if (TypeOObject is Drawable2d drawable)
            {
                var boundingBox = drawable.BoundingBox;
                var parentPosition = (drawable.Parent as Entity2d)?.ScreenBounds.Pos ?? Vec2.Zero;
                return (boundingBox.Pos + parentPosition, boundingBox.Size);
            }
            return (GetPos(), Size);
        }

        /// <summary>
        /// Recomputes the outline points from the current position and size.
        /// </summary>
        public void UpdateLines()
        {
            if (Lines == null)
                return;

            var (pos, size) = GetBounds();

            Lines[0] = pos;
            Lines[1] = pos + new Vec2(size.X, 0);
            Lines[2] = pos + size;
            Lines[3] = pos + new Vec2(0, size.Y);
            Lines[4] = pos;
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

        internal Vec2 GetObjectOrigin()
        {
            if (TypeOObject is Drawable2d drawable)
                return drawable.Origin;
            return Vec2.Zero;
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
