using OpenTK.Mathematics;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Core.Engine.Graphics;
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
            if (CanvasDrawCommandQueue.TryEnqueue(() => canvas.DrawText(text, font, pos, scale, rotation, origin, color, flipped, source, anchor)))
                return;

            CanvasDrawCommandQueue.FlushPendingBatch();
            if (canvas is TKCanvas tkCanvas && font is TKFont tkFont)
            {
                var position = pos + GetAnchorOffset(anchor);
                var originOffset = origin ?? Vec2.Zero;
                var modelMatrix =
                    Matrix4.CreateTranslation((float)-originOffset.X, (float)-originOffset.Y, 0) *
                    Matrix4.CreateRotationZ(MathHelper.DegreesToRadians((float)rotation)) *
                    Matrix4.CreateTranslation((float)(position.X + originOffset.X), (float)(position.Y + originOffset.Y), 0);
                modelMatrix *= GetAnchorMatrix(anchor);

                Rectangle? clipRect = source.HasValue
                    ? new Rectangle(
                        source.Value.Pos - pos,
                        source.Value.Size)
                    : null;

                DrawWithEntityClipping(canvas, anchor, () =>
                    tkFont.DrawTransformed(
                        text,
                        scale ?? Vec2.One,
                        color ?? Color.White,
                        flipped,
                        modelMatrix,
                        new Vec2(tkCanvas.WorldTranslation.X, tkCanvas.WorldTranslation.Y),
                        tkCanvas.WorldScale,
                        (float)tkCanvas.Viewport.Size.Y,
                        clipRect
                    ));
            }
        }
    }
}
