using TypeD.Models.Data;
using TypeDCore.Code.Drawable;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;

namespace TypeDBasic2d.Code.Drawable
{
    /// <summary>
    /// Represents a specialized implementation of <see cref="DrawableCode"/> for 2D drawable components.
    /// </summary>
    /// <remarks>This class extends the functionality of <see cref="DrawableCode"/> to support 2D-specific
    /// drawable components. It determines whether the associated component is a base component type for 2D drawables
    /// and initializes properties and dependencies accordingly.</remarks>
    public partial class Drawable2dCode : DrawableCode
    {
        /// <summary>
        /// Gets a value indicating whether the current type is considered a base component type.
        /// </summary>
        /// <remarks>A type is considered a base component type if it is either inherently marked as such 
        /// or if its base class is <see cref="Drawable2d"/>.</remarks>
        public override bool IsBaseComponentType
        {
            get { return base.IsBaseComponentType || BaseClass == typeof(Drawable2d).FullName; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Drawable2dCode"/> class with the specified component.
        /// </summary>
        /// <param name="component">The component associated with this drawable object.</param>
        public Drawable2dCode(Component component) : base(component)
        {
        }

        /// <summary>
        /// Initializes the class by configuring its properties and dependencies.
        /// </summary>
        /// <remarks>This method sets up the class by adding necessary using directives and defining
        /// properties if the class is a base component type. It also invokes the base class implementation to ensure 
        /// proper initialization.</remarks>
        protected override void InitClass()
        {
            base.InitClass();
        }
    }
}
