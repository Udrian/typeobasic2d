using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.TK.Engine.Graphics;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Interfaces;

namespace TypeOEngine.Typedeaf.Basic2d.Engine.Graphics
{
    /// <summary>
    /// Extension class for ICanvas
    /// </summary>
    public static class Canvas2d
    {
        public static void DrawLine(this ICanvas canvas, Vec2 from, Vec2 size, Color color, IAnchor2d anchor = null)
        {
            throw new NotImplementedException();
        }

        public static void DrawLineE(this ICanvas canvas, Vec2 from, Vec2 to, Color color, IAnchor2d anchor = null)
        {
            throw new NotImplementedException();
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

        public static void DrawRectangle(this ICanvas canvas, Rectangle rectangle, bool filled, Color color, IAnchor2d anchor = null)
        {
            throw new NotImplementedException();
        }

        public static void DrawRectangle(this ICanvas canvas, Vec2 from, Vec2 size, bool filled, Color color, IAnchor2d anchor = null)
        {
            if(canvas is TKCanvas tkCanvas)
            {
                tkCanvas.Shader.Use();
                var modelMatrix = Matrix4.Identity * Matrix4.CreateTranslation((float)from.X, (float)from.Y, 0);
                tkCanvas.Shader.Set("model", modelMatrix);
                tkCanvas.Shader.Set("view", tkCanvas.ViewMatrix);
                tkCanvas.Shader.Set("projection", tkCanvas.ProjectionMatrix);
                tkCanvas.Shader.Set("ourColor", color);

                tkCanvas.RectanglePrimitive.Size = size;

                tkCanvas.RectanglePrimitive.InternalDraw();
            }
        }

        public static void DrawRectangleE(this ICanvas canvas, Vec2 from, Vec2 to, bool filled, Color color, IAnchor2d anchor = null)
        {
            throw new NotImplementedException();
        }

        public static void DrawImage(this ICanvas canvas, Texture texture, Vec2 pos, IAnchor2d anchor = null)
        {
            throw new NotImplementedException();
        }

        public static void DrawImage(this ICanvas canvas, Texture texture, Vec2 pos, Vec2? scale = null, double rotation = 0, Vec2? origin = null, Color? color = null, Flipped flipped = Flipped.None, Rectangle? source = null, IAnchor2d anchor = null)
        {
            throw new NotImplementedException();
        }

        public static void DrawText(this ICanvas canvas, Font font, string text, Vec2 pos, IAnchor2d anchor = null)
        {
            throw new NotImplementedException();
        }

        public static void DrawText(this ICanvas canvas, Font font, string text, Vec2 pos, Vec2? scale = null, double rotation = 0, Vec2? origin = null, Color? color = null, Flipped flipped = Flipped.None, Rectangle? source = null, IAnchor2d anchor = null)
        {
            throw new NotImplementedException();
        }
    }
}
