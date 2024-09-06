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
    }
}
