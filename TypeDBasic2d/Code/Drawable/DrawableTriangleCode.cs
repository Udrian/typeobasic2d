using TypeD.Models.Data;
using TypeDCore.Code.Drawable;
using TypeOBasic2d.Typedeaf.Basic2d.Entities.Drawables.Primitives;

namespace TypeDBasic2d.Code.Drawable
{
    /// <summary>
    /// TypeD code representation for 2D drawable triangle components.
    /// </summary>
    public partial class DrawableTriangleCode : DrawableCode
    {
        /// <inheritdoc/>
        public override bool IsBaseComponentType
        {
            get { return base.IsBaseComponentType || BaseClass == typeof(DrawableTriangle).FullName; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawableTriangleCode"/> class.
        /// </summary>
        /// <param name="component">The component associated with this drawable.</param>
        public DrawableTriangleCode(Component component) : base(component)
        {
        }

        /// <inheritdoc/>
        protected override void InitClass()
        {
            base.InitClass();
        }
    }
}
