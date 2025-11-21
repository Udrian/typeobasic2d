using TypeOEngine.Typedeaf.Core.Common;

namespace TypeOEngine.Typedeaf.TypeOBasic2d
{
    namespace Entities.Interfaces
    {
        /// <summary>
        /// Represents a 2D anchor with a position and screen bounds.
        /// </summary>
        /// <remarks>This interface defines the properties required to describe an anchor in a 2D space, 
        /// including its position and the bounds it occupies on the screen.</remarks>
        public interface IAnchor2d
        {
            /// <summary>
            /// Gets or sets the position of the object in 2D space.
            /// </summary>
            public Vec2 Position { get; set; }

            /// <summary>
            /// Gets or sets the dimensions and position of the screen bounds.
            /// </summary>
            public Rectangle ScreenBounds { get; set; }
        }
    }
}
