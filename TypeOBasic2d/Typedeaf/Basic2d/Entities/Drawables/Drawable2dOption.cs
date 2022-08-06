using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine;

namespace TypeOEngine.Typedeaf.TypeOBasic2d
{
    namespace Entities.Drawables
    {
        public class Drawable2dOption<D> : DrawableOption<D> where D : Drawable2d
        {
            public Vec2? Position { get; set; }

            public override bool Create(D obj)
            {
                if(Position.HasValue)
                    obj.Position = Position.Value;
                return true;
            }
        }
    }
}
