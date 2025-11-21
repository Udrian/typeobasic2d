using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Interfaces;

namespace TypeOEngine.Typedeaf.TypeOBasic2d
{
    namespace Entities
    {
        /// <summary>
        /// Represents a 2D anchor point that defines a position and orientation relative to a parent entity.
        /// </summary>
        /// <remarks>The <see cref="Anchor2d"/> class provides a way to specify a position and orientation
        /// in 2D space, either in absolute coordinates or as a fraction of the parent entity's size. The anchor's
        /// position and orientation are used to calculate its screen bounds relative to the parent entity.</remarks>
        public class Anchor2d : IAnchor2d
        {
            /// <summary>
            /// Gets the parent entity of the current 2D entity.
            /// </summary>
            public Entity2d Parent { get; private set; }

            /// <summary>
            /// Gets or sets the position of the object in 2D space.
            /// </summary>
            public Vec2 Position { get; set; }

            /// <summary>
            /// Gets or sets the 2D orientation of the object.
            /// </summary>
            public Orientation2d Orientation { get; set; }

            /// <summary>
            /// Gets or sets the orientation type of the object.
            /// </summary>
            public OrientationType OrientationType { get; set; }

            internal Anchor2d(Vec2 position, Orientation2d orientation, OrientationType orientationType, Entity2d parent)
            {
                Position = position;
                Orientation = orientation;
                OrientationType = orientationType;
                Parent = parent;
            }

            /// <summary>
            /// Gets or sets the screen-space bounds of the current object.
            /// </summary>
            /// <remarks>The bounds are determined relative to the parent object's screen bounds and
            /// the object's orientation. Setting this property adjusts the object's position relative to its
            /// parent.</remarks>
            public Rectangle ScreenBounds {
                get {
                    var pos = Vec2.Zero;
                    var parentPos = (Parent?.ScreenBounds.Pos ?? Vec2.Zero);
                    var parentSize = (Parent?.Size ?? Vec2.Zero);

                    switch(OrientationType)
                    {
                        case OrientationType.Absolute:
                            pos = Position;
                            break;
                        case OrientationType.Fraction:
                            pos = parentSize * Position;
                            break;
                        default:
                            break;
                    }
                    switch(Orientation)
                    {
                        case Orientation2d.UpperLeft:
                            pos = parentPos + pos;
                            break;
                        case Orientation2d.UpperRight:
                            pos = parentPos + new Vec2(parentSize.X - pos.X, pos.Y);
                            break;
                        case Orientation2d.LowerLeft:
                            pos = parentPos + new Vec2(pos.X, parentSize.Y - pos.Y);
                            break;
                        case Orientation2d.LowerRight:
                            pos = parentPos + (parentSize - pos);
                            break;
                        default:
                            break;
                    }
                    return new Rectangle(pos , Vec2.Zero);
                }
                set {
                    Position = value.Pos - (Parent?.ScreenBounds.Pos ?? Vec2.Zero);
                }
            }
        }
    }
}
