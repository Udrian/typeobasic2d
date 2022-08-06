using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Interfaces;

namespace TypeOEngine.Typedeaf.TypeOBasic2d
{
    namespace Entities
    {
        public class Anchor2d : IAnchor2d
        {
            public Entity2d Parent { get; private set; }
            public Vec2 Position { get; set; }
            public Orientation2d Orientation { get; set; }
            public OrientationType OrientationType { get; set; }

            internal Anchor2d(Vec2 position, Orientation2d orientation, OrientationType orientationType, Entity2d parent)
            {
                Position = position;
                Orientation = orientation;
                OrientationType = orientationType;
                Parent = parent;
            }

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
