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
        internal static Primitive<Vertex> PixelPrimitive { get; private set; } = new Primitive<Vertex>(1, PrimitiveDrawType.Point);
        internal static Dictionary<int, Primitive<Vertex>> MultiPixelPrimitive { get; private set; } = new Dictionary<int, Primitive<Vertex>>();

        /// <summary>
        /// Draws a pixel at a given point
        /// </summary>
        /// <param name="canvas">Canvas to draw the pixel on</param>
        /// <param name="point">Pixel position on the canvas</param>
        /// <param name="color">Color of the pixel</param>
        /// <param name="anchor">Transform position anchor</param>
        public static void DrawPixel(this ICanvas canvas, Vec2 point, Color color, IAnchor2d anchor = null)
        {
            if (canvas is TKCanvas tkCanvas)
            {
                var pos = (point + anchor?.Position) ?? point;
                PixelPrimitive.Vertices[0].Position = new Vec3(pos);
                ActivateShader(tkCanvas.Shader, tkCanvas, Vec2.Zero, 0, color);

                PixelPrimitive.Draw();
            }
        }

        /// <summary>
        /// Draws multiple pixels given a vector of points
        /// </summary>
        /// <param name="canvas">Canvas to draw the pixels on</param>
        /// <param name="points">Vector of points describing where to draw the pixels</param>
        /// <param name="color">Color of the pixels</param>
        /// <param name="anchor">Transform position anchor</param>
        public static void DrawPixels(this ICanvas canvas, List<Vec2> points, Color color, IAnchor2d anchor = null)
        {
            if (canvas is TKCanvas tkCanvas)
            {
                if(!MultiPixelPrimitive.ContainsKey(points.Count))
                {
                    MultiPixelPrimitive.Add(points.Count, new Primitive<Vertex>(points.Count, PrimitiveDrawType.Point));
                }
                var pixelPrimitive = MultiPixelPrimitive[points.Count];

                int i = 0;
                foreach(var point in points)
                {
                    var pos = (point + anchor?.Position) ?? point;
                    pixelPrimitive.Vertices[i].Position = new Vec3(pos);
                    i++;
                }
                ActivateShader(tkCanvas.Shader, tkCanvas, Vec2.Zero, 0, color);

                pixelPrimitive.Draw();
            }
        }
    }
}
