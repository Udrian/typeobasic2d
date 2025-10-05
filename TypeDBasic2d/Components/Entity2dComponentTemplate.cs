using TypeD.Components;
using TypeD.Helpers;
using TypeDBasic2d.Code.Entity;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeDBasic2d.Components
{
    /// <summary>
    /// Represents a template for 2D entity components, providing functionality to filter child components.
    /// </summary>
    /// <remarks>This class is designed to work with 2D entity components and applies filtering logic to
    /// include drawable components. It extends the <see cref="ComponentTemplate{T}"/> base class with a specific
    /// implementation for <see cref="Entity2dCode"/>.</remarks>
    public class Entity2dComponentTemplate : ComponentTemplate<Entity2dCode>
    {
        /// <summary>
        /// Appends a filter for child elements of type <see cref="Drawable"/> to the specified filter helper.
        /// </summary>
        /// <remarks>This method modifies the <paramref name="filter"/> by appending a filter string for
        /// the <see cref="Drawable"/> type. Ensure that the <paramref name="filter"/> instance is properly initialized
        /// before calling this method.</remarks>
        /// <param name="filter">The <see cref="FilterHelper"/> instance to which the filter is applied. This parameter cannot be <see langword="null"/>.</param>
        public override void ChildrenFilter(FilterHelper filter)
        {
            filter.Filters += $"{typeof(Drawable).FullName};";
        }
    }
}
