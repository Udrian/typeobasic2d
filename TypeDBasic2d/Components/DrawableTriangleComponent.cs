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
        /// Creates and returns a preconfigured component for 2D drawable triangles.
        /// </summary>
        /// <returns>A component instance configured for 2D drawable triangles.</returns>
        public static Component DrawableTriangleComponent()
        {
            return new Component()
            {
                ClassName = typeof(DrawableTriangle).Name,
                Namespace = typeof(DrawableTriangle).Namespace,
                Template = new DrawableTriangleComponentTemplate(),
                BaseInheritedComponent = CoreComponent.DrawableComponent(),
                Icon = "drawable2d.png"
            };
        }
    }
}
