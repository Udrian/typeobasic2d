using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.TK;
using TypeOEngine.Typedeaf.TK.Engine.Graphics;
using TypeOTK.Typedeaf.TK.Engine.Graphics;

namespace TypeOEngine.Typedeaf.Basic2d.Engine.Graphics
{
    internal readonly record struct LineBatchCommand(Vec2 From, Vec2 To, Color Color);

    internal sealed class Canvas2dLineBatcher
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct BatchVertex
        {
            public double X;
            public double Y;
            public double Z;
            public byte R;
            public byte G;
            public byte B;
            public byte A;

            public BatchVertex(Vec2 position, Color color)
            {
                X = position.X;
                Y = position.Y;
                Z = 0;
                R = color.R;
                G = color.G;
                B = color.B;
                A = color.A;
            }
        }

        private BatchVertex[] _vertices = Array.Empty<BatchVertex>();
        private int _vertexArrayId;
        private int _vertexBufferId;
        private int _contextGeneration;

        public void Draw(TKCanvas canvas, IReadOnlyList<LineBatchCommand> lines)
        {
            var vertexCount = checked(lines.Count * 4);
            if (_vertices.Length < vertexCount)
                Array.Resize(ref _vertices, vertexCount);

            var vertexIndex = 0;
            foreach (var line in lines)
            {
                _vertices[vertexIndex++] = new BatchVertex(line.From, line.Color);
                _vertices[vertexIndex++] = new BatchVertex(line.To, line.Color);
                _vertices[vertexIndex++] = new BatchVertex(line.To, line.Color);
                _vertices[vertexIndex++] = new BatchVertex(line.From, line.Color);
            }

            EnsureBuffers();
            var shader = canvas.Shader;
            shader.Set("model", Matrix4.Identity);
            shader.Set("view", canvas.ViewMatrix);
            shader.Set("projection", canvas.ProjectionMatrix);
            shader.Set("ourColor", Color.White);
            shader.Set("useVertexColor", 1);

            try
            {
                GL.BindVertexArray(_vertexArrayId);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferId);
                GL.BufferData(
                    BufferTarget.ArrayBuffer,
                    vertexCount * Marshal.SizeOf<BatchVertex>(),
                    _vertices,
                    BufferUsageHint.StreamDraw);
                GL.DrawArrays(PrimitiveType.Lines, 0, vertexCount);
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

            var stride = Marshal.SizeOf<BatchVertex>();
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Double, false, stride, 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, true, stride, sizeof(double) * 3);

            _contextGeneration = TKGLHelper.ContextGeneration;
        }
    }
}
