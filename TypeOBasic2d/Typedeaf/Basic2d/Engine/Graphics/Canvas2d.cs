using OpenTK.Mathematics;
using TypeOEngine.Typedeaf.Core.Common;
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
        internal static Vec2 GetAnchorOffset(IAnchor2d anchor)
        {
            return anchor is Entity2d ? Vec2.Zero : anchor?.Position ?? Vec2.Zero;
        }

        private static Matrix4 GetAnchorMatrix(IAnchor2d anchor)
        {
            return anchor is Entity2d entity ? entity.ModelMatrix : Matrix4.Identity;
        }

        private static void ActivateShader(Shader shader, TKCanvas tkCanvas, Vec2 pos, double rotation, Color? color, Vec2? origin = null, IAnchor2d anchor = null)
        {
            shader.Use();
            var rotationOrigin = origin ?? Vec2.Zero;
            var modelMatrix =
                Matrix4.CreateTranslation((float)-rotationOrigin.X, (float)-rotationOrigin.Y, 0) *
                Matrix4.CreateRotationZ(MathHelper.DegreesToRadians((float)rotation)) *
                Matrix4.CreateTranslation((float)(pos.X + rotationOrigin.X), (float)(pos.Y + rotationOrigin.Y), 0);
            modelMatrix *= GetAnchorMatrix(anchor);
            shader.Set("model", modelMatrix);
            shader.Set("view", tkCanvas.ViewMatrix);
            shader.Set("projection", tkCanvas.ProjectionMatrix);
            if(color.HasValue)
                shader.Set("ourColor", color.Value);
        }
    }
}
