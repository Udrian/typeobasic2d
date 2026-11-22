using TypeD.Models.Data;
using TypeDCore.Components;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;

namespace TypeDBasic2d.Components
{
    public static partial class Basic2dComponent
    {
        /// <summary>
        /// Creates the pre built engine component for DrawableFont
        /// </summary>
        /// <returns>A <see cref="Component"/> instance configured for 2D drawable texts using a font.</returns>
        public static Component DrawableFontComponent()
        {
            return new Component()
            {
                ClassName = typeof(DrawableFont).Name,
                Namespace = typeof(DrawableFont).Namespace,
                Template = new DrawableFontComponentTemplate(),
                BaseInheritedComponent = CoreComponent.DrawableComponent(),
                Icon = "drawablefont.png"
            };
        }
    }
}
