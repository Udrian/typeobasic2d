using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine;

namespace TypeOEngine.Typedeaf.TypeOBasic2d
{
    namespace Entities.Drawables
    {
        /// <summary>
        /// Represents an option for configuring a 2D drawable object.
        /// </summary>
        /// <remarks>This class provides configuration options specific to 2D drawable objects, such as
        /// their position.</remarks>
        /// <typeparam name="D">The type of the drawable object, which must derive from <see cref="Drawable2d"/>.</typeparam>
        public class Drawable2dOption<D> : DrawableOption<D> where D : Drawable2d
        {
            /// <summary>
            /// Gets or sets the position of the object in 2D space.
            /// </summary>
            public Vec2? Position { get; set; }

            /// <summary>
            /// Assigns the specified position to the object and returns a value indicating whether the operation was
            /// successful.
            /// </summary>
            /// <param name="obj">The object to which the position will be assigned.</param>
            /// <returns><see langword="true"/> if the operation was successful; otherwise, <see langword="false"/>.</returns>
            public override bool Create(D obj)
            {
                if(Position.HasValue)
                    obj.Position = Position.Value;
                return true;
            }
        }
    }
}
