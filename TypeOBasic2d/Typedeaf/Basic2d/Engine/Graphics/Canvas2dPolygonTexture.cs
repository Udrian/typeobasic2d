using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.TK.Contents;
using TypeOEngine.Typedeaf.TK.Engine.Graphics;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Interfaces;

namespace TypeOEngine.Typedeaf.Basic2d.Engine.Graphics
{
    public static partial class Canvas2d
    {
        private static readonly Canvas2dPolygonTextureBuffer PolygonTextureBuffer = new();

        /// <summary>
        /// Draws a filled polygon textured with the given texture coordinates.
        /// </summary>
        /// <param name="canvas">Canvas to draw the polygon on.</param>
        /// <param name="texture">Texture to draw.</param>
        /// <param name="pos">Position the points are relative to.</param>
        /// <param name="points">The polygon vertices in order, relative to <paramref name="pos"/>.</param>
        /// <param name="uvs">A texture coordinate per point, where (0,0) to (1,1) covers the texture.</param>
        /// <param name="clipToTexture">Whether the parts of the polygon outside of the texture are left undrawn instead of repeating the texture.</param>
        /// <param name="color">Color the texture is tinted with.</param>
        /// <param name="rotation">Rotation angle in degrees.</param>
        /// <param name="origin">Local position to rotate around.</param>
        /// <param name="anchor">Transform position anchor.</param>
        /// <param name="vertexColors">An optional color per point that tints the texture, used instead of <paramref name="color"/>.</param>
        public static void DrawTexturedPolygon(this ICanvas canvas, Texture texture, Vec2 pos, IReadOnlyList<Vec2> points, IReadOnlyList<Vec2> uvs, bool clipToTexture, Color color, double rotation = 0, Vec2? origin = null, IAnchor2d anchor = null, IReadOnlyList<Color> vertexColors = null, int blendMode = 0)
        {
            if (texture == null || points == null || uvs == null || points.Count < 3 || uvs.Count != points.Count)
                return;
            if (vertexColors != null && vertexColors.Count != points.Count)
                vertexColors = null;

            var pointSnapshot = points.ToArray();
            var uvSnapshot = uvs.ToArray();
            var colorSnapshot = vertexColors?.ToArray();
            if (CanvasDrawCommandQueue.TryEnqueue(() => canvas.DrawTexturedPolygon(texture, pos, pointSnapshot, uvSnapshot, clipToTexture, color, rotation, origin, anchor, colorSnapshot, blendMode)))
                return;

            CanvasDrawCommandQueue.FlushPendingBatch();
            if (canvas is not TKCanvas tkCanvas || texture is not TKTexture tkTexture)
                return;

            var indices = PolygonTriangulator.Triangulate(pointSnapshot);
            if (indices.Length == 0)
                return;

            pos += GetAnchorOffset(anchor);
            DrawWithEntityClipping(canvas, anchor, () =>
            {
                var shader = tkCanvas.TextureShader;
                ActivateShader(shader, tkCanvas, pos, rotation, colorSnapshot != null ? Color.White : color, origin, anchor);
                shader.Set("discardOutside", clipToTexture ? 1 : 0);
                shader.Set("useVertexColor", colorSnapshot != null ? 1 : 0);
                shader.Set("blendMode", blendMode);
                try
                {
                    tkTexture.Use();
                    PolygonTextureBuffer.Draw(pointSnapshot, uvSnapshot, indices, colorSnapshot);
                }
                finally
                {
                    shader.Set("discardOutside", 0);
                    shader.Set("useVertexColor", 0);
                    shader.Set("blendMode", 0);
                }
            });
        }
    }
}
