using TypeOEngine.Typedeaf.Core.Attributes;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeOEngine.Typedeaf.TypeOBasic2d
{
    namespace Entities.Drawables
    {
        /// <summary>
        /// Represents a two-dimensional drawable object with position, size, scale, rotation, and origin properties.
        /// </summary>
        /// <remarks>This is a base class for drawable objects in a 2D space. Derived classes can override the Size property
        /// to define custom dimensions. The class provides a concrete BoundingBox implementation that accounts for position,
        /// origin, scale, and rotation.</remarks>
        public abstract class Drawable2d : Drawable
        {
            /// <summary>
            /// Gets or sets the position of the object in 2D space.
            /// </summary>
            [TypeOProperty("Gets or sets the position of the object in 2D space.")]
            public Vec2 Position { get; set; }

            /// <summary>
            /// Gets or sets the origin point that the Position refers to. (0,0) is top-left, (0.5,0.5) is center.
            /// </summary>
            [TypeOProperty("Gets or sets the origin point that the Position refers to.")]
            public Vec2 Origin { get; set; }

            /// <summary>
            /// Gets or sets the size of the object as a two-dimensional vector.
            /// </summary>
            [TypeOProperty("Gets or sets the size of the object as a two-dimensional vector.")]
            public virtual Vec2 Size { get; set; }

            /// <summary>
            /// Gets or sets the scale of the object as a two-dimensional vector.
            /// </summary>
            [TypeOProperty("Gets or sets the scale of the object as a two-dimensional vector.", null)]
            public Vec2 Scale { get; set; }

            /// <summary>
            /// Gets or sets the rotation angle, in degrees.
            /// </summary>
            [TypeOProperty("Gets or sets the rotation angle of the object in degrees.", 0.0)]
            public double Rotation { get; set; }

            /// <summary>
            /// Gets the rectangular bounding box of the drawable, accounting for position, origin, scale, and rotation.
            /// </summary>
            public Rectangle BoundingBox
            {
                get
                {
                    var size = Size;
                    var scaledSize = size * Scale;
                    var originOffset = size * Origin;
                    var boundingPos = Position - originOffset;

                    if (Rotation == 0)
                    {
                        return new Rectangle(boundingPos, scaledSize);
                    }

                    var rotationRadians = Rotation * (Math.PI / 180.0);
                    var cosRot = Math.Cos(rotationRadians);
                    var sinRot = Math.Sin(rotationRadians);

                    var corners = new[]
                    {
                        new Vec2(0, 0),
                        new Vec2(scaledSize.X, 0),
                        new Vec2(scaledSize.X, scaledSize.Y),
                        new Vec2(0, scaledSize.Y)
                    };

                    var rotatedCorners = new Vec2[4];
                    var minX = double.MaxValue;
                    var maxX = double.MinValue;
                    var minY = double.MaxValue;
                    var maxY = double.MinValue;

                    for (int i = 0; i < 4; i++)
                    {
                        var x = corners[i].X * cosRot - corners[i].Y * sinRot;
                        var y = corners[i].X * sinRot + corners[i].Y * cosRot;
                        rotatedCorners[i] = new Vec2(x, y);

                        minX = Math.Min(minX, x);
                        maxX = Math.Max(maxX, x);
                        minY = Math.Min(minY, y);
                        maxY = Math.Max(maxY, y);
                    }

                    var boundingSize = new Vec2(maxX - minX, maxY - minY);
                    var adjustedPos = boundingPos + new Vec2(minX, minY);
                    return new Rectangle(adjustedPos, boundingSize);
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Drawable2d"/> class.
            /// </summary>
            /// <remarks>This constructor sets the initial position of the drawable object to <see cref="Vec2.Zero"/>,
            /// origin to <see cref="Vec2.Zero"/> (top-left), scale to <see cref="Vec2.One"/>, and rotation to 0 degrees.</remarks>
            protected Drawable2d() : base()
            {
                Position = Vec2.Zero;
                Origin = Vec2.Zero;
                Scale = Vec2.One;
                Rotation = 0;
            }
        }
    }
}
