using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.TK.Engine.Graphics;
using TypeOEngine.Typedeaf.TK.Engine.Graphics.Primitives;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Interfaces;

namespace TypeOEngine.Typedeaf.Basic2d.Engine.Graphics
{
    public static partial class Canvas2d
    {
        // Primitives have a fixed vertex count, so one is kept per polygon size
        private static readonly Dictionary<int, Primitive<Vertex>> PolygonOutlinePrimitives = new();
        private static readonly Dictionary<int, Primitive<Vertex>> PolygonFillPrimitives = new();
        private static readonly Canvas2dPolygonColorBuffer PolygonColorBuffer = new();

        /// <summary>
        /// Draws a polygon from a list of points.
        /// </summary>
        /// <param name="canvas">Canvas to draw the polygon on.</param>
        /// <param name="pos">Position the points are relative to.</param>
        /// <param name="points">The polygon vertices in order, relative to <paramref name="pos"/>.</param>
        /// <param name="filled">Whether to fill the polygon or draw only its outline.</param>
        /// <param name="color">Color of the polygon.</param>
        /// <param name="rotation">Rotation angle in degrees.</param>
        /// <param name="origin">Local position to rotate around.</param>
        /// <param name="anchor">Transform position anchor.</param>
        /// <param name="vertexColors">Optional color per point, interpolated across the polygon instead of <paramref name="color"/>.</param>
        public static void DrawPolygon(this ICanvas canvas, Vec2 pos, IReadOnlyList<Vec2> points, bool filled, Color color, double rotation = 0, Vec2? origin = null, IAnchor2d anchor = null, IReadOnlyList<Color> vertexColors = null)
        {
            if (points == null || points.Count < 3)
                return;

            var snapshot = points.ToArray();
            var colorSnapshot = vertexColors != null && vertexColors.Count == snapshot.Length ? vertexColors.ToArray() : null;
            if (CanvasDrawCommandQueue.TryEnqueue(() => canvas.DrawPolygon(pos, snapshot, filled, color, rotation, origin, anchor, colorSnapshot)))
                return;

            CanvasDrawCommandQueue.FlushPendingBatch();
            if (canvas is not TKCanvas tkCanvas)
                return;

            if (colorSnapshot != null)
            {
                var indices = filled ? PolygonTriangulator.Triangulate(snapshot) : null;
                if (filled && indices.Length == 0)
                    return;
                pos += GetAnchorOffset(anchor);
                DrawWithEntityClipping(canvas, anchor, () =>
                {
                    ActivateShader(tkCanvas.Shader, tkCanvas, pos, rotation, null, origin, anchor);
                    PolygonColorBuffer.Draw(tkCanvas, snapshot, colorSnapshot, indices);
                });
                return;
            }
            Primitive<Vertex> primitive;
            if (filled)
            {
                var triangles = PolygonTriangulator.Triangulate(snapshot);
                if (triangles.Length == 0)
                    return;
                if (!PolygonFillPrimitives.TryGetValue(triangles.Length, out primitive))
                {
                    primitive = new Primitive<Vertex>(triangles.Length, PrimitiveDrawType.Triangle);
                    PolygonFillPrimitives[triangles.Length] = primitive;
                }
                for (var i = 0; i < triangles.Length; i++)
                    primitive.Vertices[i] = new Vertex(snapshot[triangles[i]]);
            }
            else
            {
                if (!PolygonOutlinePrimitives.TryGetValue(snapshot.Length, out primitive))
                {
                    primitive = new Primitive<Vertex>(snapshot.Length, PrimitiveDrawType.Line);
                    PolygonOutlinePrimitives[snapshot.Length] = primitive;
                }
                for (var i = 0; i < snapshot.Length; i++)
                    primitive.Vertices[i] = new Vertex(snapshot[i]);
            }

            pos += GetAnchorOffset(anchor);
            DrawWithEntityClipping(canvas, anchor, () =>
            {
                ActivateShader(tkCanvas.Shader, tkCanvas, pos, rotation, color, origin, anchor);
                primitive.Draw();
            });
        }
    }
}
