using TypeD.Models.Data;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;

namespace TypeDBasic2d.Code.Drawable
{
    /// <summary>
    /// Code generator for components deriving from <see cref="DrawableFont"/>.
    /// </summary>
    public partial class DrawableFontCode : Drawable2dCode
    {
        /// <summary>
        /// Gets a value indicating whether the base class is a base component type or <see cref="DrawableFont"/> itself.
        /// </summary>
        public override bool IsBaseComponentType
        {
            get { return base.IsBaseComponentType || BaseClass == typeof(DrawableFont).FullName; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawableFontCode"/> class.
        /// </summary>
        /// <param name="component">The component to generate code for.</param>
        public DrawableFontCode(Component component) : base(component) { }

        /// <summary>
        /// Initializes the generated class.
        /// </summary>
        protected override void InitClass()
        {
            base.InitClass();
        }
    }
}
