using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.TK.Engine.Graphics;
using TypeOTK.Typedeaf.TK.Engine.Graphics;
using Color = TypeOEngine.Typedeaf.Core.Common.Color;

namespace TypeOEngine.Typedeaf.Basic2d.Engine.Graphics
{
    /// <summary>
    /// Draws triangles with a position and texture coordinate per vertex for textured polygons.
    /// </summary>
    internal sealed class Canvas2dPolygonTextureBuffer
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TextureVertex
        {
            public double X;
            public double Y;
            public double Z;
            public double U;
            public double V;
            public byte R;
            public byte G;
            public byte B;
            public byte A;
        }

        private TextureVertex[] _vertices = Array.Empty<TextureVertex>();
        private int _vertexArrayId;
        private int _vertexBufferId;
        private int _contextGeneration;

        /// <summary>
        /// Draws the triangles given by <paramref name="indices"/>. The texture and the shader's matrices must already be set.
        /// </summary>
        public void Draw(Vec2[] points, Vec2[] uvs, int[] indices, Color[] colors = null)
        {
            if (_vertices.Length < indices.Length)
                Array.Resize(ref _vertices, indices.Length);

            for (var i = 0; i < indices.Length; i++)
            {
                var index = indices[i];
                var color = colors != null ? colors[index] : Color.White;
                _vertices[i] = new TextureVertex { X = points[index].X, Y = points[index].Y, U = uvs[index].X, V = uvs[index].Y, R = color.R, G = color.G, B = color.B, A = color.A };
            }

            EnsureBuffers();
            GL.BindVertexArray(_vertexArrayId);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferId);
            GL.BufferData(BufferTarget.ArrayBuffer, indices.Length * Marshal.SizeOf<TextureVertex>(), _vertices, BufferUsageHint.StreamDraw);
            GL.DrawArrays(PrimitiveType.Triangles, 0, indices.Length);
            TKGLHelper.CheckGLError();
        }

        private void EnsureBuffers()
        {
            if (_vertexArrayId != 0 && _contextGeneration == TKGLHelper.ContextGeneration)
                return;

            _vertexArrayId = GL.GenVertexArray();
            _vertexBufferId = GL.GenBuffer();
            GL.BindVertexArray(_vertexArrayId);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferId);

            var stride = Marshal.SizeOf<TextureVertex>();
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Double, false, stride, 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Double, false, stride, sizeof(double) * 3);
                        GL.EnableVertexAttribArray(2);
                        GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, sizeof(double) * 5);

            _contextGeneration = TKGLHelper.ContextGeneration;
        }
    }
}
