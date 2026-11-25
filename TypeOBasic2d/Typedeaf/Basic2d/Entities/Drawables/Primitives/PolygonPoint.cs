using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;

namespace TypeOBasic2d.Typedeaf.Basic2d.Entities.Drawables.Primitives
{
    /// <summary>
    /// A point of a <see cref="DrawablePolygon"/>, with a position, a color and a blend for how the color is blended into the polygon color.
    /// </summary>
    public struct PolygonPoint : IEquatable<PolygonPoint>
    {
        /// <summary>
        /// Gets or sets the position, relative to the top-left of the drawable's bounds.
        /// </summary>
        public Vec2 Position { get; set; }

        /// <summary>
        /// Gets or sets the point color. Its alpha is how much of it is blended into the polygon color, so transparent uses the polygon color unchanged.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Gets or sets how the point color is blended into the polygon color. <see cref="PolygonColorBlend.Default"/> uses the blend of the polygon.
        /// </summary>
        public PolygonColorBlend Blend { get; set; }

        /// <summary>
        /// Creates a point with a color that uses the polygon color.
        /// </summary>
        /// <param name="position">The position of the point.</param>
        public PolygonPoint(Vec2 position) : this(position, Color.Transparent, PolygonColorBlend.Default)
        {
        }

        /// <summary>
        /// Creates a point with a color.
        /// </summary>
        /// <param name="position">The position of the point.</param>
        /// <param name="color">The color of the point.</param>
        public PolygonPoint(Vec2 position, Color color) : this(position, color, PolygonColorBlend.Default)
        {
        }

        /// <summary>
        /// Creates a point with a color and a blend.
        /// </summary>
        /// <param name="position">The position of the point.</param>
        /// <param name="color">The color of the point.</param>
        /// <param name="blend">How the color is blended into the polygon color.</param>
        public PolygonPoint(Vec2 position, Color color, PolygonColorBlend blend)
        {
            Position = position;
            Color = color;
            Blend = blend;
        }

        /// <inheritdoc/>
        public bool Equals(PolygonPoint other) => Position.Equals(other.Position) && Color.Equals(other.Color) && Blend == other.Blend;

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is PolygonPoint other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Position, Color, Blend);

        /// <summary>Compares two points for equality.</summary>
        public static bool operator ==(PolygonPoint a, PolygonPoint b) => a.Equals(b);

        /// <summary>Compares two points for inequality.</summary>
        public static bool operator !=(PolygonPoint a, PolygonPoint b) => !a.Equals(b);
    }
}
