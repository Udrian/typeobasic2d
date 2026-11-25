using TypeD.Models.Data;
using TypeDCore.Code.Drawable;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;

namespace TypeDBasic2d.Code.Drawable
{
    public partial class DrawablePolygonCode : DrawableCode
    {
        public override bool IsBaseComponentType
        {
            get { return base.IsBaseComponentType || BaseClass == typeof(DrawablePolygonCode).FullName; }
        }

        public DrawablePolygonCode(Component component) : base(component)
        {
        }

        protected override void InitClass()
        {
            base.InitClass();
        }
    }
}
