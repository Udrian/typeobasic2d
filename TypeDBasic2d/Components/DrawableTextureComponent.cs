using TypeD.Models.Data;
using TypeDCore.Components;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;

namespace TypeDBasic2d.Components
{
    public static partial class Basic2dComponent
    {
        /// <summary>
        /// Creates and returns a preconfigured <see cref="Component"/> instance for 2D drawable objects.
        /// </summary>
        /// <remarks>The returned <see cref="Component"/> is specifically tailored for 2D drawable
        /// entities, with its class name, namespace, and template set to match the <see cref="Drawable2d"/> type. It
        /// also establishes a parent-child relationship with the core drawable component.</remarks>
        /// <returns>A <see cref="Component"/> instance configured for 2D drawable objects.</returns>
        public static Component DrawableTextureComponent()
        {
            return new Component()
            {
                ClassName = typeof(DrawableTexture).Name,
                Namespace = typeof(DrawableTexture).Namespace,
                Template = new DrawableTextureComponentTemplate(),
                BaseInheritedComponent = CoreComponent.DrawableComponent()
            };
        }
    }
}
