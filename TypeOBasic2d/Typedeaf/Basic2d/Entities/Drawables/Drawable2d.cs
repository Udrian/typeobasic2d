using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeOEngine.Typedeaf.TypeOBasic2d
{
    namespace Entities.Drawables
    {
        public abstract class Drawable2d : Drawable
        {
            public Vec2 Position { get; set; }
            public abstract Vec2 Size { get; protected set; }

            protected Drawable2d() : base()
            {
                Position = Vec2.Zero;
            }
        }
    }
}
