using TypeDCore.Code.Drawable;

namespace TypeDBasic2d.Code.Drawable
{
    public partial class Drawable2dCode : DrawableCode
    {
        /// <summary>
        /// Adds the 2D drawable specific usings and the <c>Size</c> property override to the generated TypeD class.
        /// </summary>
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
