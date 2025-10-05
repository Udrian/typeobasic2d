using TypeD.Models.Data;
using TypeDCore.Code.Entity;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities;

namespace TypeDBasic2d.Code.Entity
{
    public partial class Entity2dCode : EntityCode
    {
        public override bool IsBaseComponentType
        {
            get { return base.IsBaseComponentType || BaseClass == typeof(Entity2d).FullName; }
        }

        public Entity2dCode(Component component) : base(component)
        {
        }

        protected override void InitClass()
        {
            base.InitClass();
        }
    }
}