using TypeD.Components;
using TypeD.Helpers;
using TypeDBasic2d.Code.Drawable;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeDBasic2d.Components
{
    /// <summary>
    /// Represents a template for creating 2D drawable components.
    /// </summary>
    /// <remarks>This class provides initialization logic and filtering capabilities for components that are
    /// associated with 2D drawable objects. It is intended to be used as a base for defining specific 2D drawable
    /// component templates.</remarks>
    public class Drawable2dComponentTemplate : ComponentTemplate<Drawable2dCode>
    {
        /// <summary>
        /// Applies a filter to include only child elements of type <see cref="Drawable"/>.
        /// </summary>
        /// <remarks>This method appends the fully qualified name of the <see cref="Drawable"/> type to
        /// the filter criteria.</remarks>
        /// <param name="filter">The <see cref="FilterHelper"/> instance to which the filter is applied.</param>
        public override void ChildrenFilter(FilterHelper filter)
        {
            filter.Filters += $"{typeof(Drawable).FullName};";
        }
    }
}
