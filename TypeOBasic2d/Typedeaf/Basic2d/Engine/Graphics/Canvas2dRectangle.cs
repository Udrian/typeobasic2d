using System.Runtime.CompilerServices;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Core.Engine.Graphics;
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
        private static readonly ConditionalWeakTable<TKCanvas, Canvas2dRectangleBatcher> RectangleBatchers = new();
        private static readonly Action<object, IReadOnlyList<RectangleBatchCommand>> FlushRectangleBatch = DrawRectangleBatch;

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
            DrawRectangle(canvas, pos, size, filled, color, 0, anchor);
        }

        /// <summary>
        /// Draws a rectangle with rotation around the specified local origin.
        /// </summary>
        /// <param name="canvas">Canvas to draw the rectangle on.</param>
        /// <param name="pos">Starting position of the rectangle.</param>
        /// <param name="size">Rectangle size.</param>
        /// <param name="filled">Whether to fill the rectangle or draw only its outline.</param>
        /// <param name="color">Color of the rectangle.</param>
        /// <param name="rotation">Rotation angle in degrees.</param>
        /// <param name="anchor">Transform position anchor.</param>
        public static void DrawRectangle(this ICanvas canvas, Vec2 pos, Vec2 size, bool filled, Color color, double rotation, IAnchor2d anchor = null)
        {
            DrawRectangle(canvas, pos, size, filled, color, rotation, null, anchor);
        }

        /// <summary>
        /// Draws a rectangle with rotation around a local origin.
        /// </summary>
        /// <param name="canvas">Canvas to draw the rectangle on.</param>
        /// <param name="pos">Starting position of the rectangle.</param>
        /// <param name="size">Rectangle size.</param>
        /// <param name="filled">Whether to fill the rectangle or draw only its outline.</param>
        /// <param name="color">Color of the rectangle.</param>
        /// <param name="rotation">Rotation angle in degrees.</param>
        /// <param name="anchor">Transform position anchor.</param>
        /// <param name="origin">Local position to rotate around.</param>
        public static void DrawRectangle(this ICanvas canvas, Vec2 pos, Vec2 size, bool filled, Color color, double rotation, Vec2? origin, IAnchor2d anchor = null)
        {
            if (CanvasDrawCommandQueue.TryEnqueue(() => canvas.DrawRectangle(pos, size, filled, color, rotation, origin, anchor)))
                return;

            if (canvas is TKCanvas tkCanvas)
            {
                if (filled && rotation == 0 && origin == null && anchor == null
                    && CanvasDrawCommandQueue.TryEnqueueBatch(
                        tkCanvas,
                        new RectangleBatchCommand(pos, size, color),
                        FlushRectangleBatch))
                    return;

                CanvasDrawCommandQueue.FlushPendingBatch();
                pos += GetAnchorOffset(anchor);
                DrawWithEntityClipping(canvas, anchor, () =>
                {
                    ActivateShader(tkCanvas.Shader, tkCanvas, pos, rotation, color, origin, anchor);
                    RectanglePrimitive.PrimitiveDrawType = filled ? PrimitiveDrawType.Triangle : PrimitiveDrawType.Line;
                    RectanglePrimitive.Size = size;
                    RectanglePrimitive.Draw();
                });
            }
        }

        private static void DrawRectangleBatch(object canvas, IReadOnlyList<RectangleBatchCommand> rectangles)
        {
            var tkCanvas = (TKCanvas)canvas;
            RectangleBatchers.GetValue(tkCanvas, static _ => new Canvas2dRectangleBatcher())
                .Draw(tkCanvas, rectangles);
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
