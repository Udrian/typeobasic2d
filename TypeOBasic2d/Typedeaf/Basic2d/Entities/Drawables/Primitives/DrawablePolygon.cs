using TypeOEngine.Typedeaf.Basic2d.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Attributes;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;

namespace TypeOBasic2d.Typedeaf.Basic2d.Entities.Drawables.Primitives
{
    /// <summary>
    /// A Drawable class for drawing a filled or unfilled polygon from a list of points
    /// </summary>
    public class DrawablePolygon : Drawable2d
    {
        /// <summary>
        /// Gets or sets the polygon points, each with a position relative to the top-left of the drawable's bounds and a color. The editor keeps positions non-negative by moving the position, unless the origin is not top-left.
        /// The alpha of a point color is how much of it is blended into <see cref="Color"/>, so transparent uses the polygon color unchanged.
        /// </summary>
        [TypeOProperty("Gets or sets the polygon points with a position and a color each. A transparent point color uses the polygon color, otherwise it is blended with it.", null, Group = "Polygon")]
        public List<PolygonPoint> Points { get; set; } = new List<PolygonPoint> { new PolygonPoint(new Vec2(50, 0)), new PolygonPoint(new Vec2(100, 100)), new PolygonPoint(new Vec2(0, 100)) };

        /// <summary>
        /// Gets the color a point is drawn with, the point color blended into the component color by the point color's alpha.
        /// </summary>
        /// <param name="index">The point index.</param>
        public Color GetPointColor(int index)
        {
            if (Points == null || index < 0 || index >= Points.Count)
                return Color;
            var point = Points[index];
            var blend = point.Blend != PolygonColorBlend.Default ? point.Blend : Blend;
            return BlendPointColor(Color, point.Color, blend);
        }

        /// <summary>
        /// Gets or sets how point colors are blended into <see cref="Color"/>, unless a point has its own blend.
        /// </summary>
        [TypeOProperty("Gets or sets how point colors are blended into the polygon color, unless a point has its own blend.", Group = "Rendering")]
        public PolygonColorBlend Blend { get; set; } = PolygonColorBlend.Normal;

        /// <summary>
        /// Blends a point color into a base color. The point color's alpha is the blend amount and the base alpha is kept.
        /// </summary>
        /// <param name="color">The component color.</param>
        /// <param name="pointColor">The point color.</param>
        /// <param name="blend">How the colors are blended. <see cref="PolygonColorBlend.Default"/> is the same as <see cref="PolygonColorBlend.Normal"/>.</param>
        public static Color BlendPointColor(Color color, Color pointColor, PolygonColorBlend blend = PolygonColorBlend.Normal)
        {
            if (pointColor.A == 0)
                return color;
            var amount = pointColor.A / 255.0;
            return new Color(
                (int)color.A,
                BlendChannel(color.R, pointColor.R, amount, blend),
                BlendChannel(color.G, pointColor.G, amount, blend),
                BlendChannel(color.B, pointColor.B, amount, blend));
        }

        private static int BlendChannel(byte baseValue, byte pointValue, double amount, PolygonColorBlend blend)
        {
            var b = baseValue / 255.0;
            var p = pointValue / 255.0;
            var mixed = blend switch
            {
                PolygonColorBlend.Multiply => b * p,
                PolygonColorBlend.Add => b + p,
                PolygonColorBlend.Subtract => b - p,
                PolygonColorBlend.Screen => 1 - (1 - b) * (1 - p),
                PolygonColorBlend.Overlay => b < 0.5 ? 2 * b * p : 1 - 2 * (1 - b) * (1 - p),
                PolygonColorBlend.Darken => Math.Min(b, p),
                PolygonColorBlend.Lighten => Math.Max(b, p),
                PolygonColorBlend.Difference => Math.Abs(b - p),
                _ => p
            };
            var result = b + (Math.Clamp(mixed, 0, 1) - b) * amount;
            return (int)Math.Round(Math.Clamp(result, 0, 1) * 255);
        }

        /// <summary>
        /// Gets or sets the texture drawn inside the filled polygon, tinted with <see cref="Color"/> and the point colors.
        /// </summary>
        [TypeOProperty("Gets or sets the texture drawn inside the filled polygon, tinted with the color and the point colors.", Group = "Rendering")]
        public Texture Texture { get; set; }

