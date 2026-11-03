using TypeD.Models.Data;
using TypeDCore.Code.Entity;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities;

namespace TypeDBasic2d.Code.Entity
{
    /// <summary>
    /// Code generator for components deriving from <see cref="Entity2d"/>.
    /// </summary>
    public partial class Entity2dCode : EntityCode
    {
        /// <summary>
        /// Gets a value indicating whether the base class is a base component type or <see cref="Entity2d"/> itself.
        /// </summary>
        public override bool IsBaseComponentType
        {
            get { return base.IsBaseComponentType || BaseClass == typeof(Entity2d).FullName; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity2dCode"/> class.
        /// </summary>
        /// <param name="component">The component to generate code for.</param>
        public Entity2dCode(Component component) : base(component)
        {
        }

        /// <summary>
        /// Initializes the generated class.
        /// </summary>
        protected override void InitClass()
        {
            base.InitClass();
        }
    }
}