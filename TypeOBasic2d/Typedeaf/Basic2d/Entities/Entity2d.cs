using OpenTK.Mathematics;
using TypeOEngine.Typedeaf.Core.Attributes;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Entities;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;
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
            private SizeMode _sizeMode = SizeMode.Auto;
            private Matrix4 _modelMatrix = Matrix4.Identity;
            private Matrix4 _childrenModelMatrix = Matrix4.Identity;
            private Entity2d _cachedParent;
            private Vec2 _cachedPosition;
            private Vec2 _cachedScale;
            private double _cachedRotation;
            private Vec2 _cachedSize;
            private Vec2 _cachedOrigin;
            private int _modelMatrixVersion;
            private int _childrenModelMatrixVersion;
            private int _cachedParentChildrenModelMatrixVersion = -1;
            private int _cachedModelMatrixVersion = -1;
            private Vec2 _cachedChildrenSize;
            private Vec2 _cachedChildrenBoundsPosition;
            private Vec2 _cachedChildrenBoundsSize;
            private SizeMode _cachedChildrenSizeMode;
            private bool _hasModelMatrix;
            private bool _hasChildrenModelMatrix;

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
            /// Gets or sets the two-dimensional size of the object. In Auto mode, the getter returns the bounds of its children.
            /// </summary>
            [TypeOProperty("Gets or sets the two-dimensional size of the object. In Auto mode, the getter returns the bounds of its children.", Group = "Transform")]
            public virtual Vec2 Size
            {
                get => SizeMode == SizeMode.Auto ? GetChildrenBounds().Size : _size;
                set
                {
                    if (_size == value) return;
                    _size = value;
                    InvalidateModelMatrix();
                }
            }
            /// <summary>
            /// Gets or sets how this entity sizes and constrains its children.
            /// </summary>
            [TypeOProperty("Gets or sets how this entity sizes and constrains its children.", SizeMode.Auto, Group = "Transform")]
            public SizeMode SizeMode
            {
                get => _sizeMode;
                set
                {
                    if (_sizeMode == value) return;
                    _sizeMode = value;
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
                    var parentModelMatrix = parent?.ChildrenModelMatrix ?? Matrix4.Identity;
                    var parentVersion = parent?._childrenModelMatrixVersion ?? 0;
                    var position = Position;
                    var scale = Scale;
                    var rotation = Rotation;
                    var size = Size;
                    var origin = Origin;
                    if (_hasModelMatrix
                        && ReferenceEquals(_cachedParent, parent)
                        && _cachedParentChildrenModelMatrixVersion == parentVersion
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
                    _cachedParentChildrenModelMatrixVersion = parentVersion;
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
            /// Gets the matrix that maps this entity's child coordinate space into world coordinates.
            /// </summary>
            public Matrix4 ChildrenModelMatrix
            {
                get
                {
                    var modelMatrix = ModelMatrix;
                    var contentBounds = SizeMode == SizeMode.Stretch
                        ? GetChildrenBounds()
                        : new Rectangle(Vec2.Zero, Vec2.Zero);
                    var size = Size;
                    if (_hasChildrenModelMatrix
                        && _cachedModelMatrixVersion == _modelMatrixVersion
                        && _cachedChildrenSize == size
                        && _cachedChildrenBoundsPosition == contentBounds.Pos
                        && _cachedChildrenBoundsSize == contentBounds.Size
                        && _cachedChildrenSizeMode == SizeMode)
                    {
                        return _childrenModelMatrix;
                    }

                    var contentMatrix = Matrix4.Identity;
                    if (SizeMode == SizeMode.Stretch && contentBounds.Size != Vec2.Zero)
                    {
                        var scale = new Vec2(
                            contentBounds.Size.X == 0 ? 1 : size.X / contentBounds.Size.X,
                            contentBounds.Size.Y == 0 ? 1 : size.Y / contentBounds.Size.Y);
                        contentMatrix =
                            Matrix4.CreateTranslation((float)-contentBounds.Pos.X, (float)-contentBounds.Pos.Y, 0) *
                            Matrix4.CreateScale((float)scale.X, (float)scale.Y, 1);
                    }

                    _childrenModelMatrix = contentMatrix * modelMatrix;
                    _cachedModelMatrixVersion = _modelMatrixVersion;
                    _cachedChildrenSize = size;
                    _cachedChildrenBoundsPosition = contentBounds.Pos;
                    _cachedChildrenBoundsSize = contentBounds.Size;
                    _cachedChildrenSizeMode = SizeMode;
                    _hasChildrenModelMatrix = true;
                    _childrenModelMatrixVersion++;
                    return _childrenModelMatrix;
                }
            }

            /// <summary>
            /// Gets the world-space position of this entity's origin pivot.
            /// </summary>
            public Vec2 WorldPosition
            {
                get
                {
                    return TransformPoint(Size * Origin, ModelMatrix);
                }
            }

            /// <summary>
            /// Gets the entity's transformed world-space corners in clockwise order.
            /// </summary>
            public (Vec2 TopLeft, Vec2 TopRight, Vec2 BottomRight, Vec2 BottomLeft) WorldCorners
            {
                get
                {
                    return GetBoundsCorners(ModelMatrix);
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
                _hasChildrenModelMatrix = false;
            }

            private Rectangle GetChildrenBounds()
            {
                var hasBounds = false;
                var minX = double.MaxValue;
                var minY = double.MaxValue;
                var maxX = double.MinValue;
                var maxY = double.MinValue;

                void Include((Vec2 TopLeft, Vec2 TopRight, Vec2 BottomRight, Vec2 BottomLeft) corners)
                {
                    minX = Math.Min(minX, Math.Min(Math.Min(corners.TopLeft.X, corners.TopRight.X), Math.Min(corners.BottomRight.X, corners.BottomLeft.X)));
                    minY = Math.Min(minY, Math.Min(Math.Min(corners.TopLeft.Y, corners.TopRight.Y), Math.Min(corners.BottomRight.Y, corners.BottomLeft.Y)));
                    maxX = Math.Max(maxX, Math.Max(Math.Max(corners.TopLeft.X, corners.TopRight.X), Math.Max(corners.BottomRight.X, corners.BottomLeft.X)));
                    maxY = Math.Max(maxY, Math.Max(Math.Max(corners.TopLeft.Y, corners.TopRight.Y), Math.Max(corners.BottomRight.Y, corners.BottomLeft.Y)));
                    hasBounds = true;
                }

                if (Entities != null)
                {
                    foreach (var child in Entities.ListAll().OfType<Entity2d>())
                        Include(child.GetLocalCorners());
                }

                if (Drawables != null)
                {
                    foreach (var drawable in Drawables.Get<Drawable2d>())
                        Include(drawable.GetLocalCorners());
                }

                return hasBounds
                    ? new Rectangle(new Vec2(minX, minY), new Vec2(maxX - minX, maxY - minY))
                    : new Rectangle(Vec2.Zero, Vec2.Zero);
            }

            internal (Vec2 TopLeft, Vec2 TopRight, Vec2 BottomRight, Vec2 BottomLeft) GetLocalCorners()
            {
                var size = Size;
                var bounds = SizeMode == SizeMode.Auto
                    ? GetChildrenBounds()
                    : new Rectangle(Vec2.Zero, size);
                var matrix = CreateLocalModelMatrix(
                    Position,
                    Scale,
                    Rotation,
                    size,
                    Origin);
                return (
                    TransformPoint(bounds.Pos, matrix),
                    TransformPoint(new Vec2(bounds.Pos.X + bounds.Size.X, bounds.Pos.Y), matrix),
                    TransformPoint(bounds.Pos + bounds.Size, matrix),
                    TransformPoint(new Vec2(bounds.Pos.X, bounds.Pos.Y + bounds.Size.Y), matrix));
            }

            private (Vec2 TopLeft, Vec2 TopRight, Vec2 BottomRight, Vec2 BottomLeft) GetBoundsCorners(Matrix4 matrix)
            {
                var size = Size;
                var bounds = SizeMode == SizeMode.Auto
                    ? GetChildrenBounds()
                    : new Rectangle(Vec2.Zero, size);
                return (
                    TransformPoint(bounds.Pos, matrix),
                    TransformPoint(new Vec2(bounds.Pos.X + bounds.Size.X, bounds.Pos.Y), matrix),
                    TransformPoint(bounds.Pos + bounds.Size, matrix),
                    TransformPoint(new Vec2(bounds.Pos.X, bounds.Pos.Y + bounds.Size.Y), matrix));
            }

            internal static Matrix4 CreateLocalModelMatrix(Vec2 position, Vec2 scale, double rotation, Vec2 size, Vec2 origin)
            {
                var pivot = size * origin;
                return
                    Matrix4.CreateTranslation((float)-pivot.X, (float)-pivot.Y, 0) *
                    Matrix4.CreateScale((float)scale.X, (float)scale.Y, 1) *
                    Matrix4.CreateRotationZ(MathHelper.DegreesToRadians((float)rotation)) *
                    Matrix4.CreateTranslation((float)position.X, (float)position.Y, 0);
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
                    var (topLeft, topRight, bottomRight, bottomLeft) = GetBoundsCorners(ModelMatrix);
                    var minX = Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomRight.X, bottomLeft.X));
                    var minY = Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomRight.Y, bottomLeft.Y));
                    var maxX = Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomRight.X, bottomLeft.X));
                    var maxY = Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomRight.Y, bottomLeft.Y));
                    return new Rectangle(new Vec2(minX, minY), new Vec2(maxX - minX, maxY - minY));
                }
                set {
                    if (SizeMode == SizeMode.Auto)
                        SizeMode = SizeMode.None;
                    Size = value.Size;
                    var parent = Parent as Entity2d;
                    var localPosition = parent == null
                        ? value.Pos
                        : InverseTransformPoint(value.Pos, parent.ChildrenModelMatrix);
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