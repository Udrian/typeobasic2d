using TypeOEngine.Typedeaf.Core.Common;

namespace TypeOEngine.Typedeaf.TypeOBasic2d
{
    namespace Entities.Interfaces
    {
        public interface IAnchor2d
        {
            public Vec2 Position { get; set; }

            public Rectangle ScreenBounds { get; set; }
        }
    }
}
