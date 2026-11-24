using OpenTK.Mathematics;
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
        public class Entity2d : Entity, IAnchor2d
        {
            private Vec2 _position;
            private Vec2 _scale;
            private double _rotation;
            private Vec2 _size;
            private Vec2 _origin;
            private Matrix4 _modelMatrix = Matrix4.Identity;
            private Entity2d _cachedParent;
            private Vec2 _cachedPosition;
            private Vec2 _cachedScale;
            private double _cachedRotation;
            private Vec2 _cachedSize;
            private Vec2 _cachedOrigin;
            private int _modelMatrixVersion;
            private int _cachedParentModelMatrixVersion = -1;
            private bool _hasModelMatrix;

            /// <summary>
            /// Gets or sets the position represented by a two-dimensional vector.
            /// </summary>
            [TypeOProperty("Gets or sets the position represented by a two-dimensional vector.", Group = "Transform")]
            public virtual Vec2 Position
            {
                get => _position;
                set
                {
                    if (_position == value) return;
                    _position = value;
                    InvalidateModelMatrix();
                }
            }
            /// <summary>
            /// Gets or sets the scaling factor applied to the object along the X and Y axes.
            /// </summary>
            [TypeOProperty("Gets or sets the scaling factor applied to the object along the X and Y axes.", Group = "Transform")]
            public virtual Vec2 Scale
            {
                get => _scale;
                set
                {
                    if (_scale == value) return;
                    _scale = value;
                    InvalidateModelMatrix();
                }
            }
            /// <summary>
            /// Gets or sets the rotation angle, in degrees, applied to the object.
            /// </summary>
            [TypeOProperty("Gets or sets the rotation angle, in degrees, applied to the object.", Group = "Transform")]
            public virtual double Rotation
            {
                get => _rotation;
                set
                {
                    if (_rotation.Equals(value)) return;
                    _rotation = value;
                    InvalidateModelMatrix();
                }
            }
            /// <summary>
            /// Gets or sets the two-dimensional size of the object.
            /// </summary>
            [TypeOProperty("Gets or sets the two-dimensional size of the object.", Group = "Transform")]
            public virtual Vec2 Size
            {
                get => _size;
                set
                {
                    if (_size == value) return;
                    _size = value;
                    InvalidateModelMatrix();
                }
            }
            /// <summary>
            /// Gets or sets the origin point of the object in 2D space.
            /// </summary>
            [TypeOProperty("Gets or sets the origin point of the object in 2D space.", Group = "Transform")]
            public virtual Vec2 Origin
            {
                get => _origin;
                set
                {
                    if (_origin == value) return;
                    _origin = value;
                    InvalidateModelMatrix();
                }
            }

            /// <summary>
            /// Gets the cached model matrix that maps this entity's local coordinates into world coordinates.
            /// </summary>
            public Matrix4 ModelMatrix
            {
                get
                {
                    var parent = Parent as Entity2d;
                    var parentModelMatrix = parent?.ModelMatrix ?? Matrix4.Identity;
                    var parentVersion = parent?._modelMatrixVersion ?? 0;
                    var position = Position;
                    var scale = Scale;
                    var rotation = Rotation;
                    var size = Size;
                    var origin = Origin;

                    if (_hasModelMatrix
                        && ReferenceEquals(_cachedParent, parent)
                        && _cachedParentModelMatrixVersion == parentVersion
                        && _cachedPosition == position
                        && _cachedScale == scale
                        && _cachedRotation.Equals(rotation)
                        && _cachedSize == size
                        && _cachedOrigin == origin)
                    {
                        return _modelMatrix;
                    }

                    var pivot = size * origin;
                    var localMatrix =
                        Matrix4.CreateTranslation((float)-pivot.X, (float)-pivot.Y, 0) *
                        Matrix4.CreateScale((float)scale.X, (float)scale.Y, 1) *
                        Matrix4.CreateRotationZ(MathHelper.DegreesToRadians((float)rotation)) *
                        Matrix4.CreateTranslation((float)position.X, (float)position.Y, 0);

                    _modelMatrix = localMatrix * parentModelMatrix;
                    _cachedParent = parent;
                    _cachedParentModelMatrixVersion = parentVersion;
                    _cachedPosition = position;
                    _cachedScale = scale;
                    _cachedRotation = rotation;
                    _cachedSize = size;
                    _cachedOrigin = origin;
                    _hasModelMatrix = true;
                    _modelMatrixVersion++;
                    return _modelMatrix;
                }
            }

            /// <summary>
            /// Gets the world-space position of this entity's origin pivot.
            /// </summary>
            public Vec2 WorldPosition => TransformPoint(Size * Origin, ModelMatrix);

            /// <summary>
            /// Gets the entity's transformed world-space corners in clockwise order.
            /// </summary>
            public (Vec2 TopLeft, Vec2 TopRight, Vec2 BottomRight, Vec2 BottomLeft) WorldCorners
            {
                get
                {
                    var size = Size;
                    var matrix = ModelMatrix;
                    return (
                        TransformPoint(Vec2.Zero, matrix),
                        TransformPoint(new Vec2(size.X, 0), matrix),
                        TransformPoint(size, matrix),
                        TransformPoint(new Vec2(0, size.Y), matrix));
                }
            }

            /// <summary>
            /// Converts a world-space displacement into this entity's local coordinate space.
            /// </summary>
            public Vec2 WorldDeltaToLocal(Vec2 worldDelta)
            {
                var matrix = ModelMatrix;
                return InverseTransformPoint(worldDelta, matrix) - InverseTransformPoint(Vec2.Zero, matrix);
            }

            private void InvalidateModelMatrix()
            {
                _hasModelMatrix = false;
            }

            internal static Vec2 TransformPoint(Vec2 point, Matrix4 matrix)
            {
                return new Vec2(
                    point.X * matrix.M11 + point.Y * matrix.M21 + matrix.M41,
                    point.X * matrix.M12 + point.Y * matrix.M22 + matrix.M42);
            }

            internal static Vec2 InverseTransformPoint(Vec2 point, Matrix4 matrix)
            {
                var determinant = matrix.M11 * matrix.M22 - matrix.M12 * matrix.M21;
                if (Math.Abs(determinant) < 1e-8f)
                    throw new InvalidOperationException("The entity model matrix cannot be inverted.");

                var x = point.X - matrix.M41;
                var y = point.Y - matrix.M42;
                return new Vec2(
                    (x * matrix.M22 - y * matrix.M21) / determinant,
                    (y * matrix.M11 - x * matrix.M12) / determinant);
            }

            /// <summary>
            /// Initializes a new instance of the Entity2d class with default position, scale, rotation, size, and origin values.
            /// </summary>
            public Entity2d() : base()
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
            /// <remarks>The bounds include this entity's and its ancestors' scale, rotation, and origin transforms.
            /// Setting this property updates the entity's local position and size.</remarks>
            public Rectangle ScreenBounds {
                get {
                    var size = Size;
                    var matrix = ModelMatrix;
                    var topLeft = TransformPoint(Vec2.Zero, matrix);
                    var topRight = TransformPoint(new Vec2(size.X, 0), matrix);
                    var bottomRight = TransformPoint(size, matrix);
                    var bottomLeft = TransformPoint(new Vec2(0, size.Y), matrix);
                    var minX = Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomRight.X, bottomLeft.X));
                    var minY = Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomRight.Y, bottomLeft.Y));
                    var maxX = Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomRight.X, bottomLeft.X));
                    var maxY = Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomRight.Y, bottomLeft.Y));
                    return new Rectangle(new Vec2(minX, minY), new Vec2(maxX - minX, maxY - minY));
                }
                set {
                    Size = value.Size;
                    var parent = Parent as Entity2d;
                    var localPosition = parent == null
                        ? value.Pos
                        : InverseTransformPoint(value.Pos, parent.ModelMatrix);
                    Position = localPosition + Size * Origin;
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