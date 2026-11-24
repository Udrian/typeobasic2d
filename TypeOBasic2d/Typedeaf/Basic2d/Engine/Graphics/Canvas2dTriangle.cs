using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.TK.Engine.Graphics;
using TypeOEngine.Typedeaf.TK.Engine.Graphics.Primitives;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Interfaces;

namespace TypeOEngine.Typedeaf.Basic2d.Engine.Graphics
{
    /// <summary>
    /// Extension methods for drawing triangles on a 2D canvas.
    /// </summary>
    public static partial class Canvas2d
    {
        private static readonly Primitive<Vertex> TriangleOutlinePrimitive =
            new Primitive<Vertex>(3, PrimitiveDrawType.Line);

        private static readonly Primitive<Vertex> TriangleFillPrimitive =
            new Primitive<Vertex>(3, PrimitiveDrawType.Triangle);

        /// <summary>
        /// Draws an isosceles triangle within the specified size.
        /// </summary>
        /// <param name="canvas">Canvas to draw the triangle on.</param>
        /// <param name="pos">Top-left position of the triangle.</param>
        /// <param name="size">Width and height of the triangle.</param>
        /// <param name="filled">Whether to fill the triangle or draw only its outline.</param>
        /// <param name="color">Color of the triangle.</param>
        /// <param name="rotation">Rotation angle in degrees.</param>
        /// <param name="anchor">Transform position anchor.</param>
        public static void DrawTriangle(this ICanvas canvas, Vec2 pos, Vec2 size, bool filled, Color color, double rotation = 0, IAnchor2d anchor = null)
        {
            DrawTriangle(canvas, pos, size, filled, color, rotation, null, anchor);
        }

        /// <summary>
        /// Draws an isosceles triangle with rotation around a local origin.
        /// </summary>
        /// <param name="canvas">Canvas to draw the triangle on.</param>
        /// <param name="pos">Top-left position of the triangle.</param>
        /// <param name="size">Width and height of the triangle.</param>
        /// <param name="filled">Whether to fill the triangle or draw only its outline.</param>
        /// <param name="color">Color of the triangle.</param>
        /// <param name="rotation">Rotation angle in degrees.</param>
        /// <param name="anchor">Transform position anchor.</param>
        /// <param name="origin">Local position to rotate around.</param>
        public static void DrawTriangle(this ICanvas canvas, Vec2 pos, Vec2 size, bool filled, Color color, double rotation, Vec2? origin, IAnchor2d anchor = null)
        {
            if (CanvasDrawCommandQueue.TryEnqueue(() => canvas.DrawTriangle(pos, size, filled, color, rotation, origin, anchor)))
                return;

            CanvasDrawCommandQueue.FlushPendingBatch();
            if (canvas is not TKCanvas tkCanvas)
                return;

            var points = new[]
            {
                new Vec2(size.X / 2, 0),
                new Vec2(size.X, size.Y),
                new Vec2(0, size.Y)
            };

            for (var i = 0; i < points.Length; i++)
            {
                var vertex = new Vertex(points[i]);
                TriangleOutlinePrimitive.Vertices[i] = vertex;
                TriangleFillPrimitive.Vertices[i] = vertex;
            }

            pos += GetAnchorOffset(anchor);
            DrawWithEntityClipping(canvas, anchor, () =>
            {
                ActivateShader(tkCanvas.Shader, tkCanvas, pos, rotation, color, origin, anchor);
                if (filled)
                    TriangleFillPrimitive.Draw();
                else
                    TriangleOutlinePrimitive.Draw();
            });
        }
    }
}
