using System;
using System.Collections.Generic;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.TK.Engine.Graphics;
using TypeOEngine.Typedeaf.TK.Engine.Graphics.Primitives;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Interfaces;

namespace TypeOEngine.Typedeaf.Basic2d.Engine.Graphics
{
    /// <summary>
    /// Extension class for ICanvas
    /// </summary>
    public static partial class Canvas2d
    {
        internal static Primitive<Vertex> LinePrimitive { get; private set; } = new Primitive<Vertex>(2, PrimitiveDrawType.Line);

        /// <summary>
        /// Draws a line given a position and a size
        /// </summary>
        /// <param name="canvas">Canvas to draw the line on</param>
        /// <param name="pos">Start position</param>
        /// <param name="size">Length vector of the line</param>
        /// <param name="color">Color that the line will have</param>
        /// <param name="anchor">Transform position anchor</param>
        /// <exception cref="NotImplementedException"></exception>
        public static void DrawLine(this ICanvas canvas, Vec2 pos, Vec2 size, Color color, IAnchor2d anchor = null)
        {
            if (canvas is TKCanvas tkCanvas)
            {
                pos = (pos + anchor?.Position) ?? pos;
                LinePrimitive.Vertices[0].Position = new Vec3(pos);
                LinePrimitive.Vertices[1].Position = new Vec3(pos + size);
                ActivateShader(tkCanvas.Shader, tkCanvas, Vec2.Zero, 0, color);

                LinePrimitive.Draw();
            }
        }

        /// <summary>
        /// Draws a line given absolute starting and end position
        /// </summary>
        /// <param name="canvas">Canvas to draw the line on</param>
        /// <param name="from">Absolute start position</param>
        /// <param name="to">Absolute end position</param>
        /// <param name="color">Color that the line will have</param>
        /// <param name="anchor">Transform position anchor</param>
        public static void DrawLineA(this ICanvas canvas, Vec2 from, Vec2 to, Color color, IAnchor2d anchor = null)
        {
            DrawLine(canvas, from, to - from, color, anchor);
        }

        /// <summary>
        /// Draw multiple lines given a vector of points
        /// </summary>
        /// <param name="canvas">Canvas to draw the line on</param>
        /// <param name="points">Vector of points describing the shape of the lines being drawn</param>
        /// <param name="color">Color that the line will have</param>
        /// <param name="anchor">Transform position anchor</param>
        /// <exception cref="NotImplementedException"></exception>
        public static void DrawLines(this ICanvas canvas, List<Vec2> points, Color color, IAnchor2d anchor = null)
        {
            if(points.Count == 0) return;
            if (points.Count == 1)
            {
                DrawLineA(canvas, points[0], points[0], color, anchor);
                return;
            }

            for (int i = 1; i < points.Count; i++)
            {
                DrawLineA(canvas, points[i - 1], points[i], color, anchor);
            }
        }
    }
}
