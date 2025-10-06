using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeOEngine.Typedeaf.TypeOBasic2d
{
    namespace Entities.Drawables
    {
        /// <summary>
        /// Represents a two-dimensional drawable object with a position and size.
        /// </summary>
        /// <remarks>This is an abstract base class for drawable objects in a 2D space. Derived classes
        /// must implement the <see cref="Size"/> property to define the dimensions of the object.</remarks>
        public abstract class Drawable2d : Drawable
        {
            /// <summary>
            /// Gets or sets the position of the object in 2D space.
            /// </summary>
            public Vec2 Position { get; set; }

            /// <summary>
            /// Gets or sets the size of the object as a two-dimensional vector.
            /// </summary>
            public abstract Vec2 Size { get; protected set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Drawable2d"/> class.
            /// </summary>
            /// <remarks>This constructor sets the initial position of the drawable object to <see cref="Vec2.Zero"/>.</remarks>
            protected Drawable2d() : base()
            {
                Position = Vec2.Zero;
            }
        }
    }
}
