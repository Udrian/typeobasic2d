using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.TK.Engine.Graphics;
using TypeOTK.Typedeaf.TK.Engine.Graphics;
using Color = TypeOEngine.Typedeaf.Core.Common.Color;

namespace TypeOEngine.Typedeaf.Basic2d.Engine.Graphics
{
    /// <summary>
    /// Draws polygons whose vertices each carry their own color, which the shader interpolates across the polygon.
    /// </summary>
    internal sealed class Canvas2dPolygonColorBuffer
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct ColorVertex
        {
            public double X;
            public double Y;
            public double Z;
            public byte R;
            public byte G;
            public byte B;
            public byte A;
        }

        private ColorVertex[] _vertices = Array.Empty<ColorVertex>();
        private int _vertexArrayId;
        private int _vertexBufferId;
        private int _contextGeneration;

        /// <summary>
        /// Draws vertices in the order given by <paramref name="indices"/>, or in point order for an outline when it is null.
        /// The shader's model, view and projection must already be set.
        /// </summary>
        public void Draw(TKCanvas canvas, IReadOnlyList<Vec2> points, IReadOnlyList<Color> colors, int[] indices)
        {
            var count = indices?.Length ?? points.Count;
            if (_vertices.Length < count)
                Array.Resize(ref _vertices, count);

            for (var i = 0; i < count; i++)
            {
                var index = indices == null ? i : indices[i];
                var color = colors[index];
                _vertices[i] = new ColorVertex { X = points[index].X, Y = points[index].Y, R = color.R, G = color.G, B = color.B, A = color.A };
            }

            EnsureBuffers();
            var shader = canvas.Shader;
            shader.Set("ourColor", Color.White);
            shader.Set("useVertexColor", 1);
            try
            {
                GL.BindVertexArray(_vertexArrayId);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferId);
                GL.BufferData(BufferTarget.ArrayBuffer, count * Marshal.SizeOf<ColorVertex>(), _vertices, BufferUsageHint.StreamDraw);
                GL.DrawArrays(indices == null ? PrimitiveType.LineLoop : PrimitiveType.Triangles, 0, count);
                TKGLHelper.CheckGLError();
            }
            finally
            {
                shader.Set("useVertexColor", 0);
            }
        }

        private void EnsureBuffers()
        {
            if (_vertexArrayId != 0 && _contextGeneration == TKGLHelper.ContextGeneration)
                return;

            _vertexArrayId = GL.GenVertexArray();
            _vertexBufferId = GL.GenBuffer();
            GL.BindVertexArray(_vertexArrayId);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferId);

            var stride = Marshal.SizeOf<ColorVertex>();
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Double, false, stride, 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, true, stride, sizeof(double) * 3);

            _contextGeneration = TKGLHelper.ContextGeneration;
        }
    }
}
