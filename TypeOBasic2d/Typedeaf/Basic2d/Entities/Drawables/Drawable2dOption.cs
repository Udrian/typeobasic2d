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
        /// their position, origin, scale, and rotation.</remarks>
        /// <typeparam name="D">The type of the drawable object, which must derive from <see cref="Drawable2d"/>.</typeparam>
        public class Drawable2dOption<D> : DrawableOption<D> where D : Drawable2d
        {
            /// <summary>
            /// Gets or sets the position of the object in 2D space.
            /// </summary>
            public Vec2? Position { get; set; }

            /// <summary>
            /// Gets or sets the origin point that the Position refers to.
            /// </summary>
            public Vec2? Origin { get; set; }

            /// <summary>
            /// Gets or sets the scale of the object.
            /// </summary>
            public Vec2? Scale { get; set; }

            /// <summary>
            /// Gets or sets the rotation angle in degrees.
            /// </summary>
            public double? Rotation { get; set; }

            /// <summary>
            /// Assigns the specified position, origin, scale, and rotation to the object and returns a value indicating whether the operation was
            /// successful.
            /// </summary>
            /// <param name="obj">The object to which the properties will be assigned.</param>
            /// <returns><see langword="true"/> if the operation was successful; otherwise, <see langword="false"/>.</returns>
            public override bool Create(D obj)
            {
                if(Position.HasValue)
                    obj.Position = Position.Value;
                if(Origin.HasValue)
                    obj.Origin = Origin.Value;
                if(Scale.HasValue)
                    obj.Scale = Scale.Value;
                if(Rotation.HasValue)
                    obj.Rotation = Rotation.Value;
                return true;
            }
        }
    }
}
