using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Contents;

namespace TypeOEngine.Typedeaf.TypeOBasic2d
{
    namespace Entities.Drawables
    {
        public class DrawableFontOption : Drawable2dOption<DrawableFont>
        {
            public Font Font { get; set; }
            public string Text { get; set; }
            public Vec2? Scale { get; set; }
            public double? Rotation { get; set; }
            public Color? Color { get; set; }
            public Flipped? Flipped { get; set; }

            public override bool Create(DrawableFont obj)
            {
                if(!base.Create(obj)) return false;

                if(Font != null)
                    obj.Font = Font;
                if(Text != null)
                    obj.Text = Text;
                if(Scale.HasValue)
                    obj.Scale = Scale.Value;
                if(Rotation.HasValue)
                    obj.Rotation = Rotation.Value;
                if(Color.HasValue)
                    obj.Color = Color.Value;
                if(Flipped.HasValue)
                    obj.Flipped = Flipped.Value;

                return true;
            }
        }
    }
}
