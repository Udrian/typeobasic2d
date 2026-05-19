using TypeDCore.Code.Drawable;

namespace TypeDBasic2d.Code.Drawable
{
    public partial class Drawable2dCode : DrawableCode
    {
        protected override void InitTypeDClass()
        {
            if (IsBaseComponentType)
            {
                AddUsing("TypeOEngine.Typedeaf.Core.Common");
                AddProperty(new Property("public override Vec2 Size", () => {
                    Writer.AddLine("get; set;");
                }));
            }
            base.InitTypeDClass();
        }
    }
}
