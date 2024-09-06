using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.TK.Contents;
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
        /// <summary>
        /// Draws a text on screen
        /// </summary>
        /// <param name="canvas">Canvas to draw the text on</param>
        /// <param name="font">Font to use for the text</param>
        /// <param name="text">Text to draw</param>
        /// <param name="pos">Left right corner of the text</param>
        /// <param name="scale">Font scaling</param>
        /// <param name="rotation">Text rotation</param>
        /// <param name="origin">Text position origin</param>
        /// <param name="color">Color overlay for the text</param>
        /// <param name="flipped">Flipped position of text, vertically, horizontally or both</param>
        /// <param name="source">Rectangle within where the text should be contained, will clip text outside of the bounds</param>
        /// <param name="anchor">Transform position anchor</param>
        public static void DrawText(this ICanvas canvas, string text, Font font, Vec2 pos, Vec2? scale = null, double rotation = 0, Vec2? origin = null, Color? color = null, Flipped flipped = Flipped.None, Rectangle? source = null, IAnchor2d anchor = null)
        {
            if (canvas is TKCanvas tkCanvas && font is TKFont tkFont)
            {
                pos = (pos + origin) ?? pos;
                tkFont.Draw(text, new Vec2(pos.X, tkCanvas.Viewport.Size.Y - pos.Y), color ?? Color.White);
            }
        }
    }
}
