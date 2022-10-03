using TypeOEngine.Typedeaf.Basic2d.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;

namespace TypeOEngine.Typedeaf.TypeOBasic2d
{
    namespace Entities.Drawables
    {
        public class DrawableTexture : Drawable2d
        {
            public Texture Texture { get; set; }

            public Vec2 Scale { get; set; }
            public double Rotation { get; set; }
            public Color Color { get; set; }
            public Flipped Flipped { get; set; }

            public override Vec2 Size { get { return Texture.Size; } protected set { } }

            public DrawableTexture() : base()
            {
                Scale = Vec2.One;
                Rotation = 0;
                Color = Color.White;
                Flipped = Flipped.None;
            }

            public override void Initialize() { }

            public override void Cleanup()
            {
                Texture?.Cleanup();
            }

            public override void Draw(ICanvas canvas)
            {
                if (Texture == null) return;
                canvas.DrawImage(
                    Texture,
                    Position,
                    scale: Scale,
                    rotation: Rotation,
                    color: Color,
                    flipped: Flipped,
                    anchor: Entity as Entity2d
                );
            }
        }
    }
}
