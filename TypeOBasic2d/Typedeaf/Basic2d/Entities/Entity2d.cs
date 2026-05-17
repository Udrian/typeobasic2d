using TypeOEngine.Typedeaf.Core.Attributes;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Entities;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Interfaces;

namespace TypeOEngine.Typedeaf.TypeOBasic2d
{
    namespace Entities
    {
        /// <summary>
        /// Represents a 2D entity with position, scale, rotation, size, and origin properties.
        /// </summary>
        public abstract class Entity2d : Entity, IAnchor2d
        {
            /// <summary>
            /// Gets or sets the position represented by a two-dimensional vector.
            /// </summary>
            [TypeOProperty("Gets or sets the position represented by a two-dimensional vector.")]
            public virtual Vec2   Position { get; set; }
            /// <summary>
            /// Gets or sets the scaling factor applied to the object along the X and Y axes.
            /// </summary>
            [TypeOProperty("Gets or sets the scaling factor applied to the object along the X and Y axes.")]
            public virtual Vec2   Scale    { get; set; }
            /// <summary>
            /// Gets or sets the rotation angle, in degrees, applied to the object.
            /// </summary>
            [TypeOProperty("Gets or sets the rotation angle, in degrees, applied to the object.")]
            public virtual double Rotation { get; set; }
            /// <summary>
            /// Gets or sets the two-dimensional size of the object.
            /// </summary>
            [TypeOProperty("Gets or sets the two-dimensional size of the object.")]
            public virtual Vec2   Size     { get; set; }
            /// <summary>
            /// Gets or sets the origin point of the object in 2D space.
            /// </summary>
            [TypeOProperty("Gets or sets the origin point of the object in 2D space.")]
            public virtual Vec2   Origin   { get; set; }

            /// <summary>
            /// Initializes a new instance of the Entity2d class with default position, scale, rotation, size, and
            /// origin values.
            /// </summary>
            /// <remarks>This protected constructor is intended for use by derived classes to ensure
            /// that all spatial properties are initialized to their default states.</remarks>
            protected Entity2d() : base()
            {
                Position = Vec2.Zero;
                Scale    = Vec2.One;
                Rotation = 0;
                Size     = Vec2.Zero;
                Origin   = Vec2.Zero;
            }

            /// <summary>
            /// Gets or sets the bounding rectangle of the entity in screen coordinates.
            /// </summary>
            /// <remarks>Setting this property updates the entity's position and size relative to its
            /// parent. The screen bounds are calculated based on the entity's position and size, as well as the screen
            /// bounds of its parent if applicable.</remarks>
            public Rectangle ScreenBounds {
                get {
                    return new Rectangle(
                           Position + ((Parent as Entity2d)?.ScreenBounds.Pos  ?? Vec2.Zero),
                           Size//     + (Parent?.DrawBounds.Size ?? Vec2.Zero)
                        );
                }
                set {
                    Position = value.Pos  - ((Parent as Entity2d)?.ScreenBounds.Pos  ?? Vec2.Zero);
                    Size     = value.Size;// - (Parent?.DrawBounds.Size ?? Vec2.Zero);
                }
            }

            /// <summary>
            /// Creates a new 2D anchor at the specified position with the given orientation and orientation type.
            /// </summary>
            /// <param name="anchorPosition">The position of the anchor in 2D space.</param>
            /// <param name="orientation">The orientation of the anchor relative to its position. Defaults to Orientation2d.UpperLeft.</param>
            /// <param name="orientationType">Specifies whether the orientation is interpreted as absolute or relative. Defaults to
            /// OrientationType.Absolute.</param>
            /// <returns>A new Anchor2d instance configured with the specified position, orientation, and orientation type.</returns>
            public Anchor2d CreateAnchor(Vec2 anchorPosition, Orientation2d orientation = Orientation2d.UpperLeft, OrientationType orientationType = OrientationType.Absolute)
            {
                return new Anchor2d(anchorPosition, orientation, orientationType, this);
            }
        }
    }
}