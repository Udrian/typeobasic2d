using TypeD.Models.Data;
using TypeDCore.Components;
using TypeOBasic2d.Typedeaf.Basic2d.Entities.Drawables.Primitives;

namespace TypeDBasic2d.Components
{
    /// <summary>
    /// Factory methods for the TypeOBasic2d component definitions.
    /// </summary>
    public static partial class Basic2dComponent
    {
        /// <summary>
        /// Creates and returns a preconfigured <see cref="Component"/> instance for 2D drawable rectangles.
        /// </summary>
        /// <returns>A <see cref="Component"/> instance configured for 2D drawable rectangles.</returns>
        public static Component DrawableRectangleComponent()
        {
            return new Component()
            {
                ClassName = typeof(DrawableRectangle).Name,
                Namespace = typeof(DrawableRectangle).Namespace,
                Template = new DrawableRectangleComponentTemplate(),
                BaseInheritedComponent = CoreComponent.DrawableComponent(),
                Icon = "drawable2d.png"
            };
        }
    }
}
