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
        internal static RectanglePrimitive RectanglePrimitive { get; private set; } = new RectanglePrimitive();

        /// <summary>
        /// Draws a rectangle
        /// </summary>
        /// <param name="canvas">Canvas to draw the rectangle on</param>
        /// <param name="pos">Starting position of the rectangle</param>
        /// <param name="size">Rectangle size</param>
        /// <param name="filled">Draws the rectangle filled or just the outline</param>
        /// <param name="color">Color of the rectangle</param>
        /// <param name="anchor">Transform position anchor</param>
        public static void DrawRectangle(this ICanvas canvas, Vec2 pos, Vec2 size, bool filled, Color color, IAnchor2d anchor = null)
        {
            if (canvas is TKCanvas tkCanvas)
            {
                pos = (pos + anchor?.Position) ?? pos;
                ActivateShader(tkCanvas.Shader, tkCanvas, pos, 0, color);

                RectanglePrimitive.PrimitiveDrawType = filled ? PrimitiveDrawType.Triangle : PrimitiveDrawType.Line;
                RectanglePrimitive.Size = size;
                RectanglePrimitive.Draw();
            }
        }

        /// <summary>
        /// Draws a rectangle
        /// </summary>
        /// <param name="canvas">Canvas to draw the rectangle on</param>
        /// <param name="rectangle">Position and size of the rectangle</param>
        /// <param name="filled">Draws the rectangle filled or just the outline</param>
        /// <param name="color">Color of the rectangle</param>
        /// <param name="anchor">Transform position anchor</param>
        public static void DrawRectangle(this ICanvas canvas, Rectangle rectangle, bool filled, Color color, IAnchor2d anchor = null)
        {
            DrawRectangle(canvas, rectangle.Pos, rectangle.Size, filled, color, anchor);
        }

        /// <summary>
        /// Draws a rectangle given absolute starting and end position
        /// </summary>
        /// <param name="canvas">Canvas to draw the rectangle on</param>
        /// <param name="from">Absolute start position</param>
        /// <param name="to">Absolute end position</param>
        /// <param name="filled">Draws the rectangle filled or just the outline</param>
        /// <param name="color">Color of the rectangle</param>
        /// <param name="anchor">Transform position anchor</param>
        public static void DrawRectangleA(this ICanvas canvas, Vec2 from, Vec2 to, bool filled, Color color, IAnchor2d anchor = null)
        {
            DrawRectangle(canvas, from, to - from, filled, color, anchor);
        }
    }
}
