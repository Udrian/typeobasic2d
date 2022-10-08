using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
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
    public static class Canvas2d
    {
        private static void ActivateShader(Shader shader, TKCanvas tkCanvas, Vec2 pos, double rotation, Color? color)
        {
            shader.Use();
            var modelMatrix = Matrix4.CreateTranslation((float)pos.X, (float)pos.Y, 0) * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians((float)rotation));
            shader.Set("model", modelMatrix);
            shader.Set("view", tkCanvas.ViewMatrix);
            shader.Set("projection", tkCanvas.ProjectionMatrix);
            if(color.HasValue)
                shader.Set("ourColor", color.Value);
        }

        public static void DrawLine(this ICanvas canvas, Vec2 pos, Vec2 size, Color color, IAnchor2d anchor = null)
        {
            throw new NotImplementedException();
        }

        public static void DrawLineE(this ICanvas canvas, Vec2 from, Vec2 to, Color color, IAnchor2d anchor = null)
        {
            DrawLine(canvas, from, to - from, color, anchor);
        }

        public static void DrawLines(this ICanvas canvas, List<Vec2> points, Color color, IAnchor2d anchor = null)
        {
            throw new NotImplementedException();
        }

        public static void DrawPixel(this ICanvas canvas, Vec2 point, Color color, IAnchor2d anchor = null)
        {
            throw new NotImplementedException();
        }

        public static void DrawPixels(this ICanvas canvas, List<Vec2> points, Color color, IAnchor2d anchor = null)
        {
            throw new NotImplementedException();
        }

        internal static RectanglePrimitive RectanglePrimitive { get; private set; } = new RectanglePrimitive();
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

        public static void DrawRectangle(this ICanvas canvas, Rectangle rectangle, bool filled, Color color, IAnchor2d anchor = null)
        {
            DrawRectangle(canvas, rectangle.Pos, rectangle.Size, filled, color, anchor);
        }

        public static void DrawRectangleE(this ICanvas canvas, Vec2 from, Vec2 to, bool filled, Color color, IAnchor2d anchor = null)
        {
            DrawRectangle(canvas, from, to - from, filled, color, anchor);
        }

        internal static TexturedPrimitive TexturedPrimitive { get; private set; } = new TexturedPrimitive();
        public static void DrawImage(this ICanvas canvas, Texture texture, Vec2 pos, Vec2? scale = null, double rotation = 0, Vec2? origin = null, Color? color = null, Flipped flipped = Flipped.None, Rectangle? source = null, IAnchor2d anchor = null)
        {
            if (canvas is TKCanvas tkCanvas && texture is TKTexture tkTexture)
            {
                pos = (pos + anchor?.Position) ?? pos;
                pos = (pos + origin) ?? pos;

                ActivateShader(tkCanvas.TextureShader, tkCanvas, pos, rotation, color ?? Color.White);
                tkTexture.Use();

                TexturedPrimitive.PrimitiveDrawType = PrimitiveDrawType.Triangle;
                TexturedPrimitive.Size = texture.Size * scale??Vec2.One;
                TexturedPrimitive.Draw();
            }
        }

        public static void DrawImage(this ICanvas canvas, Texture texture, Vec2 pos, IAnchor2d anchor = null)
        {
            DrawImage(canvas, texture, pos, null, 0, null, null, Flipped.None, null, anchor);
        }

        public static void DrawText(this ICanvas canvas, Font font, string text, Vec2 pos, IAnchor2d anchor = null)
        {
            DrawText(canvas, font, text, pos, null, 0, null, null, Flipped.None, null, anchor);
        }

        public static void DrawText(this ICanvas canvas, Font font, string text, Vec2 pos, Vec2? scale = null, double rotation = 0, Vec2? origin = null, Color? color = null, Flipped flipped = Flipped.None, Rectangle? source = null, IAnchor2d anchor = null)
        {
            throw new NotImplementedException();
        }
    }
}
