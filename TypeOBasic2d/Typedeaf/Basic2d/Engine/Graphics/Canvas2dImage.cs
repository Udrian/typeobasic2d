using OpenTK.Graphics.OpenGL4;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities;
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
                // The clip area is in the same space as the position, so it gets the same offsets
                var offset = (anchor?.Position ?? Vec2.Zero) + (origin ?? Vec2.Zero);
                pos += offset;

                // Enable scissor test if source clipping is specified
                if (source.HasValue)
                {
                    var sourceRect = source.Value;
                    EnableScissorTest(tkCanvas, ToScreen(tkCanvas, new Rectangle(sourceRect.Pos + offset, sourceRect.Size)));
                }

                ActivateShader(tkCanvas.TextureShader, tkCanvas, pos, rotation, color ?? Color.White);
                tkTexture.Use();

                TexturedPrimitive.PrimitiveDrawType = PrimitiveDrawType.Triangle;
                TexturedPrimitive.Size = texture.Size * (scale ?? Vec2.One);
                
                // Apply flipping by modifying texture coordinates
                ApplyFlipping(TexturedPrimitive, flipped);
                
                TexturedPrimitive.Draw();
                
                // Reset texture coordinates to default state for next draw
                if (flipped != Flipped.None)
                    ResetFlipping(TexturedPrimitive);

                // Disable scissor test after drawing
                if (source.HasValue)
                {
                    DisableScissorTest();
                }
            }
        }

        /// <summary>
        /// Converts a world rectangle to the screen with the canvas camera (world translation and scale), like the view matrix does.
        /// </summary>
        /// <param name="canvas">The canvas whose camera to apply</param>
        /// <param name="worldRect">The rectangle in world coordinates</param>
        /// <returns>The rectangle in screen coordinates</returns>
        internal static Rectangle ToScreen(ICanvas canvas, Rectangle worldRect)
        {
            var scale = canvas.WorldScale;
            var translation = new Vec2(canvas.WorldTranslation.X, canvas.WorldTranslation.Y);
            return new Rectangle((worldRect.Pos - translation) * scale, worldRect.Size * scale);
        }

        /// <summary>
        /// Enables scissor test for clipping drawing to a specific rectangle.
        /// </summary>
        /// <param name="canvas">The canvas to apply scissor test on</param>
        /// <param name="clipRect">The rectangle to clip drawing to, in screen coordinates</param>
        private static void EnableScissorTest(TKCanvas canvas, Rectangle clipRect)
        {
            var (x, y, width, height) = ToScissor(canvas.Viewport.Size, clipRect);
            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(x, y, width, height);
        }

        /// <summary>
        /// Converts a screen rectangle to an OpenGL scissor box, clamped to the viewport.
        /// OpenGL scissor coordinates have origin at bottom-left, Y increases upward.
        /// </summary>
        internal static (int X, int Y, int Width, int Height) ToScissor(Vec2 viewportSize, Rectangle clipRect)
        {
            var left = System.Math.Max(0, System.Math.Floor(clipRect.Pos.X));
            var top = System.Math.Max(0, System.Math.Floor(clipRect.Pos.Y));
            var right = System.Math.Min(viewportSize.X, System.Math.Ceiling(clipRect.Pos.X + clipRect.Size.X));
            var bottom = System.Math.Min(viewportSize.Y, System.Math.Ceiling(clipRect.Pos.Y + clipRect.Size.Y));

            // A clip area outside the viewport hides everything rather than becoming a negative box
            var width = (int)System.Math.Max(0, right - left);
            var height = (int)System.Math.Max(0, bottom - top);
            return ((int)left, (int)(viewportSize.Y - top - height), width, height);
        }

        /// <summary>
        /// Disables scissor test after clipping is complete.
        /// </summary>
        private static void DisableScissorTest()
        {
            GL.Disable(EnableCap.ScissorTest);
        }

        /// <summary>
        /// Applies flipping transformations to the texture coordinates of a TexturedPrimitive.
        /// </summary>
        /// <param name="primitive">The TexturedPrimitive to modify</param>
        /// <param name="flipped">The flipping mode to apply</param>
        private static void ApplyFlipping(TexturedPrimitive primitive, Flipped flipped)
        {
            // Default texture coordinates (no flip)
            // Vertex 0: (1, 0) - top right
            // Vertex 1: (1, 1) - bottom right
            // Vertex 2: (0, 1) - bottom left
            // Vertex 3: (0, 0) - top left

            if (flipped == Flipped.None)
                return;

            bool flipHorizontal = flipped == Flipped.Horizontal || flipped == Flipped.Both;
            bool flipVertical = flipped == Flipped.Vertical || flipped == Flipped.Both;

            // Flip by inverting the appropriate coordinates
            for (int i = 0; i < 4; i++)
            {
                var texCoord = primitive.Vertices[i].TexCoord;
                
                if (flipHorizontal)
                    texCoord.X = 1 - texCoord.X;
                
                if (flipVertical)
                    texCoord.Y = 1 - texCoord.Y;
                
                primitive.Vertices[i].TexCoord = texCoord;
            }
        }

        /// <summary>
        /// Resets the texture coordinates of a TexturedPrimitive to their default state.
        /// </summary>
        /// <param name="primitive">The TexturedPrimitive to reset</param>
        private static void ResetFlipping(TexturedPrimitive primitive)
        {
            primitive.Vertices[0].TexCoord = new Vec2(1, 0);  // top right
            primitive.Vertices[1].TexCoord = new Vec2(1, 1);  // bottom right
            primitive.Vertices[2].TexCoord = new Vec2(0, 1);  // bottom left
            primitive.Vertices[3].TexCoord = new Vec2(0, 0);  // top left
        }
    }
}
