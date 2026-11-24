using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.TK.Engine.Graphics;
using TypeOEngine.Typedeaf.TK.Engine.Graphics.Primitives;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Interfaces;

namespace TypeOEngine.Typedeaf.Basic2d.Engine.Graphics
{
    /// <summary>
    /// Extension methods for drawing circles on a 2D canvas.
    /// </summary>
    public static partial class Canvas2d
    {
        private const int CircleSegments = 64;

        private static readonly Primitive<Vertex> CircleOutlinePrimitive =
            new Primitive<Vertex>(CircleSegments, PrimitiveDrawType.Line);

        private static readonly Primitive<Vertex> CircleFillPrimitive =
            new Primitive<Vertex>(CircleSegments * 3, PrimitiveDrawType.Triangle);

        /// <summary>
        /// Draws a circle centered in the specified bounds. If the bounds are not square,
        /// the circle is inscribed using the smaller dimension.
        /// </summary>
        /// <param name="canvas">Canvas to draw the circle on.</param>
        /// <param name="bounds">Bounds that contain the circle.</param>
        /// <param name="filled">Whether to fill the circle or draw only its outline.</param>
        /// <param name="color">Color of the circle.</param>
        /// <param name="anchor">Transform position anchor.</param>
        public static void DrawCircle(this ICanvas canvas, Rectangle bounds, bool filled, Color color, IAnchor2d anchor = null)
        {
            if (canvas is not TKCanvas tkCanvas)
                return;

            var center = bounds.Pos + bounds.Size / 2;
            var radius = Math.Min(Math.Abs(bounds.Size.X), Math.Abs(bounds.Size.Y)) / 2;
            if (radius == 0)
                return;

            var angleStep = 2 * Math.PI / CircleSegments;
            for (var i = 0; i < CircleSegments; i++)
            {
                var angle = i * angleStep;
                var point = new Vec2(Math.Cos(angle) * radius, Math.Sin(angle) * radius);
                CircleOutlinePrimitive.Vertices[i] = new Vertex(point);

                var nextAngle = (i + 1) * angleStep;
                var nextPoint = new Vec2(Math.Cos(nextAngle) * radius, Math.Sin(nextAngle) * radius);
                var fillIndex = i * 3;
                CircleFillPrimitive.Vertices[fillIndex] = new Vertex(Vec2.Zero);
                CircleFillPrimitive.Vertices[fillIndex + 1] = new Vertex(point);
                CircleFillPrimitive.Vertices[fillIndex + 2] = new Vertex(nextPoint);
            }

            var position = center + GetAnchorOffset(anchor);
            DrawWithEntityClipping(canvas, anchor, () =>
            {
                ActivateShader(tkCanvas.Shader, tkCanvas, position, 0, color, anchor: anchor);
                if (filled)
                    CircleFillPrimitive.Draw();
                else
                    CircleOutlinePrimitive.Draw();
            });
        }
    }
}
