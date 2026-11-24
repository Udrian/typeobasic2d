using OpenTK.Mathematics;
using TypeOEngine.Typedeaf.Core.Attributes;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities;

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
            [TypeOProperty("Gets or sets the position of the object in 2D space.", Group = "Transform")]
            public Vec2 Position { get; set; }

            /// <summary>
            /// Gets or sets the origin point that the Position refers to. (0,0) is top-left, (0.5,0.5) is center.
            /// </summary>
            [TypeOProperty("Gets or sets the origin point that the Position refers to.", Group = "Transform")]
            public Vec2 Origin { get; set; }

            /// <summary>
            /// Gets or sets the size of the object as a two-dimensional vector.
            /// </summary>
            [TypeOProperty("Gets or sets the size of the object as a two-dimensional vector.", Group = "Transform")]
            public virtual Vec2 Size { get; set; }

            /// <summary>
            /// Gets or sets the scale of the object as a two-dimensional vector.
            /// </summary>
            [TypeOProperty("Gets or sets the scale of the object as a two-dimensional vector.", null, Group = "Transform")]
            public Vec2 Scale { get; set; }

            /// <summary>
            /// Gets or sets the rotation angle, in degrees.
            /// </summary>
            [TypeOProperty("Gets or sets the rotation angle of the object in degrees.", 0.0, Group = "Transform")]
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
                    var originOffset = scaledSize * Origin;
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

                    var minX = double.MaxValue;
                    var maxX = double.MinValue;
                    var minY = double.MaxValue;
                    var maxY = double.MinValue;

                    for (int i = 0; i < 4; i++)
                    {
                        var relativeCorner = corners[i] - originOffset;
                        var x = relativeCorner.X * cosRot - relativeCorner.Y * sinRot;
                        var y = relativeCorner.X * sinRot + relativeCorner.Y * cosRot;

                        minX = Math.Min(minX, x);
                        maxX = Math.Max(maxX, x);
                        minY = Math.Min(minY, y);
                        maxY = Math.Max(maxY, y);
                    }

                    var boundingSize = new Vec2(maxX - minX, maxY - minY);
                    var adjustedPos = Position + new Vec2(minX, minY);
                    return new Rectangle(adjustedPos, boundingSize);
                }
            }

            /// <summary>
            /// Gets the axis-aligned world-space bounds of the drawable.
            /// </summary>
            public Rectangle WorldBounds
            {
                get
                {
                    var (topLeft, topRight, bottomRight, bottomLeft) = WorldCorners;
                    var minX = Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomRight.X, bottomLeft.X));
                    var minY = Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomRight.Y, bottomLeft.Y));
                    var maxX = Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomRight.X, bottomLeft.X));
                    var maxY = Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomRight.Y, bottomLeft.Y));
                    return new Rectangle(new Vec2(minX, minY), new Vec2(maxX - minX, maxY - minY));
                }
            }

            /// <summary>
            /// Gets the drawable's transformed world-space corners in clockwise order.
            /// </summary>
            public (Vec2 TopLeft, Vec2 TopRight, Vec2 BottomRight, Vec2 BottomLeft) WorldCorners
            {
                get
                {
                    var scaledSize = Size * Scale;
                    var modelMatrix = GetWorldModelMatrix();
                    return (
                        Entity2d.TransformPoint(Vec2.Zero, modelMatrix),
                        Entity2d.TransformPoint(new Vec2(scaledSize.X, 0), modelMatrix),
                        Entity2d.TransformPoint(scaledSize, modelMatrix),
                        Entity2d.TransformPoint(new Vec2(0, scaledSize.Y), modelMatrix));
                }
            }

            internal (Vec2 TopLeft, Vec2 TopRight, Vec2 BottomRight, Vec2 BottomLeft) GetLocalCorners()
            {
                var scaledSize = Size * Scale;
                var originOffset = scaledSize * Origin;
                var modelMatrix =
                    Matrix4.CreateTranslation((float)-originOffset.X, (float)-originOffset.Y, 0) *
                    Matrix4.CreateRotationZ(MathHelper.DegreesToRadians((float)Rotation)) *
                    Matrix4.CreateTranslation((float)Position.X, (float)Position.Y, 0);
                return (
                    Entity2d.TransformPoint(Vec2.Zero, modelMatrix),
                    Entity2d.TransformPoint(new Vec2(scaledSize.X, 0), modelMatrix),
                    Entity2d.TransformPoint(scaledSize, modelMatrix),
                    Entity2d.TransformPoint(new Vec2(0, scaledSize.Y), modelMatrix));
            }

            /// <summary>
            /// Gets the world-space position of the drawable's origin pivot.
            /// </summary>
            public Vec2 WorldPosition => Parent is Entity2d parent
                ? Entity2d.TransformPoint(Position, parent.ChildrenModelMatrix)
                : Position;

            /// <summary>
            /// Converts a world-space displacement into the drawable's parent coordinate space.
            /// </summary>
            public Vec2 WorldDeltaToLocal(Vec2 worldDelta)
            {
                if (Parent is not Entity2d parent)
                    return worldDelta;

                var matrix = parent.ChildrenModelMatrix;
                return Entity2d.InverseTransformPoint(worldDelta, matrix)
                    - Entity2d.InverseTransformPoint(Vec2.Zero, matrix);
            }

            /// <summary>
            /// Converts a world-space point to a normalized origin within this drawable's bounds.
            /// </summary>
            public Vec2 GetOriginAtWorldPosition(Vec2 worldPosition)
            {
                var localPosition = Entity2d.InverseTransformPoint(worldPosition, GetWorldModelMatrix());
                var scaledSize = Size * Scale;
                return new Vec2(
                    scaledSize.X == 0 ? 0 : localPosition.X / scaledSize.X,
                    scaledSize.Y == 0 ? 0 : localPosition.Y / scaledSize.Y);
            }

            private Matrix4 GetWorldModelMatrix()
            {
                var originOffset = Size * Scale * Origin;
                var modelMatrix =
                    Matrix4.CreateTranslation((float)-originOffset.X, (float)-originOffset.Y, 0) *
                    Matrix4.CreateRotationZ(MathHelper.DegreesToRadians((float)Rotation)) *
                    Matrix4.CreateTranslation((float)Position.X, (float)Position.Y, 0);
                if (Parent is Entity2d parent)
                    modelMatrix *= parent.ChildrenModelMatrix;
                return modelMatrix;
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
