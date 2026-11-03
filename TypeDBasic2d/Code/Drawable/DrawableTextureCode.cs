using TypeD.Models.Data;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;

namespace TypeDBasic2d.Code.Drawable
{
    /// <summary>
    /// Code generator for components deriving from <see cref="DrawableTexture"/>.
    /// </summary>
    public partial class DrawableTextureCode : Drawable2dCode
    {
        /// <summary>
        /// Gets a value indicating whether the base class is a base component type or <see cref="DrawableTexture"/> itself.
        /// </summary>
        public override bool IsBaseComponentType
        {
            get { return base.IsBaseComponentType || BaseClass == typeof(DrawableTexture).FullName; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawableTextureCode"/> class.
        /// </summary>
        /// <param name="component">The component to generate code for.</param>
        public DrawableTextureCode(Component component) : base(component) { }

        /// <summary>
        /// Initializes the generated class.
        /// </summary>
        protected override void InitClass()
        {
            base.InitClass();
        }
    }
}
