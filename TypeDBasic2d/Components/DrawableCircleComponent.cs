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
        /// Creates and returns a preconfigured component for 2D drawable circles.
        /// </summary>
        /// <returns>A component instance configured for 2D drawable circles.</returns>
        public static Component DrawableCircleComponent()
        {
            return new Component()
            {
                ClassName = typeof(DrawableCircle).Name,
                Namespace = typeof(DrawableCircle).Namespace,
                Template = new DrawableCircleComponentTemplate(),
                BaseInheritedComponent = CoreComponent.DrawableComponent(),
                Icon = "drawable2d.png"
            };
        }
    }
}
