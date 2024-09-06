using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.TK.Contents;
using TypeOEngine.Typedeaf.TK.Engine.Graphics;
using TypeOEngine.Typedeaf.TK.Engine.Graphics.Primitives;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Interfaces;

namespace TypeOEngine.Typedeaf.Basic2d.Engine.Graphics
{
    /// <summary>
    /// Extension class for ICanvas
    /// </summary>
    public static partial class Canvas2d
    {
        internal static TexturedPrimitive TexturedPrimitive { get; private set; } = new TexturedPrimitive();

        /// <summary>
        /// Draws a texture on screen
        /// </summary>
        /// <param name="canvas">Canvas to draw the texture on</param>
        /// <param name="texture">Texture to draw</param>
        /// <param name="pos">Left right corner of the texture</param>
        /// <param name="scale">Texture scale</param>
        /// <param name="rotation">Texture rotation</param>
        /// <param name="origin">Texture position origin</param>
        /// <param name="color">Color overlay for the texture</param>
        /// <param name="flipped">Flipped position of texture, vertically, horizontally or both</param>
        /// <param name="source">Rectangle within where the texture should be contained, will clip texture outside of the bounds</param>
        /// <param name="anchor">Transform position anchor</param>
        public static void DrawImage(this ICanvas canvas, Texture texture, Vec2 pos, Vec2? scale = null, double rotation = 0, Vec2? origin = null, Color? color = null, Flipped flipped = Flipped.None, Rectangle? source = null, IAnchor2d anchor = null)
        {
            if (canvas is TKCanvas tkCanvas && texture is TKTexture tkTexture)
            {
                pos = (pos + anchor?.Position) ?? pos;
                pos = (pos + origin) ?? pos;

                ActivateShader(tkCanvas.TextureShader, tkCanvas, pos, rotation, color ?? Color.White);
                tkTexture.Use();

                TexturedPrimitive.PrimitiveDrawType = PrimitiveDrawType.Triangle;
                TexturedPrimitive.Size = texture.Size * (scale ?? Vec2.One);
                TexturedPrimitive.Draw();
            }
        }
    }
}
