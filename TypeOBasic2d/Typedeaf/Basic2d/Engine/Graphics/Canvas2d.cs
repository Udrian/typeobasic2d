using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.TK.Engine.Graphics.Primitives;
using TypeOEngine.Typedeaf.TK.Engine.Graphics;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Interfaces;

namespace TypeOEngine.Typedeaf.Basic2d.Engine.Graphics
{
    /// <summary>
    /// Extension class for ICanvas
    /// </summary>
    public static partial class Canvas2d
    {
        private static readonly Dictionary<int, Primitive<Vertex>> ClipPrimitives = new Dictionary<int, Primitive<Vertex>>();

        internal static Vec2 GetAnchorOffset(IAnchor2d anchor)
        {
            return anchor is Entity2d ? Vec2.Zero : anchor?.Position ?? Vec2.Zero;
        }

        private static Matrix4 GetAnchorMatrix(IAnchor2d anchor)
        {
            var entity = GetAnchorEntity(anchor);
            return entity?.ChildrenModelMatrix ?? Matrix4.Identity;
        }

        private static Entity2d GetAnchorEntity(IAnchor2d anchor)
        {
            return anchor as Entity2d ?? (anchor as Anchor2d)?.Parent;
        }

        internal static void DrawWithEntityClipping(ICanvas canvas, IAnchor2d anchor, Action draw)
        {
            var clips = new List<Entity2d>();
            for (var entity = GetAnchorEntity(anchor); entity != null; entity = entity.Parent as Entity2d)
            {
                if (entity.SizeMode == SizeMode.Clip)
                    clips.Add(entity);
            }

            if (clips.Count == 0 || canvas is not TKCanvas tkCanvas)
            {
                draw();
                return;
            }

            var clipPolygon = GetClipPolygon(clips);
            if (clipPolygon.Count < 3)
                return;

            if (!tkCanvas.EnsureStencilBuffer())
                throw new InvalidOperationException("Exact 2D clipping requires a stencil buffer, but the current framebuffer has none.");

            try
            {
                GL.Enable(EnableCap.StencilTest);
                GL.StencilMask(-1);
                GL.ClearStencil(0);
                GL.Clear(ClearBufferMask.StencilBufferBit);

                GL.ColorMask(false, false, false, false);
                GL.StencilMask(1);
                GL.StencilFunc(StencilFunction.Always, 1, 1);
                GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);

                var vertexCount = (clipPolygon.Count - 2) * 3;
                if (!ClipPrimitives.TryGetValue(vertexCount, out var clipPrimitive))
                {
                    clipPrimitive = new Primitive<Vertex>(vertexCount, PrimitiveDrawType.Triangle);
                    ClipPrimitives.Add(vertexCount, clipPrimitive);
                }

                var vertexIndex = 0;
                for (var i = 1; i < clipPolygon.Count - 1; i++)
                {
                    clipPrimitive.Vertices[vertexIndex++] = new Vertex(clipPolygon[0]);
                    clipPrimitive.Vertices[vertexIndex++] = new Vertex(clipPolygon[i]);
                    clipPrimitive.Vertices[vertexIndex++] = new Vertex(clipPolygon[i + 1]);
                }

                ActivateShader(tkCanvas.Shader, tkCanvas, Vec2.Zero, 0, Color.White);
                clipPrimitive.Draw();

                GL.ColorMask(true, true, true, true);
                GL.StencilMask(0);
                GL.StencilFunc(StencilFunction.Equal, 1, 1);
                GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
                draw();
            }
            finally
            {
                GL.ColorMask(true, true, true, true);
                GL.StencilMask(-1);
                GL.Disable(EnableCap.StencilTest);
                tkCanvas.ReleaseStencilBuffer();
            }
        }

        internal static List<Vec2> GetClipPolygon(IReadOnlyList<Entity2d> clips)
        {
            if (clips.Count == 0)
                return new List<Vec2>();

            var first = clips[0].WorldCorners;
            var polygon = new List<Vec2> { first.TopLeft, first.TopRight, first.BottomRight, first.BottomLeft };
            if (Math.Abs(SignedArea(polygon)) < 1e-8)
                return new List<Vec2>();

            for (var i = 1; i < clips.Count && polygon.Count >= 3; i++)
            {
                var corners = clips[i].WorldCorners;
                polygon = IntersectConvexPolygons(
                    polygon,
                    new List<Vec2> { corners.TopLeft, corners.TopRight, corners.BottomRight, corners.BottomLeft });
            }

            return polygon;
        }

        private static List<Vec2> IntersectConvexPolygons(List<Vec2> subject, List<Vec2> clip)
        {
            var clipArea = SignedArea(clip);
            if (Math.Abs(clipArea) < 1e-8)
                return new List<Vec2>();

            var winding = clipArea >= 0 ? 1 : -1;

            for (var edgeIndex = 0; edgeIndex < clip.Count && subject.Count > 0; edgeIndex++)
            {
                var edgeStart = clip[edgeIndex];
                var edgeEnd = clip[(edgeIndex + 1) % clip.Count];
                var edge = edgeEnd - edgeStart;
                var input = subject;
                subject = new List<Vec2>();
                var previous = input[input.Count - 1];
                var previousInside = winding * Cross(edge, previous - edgeStart) >= -1e-8;

                foreach (var current in input)
                {
                    var currentInside = winding * Cross(edge, current - edgeStart) >= -1e-8;
                    if (currentInside != previousInside)
                        subject.Add(LineIntersection(previous, current, edgeStart, edgeEnd));
                    if (currentInside)
                        subject.Add(current);
                    previous = current;
                    previousInside = currentInside;
                }
            }

            return subject;
        }

        private static double SignedArea(IReadOnlyList<Vec2> polygon)
        {
            var area = 0.0;
            for (var i = 0; i < polygon.Count; i++)
                area += Cross(polygon[i], polygon[(i + 1) % polygon.Count]);
            return area;
        }

        private static Vec2 LineIntersection(Vec2 start, Vec2 end, Vec2 clipStart, Vec2 clipEnd)
        {
            var direction = end - start;
            var clipDirection = clipEnd - clipStart;
            var denominator = Cross(direction, clipDirection);
            if (Math.Abs(denominator) < 1e-12)
                return end;

            var amount = Cross(clipStart - start, clipDirection) / denominator;
            return start + direction * amount;
        }

        private static double Cross(Vec2 first, Vec2 second)
        {
            return first.X * second.Y - first.Y * second.X;
        }

        private static void ActivateShader(Shader shader, TKCanvas tkCanvas, Vec2 pos, double rotation, Color? color, Vec2? origin = null, IAnchor2d anchor = null)
        {
            shader.Use();
            var rotationOrigin = origin ?? Vec2.Zero;
            var modelMatrix =
                Matrix4.CreateTranslation((float)-rotationOrigin.X, (float)-rotationOrigin.Y, 0) *
                Matrix4.CreateRotationZ(MathHelper.DegreesToRadians((float)rotation)) *
                Matrix4.CreateTranslation((float)(pos.X + rotationOrigin.X), (float)(pos.Y + rotationOrigin.Y), 0);
            modelMatrix *= GetAnchorMatrix(anchor);
            shader.Set("model", modelMatrix);
            shader.Set("view", tkCanvas.ViewMatrix);
            shader.Set("projection", tkCanvas.ProjectionMatrix);
            if(color.HasValue)
                shader.Set("ourColor", color.Value);
        }
    }
}
