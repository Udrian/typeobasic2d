using TypeD.Models.Data;
using TypeDCore.Components;
using TypeOBasic2d.Typedeaf.Basic2d.Entities.Drawables.Primitives;

namespace TypeDBasic2d.Components
{
    public static partial class Basic2dComponent
    {
        /// <summary>
        /// Creates and returns a preconfigured <see cref="Component"/> instance for 2D drawable polygons.
        /// </summary>
        /// <returns>A <see cref="Component"/> instance configured for 2D drawable polygons.</returns>
        public static Component DrawablePolygonComponent()
        {
            return new Component()
            {
                ClassName = typeof(DrawablePolygon).Name,
                Namespace = typeof(DrawablePolygon).Namespace,
                Template = new DrawablePolygonComponentTemplate(),
                BaseInheritedComponent = CoreComponent.DrawableComponent(),
                Icon = "drawable2d.png"
            };
        }
    }
}
