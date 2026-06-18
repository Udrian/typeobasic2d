using TypeD.Models.Data;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;

namespace TypeDBasic2d.Code.Drawable
{
    public partial class DrawableTextureCode : Drawable2dCode
    {
        public override bool IsBaseComponentType
        {
            get { return base.IsBaseComponentType || BaseClass == typeof(DrawableTexture).FullName; }
        }

        public DrawableTextureCode(Component component) : base(component) { }

        protected override void InitClass()
        {
            base.InitClass();
        }
    }
}
