using TypeD.Models.Data;
using TypeDCore.Components;
using TypeOEngine.Typedeaf.Core.Entities;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities;

namespace TypeDBasic2d.Components
{
    public static partial class Basic2dComponent
    {
        /// <summary>
        /// Creates and returns a new 2D entity component.
        /// </summary>
        /// <remarks>The returned component is configured with the class name and namespace of the
        /// <see cref="Entity2d"/> type, a template of type <see cref="Entity2dComponentTemplate"/>, and a parent component
        /// derived from <see cref="CoreComponent.EntityComponent()"/>. The base type is set to <see cref="Entity"/>.</remarks>
        /// <returns>A <see cref="Component"/> instance representing a 2D entity component.</returns>
        public static Component Entity2dComponent()
        {
            return new Component()
            {
                ClassName = typeof(Entity2d).Name,
                Namespace = typeof(Entity2d).Namespace,
                Template = new Entity2dComponentTemplate(),
                BaseInheritedComponent = CoreComponent.EntityComponent()
            };
        }
    }
}