        /// <summary>
        /// Gets or sets how the texture is mapped onto the polygon: stretched over the bounding box, or drawn at its natural size and clipped to the polygon.
        /// </summary>
        [TypeOProperty("Gets or sets how the texture is mapped onto the polygon: stretched over the bounding box, or drawn at its natural size and clipped to the polygon.", Group = "Rendering")]
        public PolygonTextureMode TextureMode { get; set; } = PolygonTextureMode.Stretch;

        /// <summary>
        /// Gets the texture coordinates of points that are relative to the top-left of the bounds.
        /// </summary>
        /// <param name="points">The points, already scaled and relative to the top-left of the bounds.</param>
        /// <param name="boundsSize">The scaled size of the bounds.</param>
        /// <param name="textureSize">The natural size of the texture.</param>
        /// <param name="mode">How the texture is mapped.</param>
        public static Vec2[] GetTextureCoordinates(IReadOnlyList<Vec2> points, Vec2 boundsSize, Vec2 textureSize, PolygonTextureMode mode)
        {
            if (mode == PolygonTextureMode.Stretch)
                return GetPerimeterCoordinates(points);

            var area = mode == PolygonTextureMode.Bounds ? boundsSize : textureSize;
            var uvs = new Vec2[points.Count];
            for (var i = 0; i < uvs.Length; i++)
                uvs[i] = new Vec2(area.X == 0 ? 0 : points[i].X / area.X, area.Y == 0 ? 0 : points[i].Y / area.Y);
            return uvs;
        }

        // Spreads the points evenly along the polygon outline by length and places them at the same distance along the texture border
        private static Vec2[] GetPerimeterCoordinates(IReadOnlyList<Vec2> points)
        {
            var count = points.Count;
            var distances = new double[count + 1];
            for (var i = 0; i < count; i++)
            {
                var delta = points[(i + 1) % count] - points[i];
                distances[i + 1] = distances[i] + Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
            }

            var uvs = new Vec2[count];
            var perimeter = distances[count];
            for (var i = 0; i < count; i++)
            {
                // Four sides of the texture border: top, right, bottom, left
                var t = perimeter == 0 ? 0 : distances[i] / perimeter * 4;
                var side = (int)Math.Floor(t);
                var along = t - side;
                uvs[i] = side switch
                {
                    0 => new Vec2(along, 0),
                    1 => new Vec2(1, along),
                    2 => new Vec2(1 - along, 1),
                    _ => new Vec2(0, 1 - along)
                };
            }
            return uvs;
        }

        /// <summary>
        /// Gets or sets the color associated with the object.
        /// </summary>
        [TypeOProperty("Gets or sets the color associated with the object.", null, Group = "Rendering")]
        public Color Color { get; set; } = Color.White;

        /// <summary>
        /// Gets or sets if the polygon should be filled or only drawn with a line around the border
        /// </summary>
        [TypeOProperty("Gets or sets if the polygon should be filled or only drawn with a line around the border", null, Group = "Rendering")]
        public bool Filled { get; set; } = true;

        /// <summary>
        /// Gets the size of the polygon, which is the bounding extent of its points (including negative ones).
        /// Setting it scales the points from the top-left of the bounds so the shape is kept but fits the new size.
        /// </summary>
        public override Vec2 Size
        {
            get => GetSize(GetPositions(Points));
            set => Points = ScalePoints(Points, value);
        }

        /// <summary>
        /// Scales points from the top-left of their bounds so the bounds get the given size. An axis with no extent is left unchanged.
        /// </summary>
        /// <param name="points">The points to scale.</param>
        /// <param name="size">The new size of the bounds.</param>
        public static List<PolygonPoint> ScalePoints(IReadOnlyList<PolygonPoint> points, Vec2 size)
        {
            if (points == null || points.Count == 0)
                return points?.ToList();
            var positions = GetPositions(points);
            var current = GetSize(positions);
            var min = GetBoundsMin(positions);
            var scaleX = current.X == 0 ? 1 : Math.Max(size.X, 0) / current.X;
            var scaleY = current.Y == 0 ? 1 : Math.Max(size.Y, 0) / current.Y;
            return points.Select(p => new PolygonPoint(new Vec2(min.X + (p.Position.X - min.X) * scaleX, min.Y + (p.Position.Y - min.Y) * scaleY), p.Color)).ToList();
        }

