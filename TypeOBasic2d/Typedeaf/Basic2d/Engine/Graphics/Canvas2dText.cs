using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities;
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
        /// <param name="origin">Local text offset used as the rotation pivot</param>
        /// <param name="color">Color overlay for the text</param>
        /// <param name="flipped">Flipped position of text, vertically, horizontally or both</param>
        /// <param name="source">Canvas-space rectangle that clips the glyphs before rotation</param>
        /// <param name="anchor">Transform position anchor</param>
        public static void DrawText(this ICanvas canvas, string text, Font font, Vec2 pos, Vec2? scale = null, double rotation = 0, Vec2? origin = null, Color? color = null, Flipped flipped = Flipped.None, Rectangle? source = null, IAnchor2d anchor = null)
        {
            if (canvas is TKCanvas tkCanvas && font is TKFont tkFont)
            {
                var worldPosition = pos + (anchor?.Position ?? Vec2.Zero);
                var worldOrigin = worldPosition + (origin ?? Vec2.Zero);

                var translation = new Vec2(tkCanvas.WorldTranslation.X, tkCanvas.WorldTranslation.Y);
                var worldPos = (worldPosition - translation) * tkCanvas.WorldScale;
                var worldPivot = (worldOrigin - translation) * tkCanvas.WorldScale;
                var originOffset = worldPivot - worldPos;
                Rectangle? clipRect = source.HasValue
                    ? new Rectangle(
                        (source.Value.Pos - pos) * tkCanvas.WorldScale,
                        source.Value.Size * tkCanvas.WorldScale)
                    : null;

                tkFont.Draw(
                    text,
                    new Vec2(worldPos.X, tkCanvas.Viewport.Size.Y - worldPos.Y),
                    (scale ?? Vec2.One) * tkCanvas.WorldScale,
                    color ?? Color.White,
                    flipped,
                    rotation,
                    originOffset,
                    clipRect
                );
            }
        }
    }
}
