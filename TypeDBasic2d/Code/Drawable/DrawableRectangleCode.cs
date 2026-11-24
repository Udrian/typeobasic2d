using TypeD.Models.Data;
using TypeDCore.Code.Drawable;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;

namespace TypeDBasic2d.Code.Drawable
{

    public partial class DrawableRectangleCode : DrawableCode
    {
        public override bool IsBaseComponentType
        {
            get { return base.IsBaseComponentType || BaseClass == typeof(DrawableRectangleCode).FullName; }
        }

        public DrawableRectangleCode(Component component) : base(component)
        {
        }

        protected override void InitClass()
        {
            base.InitClass();
        }
    }
}