        /// <summary>
        /// Gets the positions of points.
        /// </summary>
        /// <param name="points">The points.</param>
        public static List<Vec2> GetPositions(IReadOnlyList<PolygonPoint> points)
        {
            return points?.Select(p => p.Position).ToList() ?? new List<Vec2>();
        }

        /// <summary>
        /// Gets the top-left corner of the bounds in point space, the smallest X and Y of the points.
        /// </summary>
        public Vec2 BoundsMin => GetBoundsMin(GetPositions(Points));

        /// <summary>
        /// Gets the top-left corner of the bounds of a set of points.
        /// </summary>
        /// <param name="points">The points.</param>
        public static Vec2 GetBoundsMin(IReadOnlyList<Vec2> points)
        {
            if (points == null || points.Count == 0)
                return Vec2.Zero;
            return new Vec2(points.Min(p => p.X), points.Min(p => p.Y));
        }

        /// <summary>
        /// Gets the size of the bounds tightly covering all points.
        /// </summary>
        /// <param name="points">The points.</param>
        public static Vec2 GetSize(IReadOnlyList<Vec2> points)
        {
            if (points == null || points.Count == 0)
                return Vec2.Zero;
            var min = GetBoundsMin(points);
            return new Vec2(points.Max(p => p.X) - min.X, points.Max(p => p.Y) - min.Y);
        }

        /// <summary>
        /// Shifts the points so the smallest X and Y are 0 and compensates with a new <see cref="Drawable2d.Position"/> so the polygon does not move visually, keeping the bounds tight.
        /// Only applies when the origin is top-left (0,0); with another origin the points are kept as they are.
        /// </summary>
        /// <param name="points">The points to normalize.</param>
        /// <param name="normalized">The shifted points when normalization was needed.</param>
        /// <param name="position">The position to use together with the shifted points.</param>
        /// <returns>True if the points had to be shifted.</returns>
        public bool TryNormalize(IReadOnlyList<Vec2> points, out List<Vec2> normalized, out Vec2 position)
        {
            normalized = null;
            position = Position;
            if (points == null || points.Count == 0 || Origin.X != 0 || Origin.Y != 0)
                return false;

            var min = GetBoundsMin(points);
            var minX = min.X;
            var minY = min.Y;
            if (minX == 0 && minY == 0)
                return false;

            normalized = points.Select(p => new Vec2(p.X - minX, p.Y - minY)).ToList();
            var offset = new Vec2(minX * Scale.X, minY * Scale.Y);
            var radians = Rotation * (Math.PI / 180.0);
            var cos = Math.Cos(radians);
            var sin = Math.Sin(radians);
            position = Position + new Vec2(offset.X * cos - offset.Y * sin, offset.X * sin + offset.Y * cos);
            return true;
        }

        /// <inheritdoc/>
        public override void Draw(ICanvas canvas)
        {
            var points = Points;
            if (points == null || points.Count < 3)
                return;

            var scaledSize = Size * Scale;
            var position = Position - scaledSize * Origin;
            var min = BoundsMin;
            var scaled = new Vec2[points.Count];
            for (var i = 0; i < scaled.Length; i++)
                scaled[i] = (points[i].Position - min) * Scale;
            var colors = points.Any(p => p.Color.A != 0)
                ? Enumerable.Range(0, points.Count).Select(GetPointColor).ToArray()
                : null;
            if (Texture != null && Filled)
            {
                var textureSize = Texture.Size * Scale;
                var uvs = GetTextureCoordinates(scaled, scaledSize, textureSize, TextureMode);
                canvas.DrawTexturedPolygon(Texture, position, scaled, uvs, TextureMode == PolygonTextureMode.Clip, Color, Rotation, scaledSize * Origin,                 Parent as Entity2d, colors, (int)Blend);
                                return;
            }

            canvas.DrawPolygon(position, scaled, Filled, Color, Rotation, scaledSize * Origin, Parent as Entity2d, colors);
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
