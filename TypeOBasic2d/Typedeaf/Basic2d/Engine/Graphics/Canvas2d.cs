using OpenTK.Mathematics;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.TK.Engine.Graphics;

namespace TypeOEngine.Typedeaf.Basic2d.Engine.Graphics
{
    /// <summary>
    /// Extension class for ICanvas
    /// </summary>
    public static partial class Canvas2d
    {
        private static void ActivateShader(Shader shader, TKCanvas tkCanvas, Vec2 pos, double rotation, Color? color, Vec2? origin = null)
        {
            shader.Use();
            var rotationOrigin = origin ?? Vec2.Zero;
            var modelMatrix =
                Matrix4.CreateTranslation((float)-rotationOrigin.X, (float)-rotationOrigin.Y, 0) *
                Matrix4.CreateRotationZ(MathHelper.DegreesToRadians((float)rotation)) *
                Matrix4.CreateTranslation((float)(pos.X + rotationOrigin.X), (float)(pos.Y + rotationOrigin.Y), 0);
            shader.Set("model", modelMatrix);
            shader.Set("view", tkCanvas.ViewMatrix);
            shader.Set("projection", tkCanvas.ProjectionMatrix);
            if(color.HasValue)
                shader.Set("ourColor", color.Value);
        }
    }
}
