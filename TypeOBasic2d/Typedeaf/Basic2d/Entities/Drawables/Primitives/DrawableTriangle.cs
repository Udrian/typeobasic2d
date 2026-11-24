using TypeOEngine.Typedeaf.Basic2d.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Attributes;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;

namespace TypeOBasic2d.Typedeaf.Basic2d.Entities.Drawables.Primitives
{
    /// <summary>
    /// A drawable class for drawing a filled or unfilled triangle.
    /// </summary>
    public class DrawableTriangle : Drawable2d
    {
        /// <summary>
        /// Gets or sets the color associated with the object.
        /// </summary>
        [TypeOProperty("Gets or sets the color associated with the object.", null, Group = "Rendering")]
        public Color Color { get; set; } = Color.White;

        /// <summary>
        /// Gets or sets whether the triangle should be filled or only drawn with a line around the border.
        /// </summary>
        [TypeOProperty("Gets or sets whether the triangle should be filled or only drawn with a line around the border.", null, Group = "Rendering")]
        public bool Filled { get; set; } = true;

        /// <inheritdoc/>
        public override void Draw(ICanvas canvas)
        {
            var scaledSize = Size * Scale;
            var position = Position - scaledSize * Origin;
            canvas.DrawTriangle(position, scaledSize, Filled, Color, Rotation);
        }

        /// <inheritdoc/>
        protected override void Cleanup()
        {
        }

        /// <inheritdoc/>
        protected override void Initialize()
        {
        }
    }
}
