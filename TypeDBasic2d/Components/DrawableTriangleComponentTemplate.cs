using TypeD.Components;
using TypeD.Helpers;
using TypeDBasic2d.Code.Drawable;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeDBasic2d.Components
{
    /// <summary>
    /// Represents a template for creating 2D drawable triangle components.
    /// </summary>
    public class DrawableTriangleComponentTemplate : ComponentTemplate<DrawableTriangleCode>
    {
        /// <summary>
        /// Applies a filter to include only child elements of type <see cref="Drawable"/>.
        /// </summary>
        /// <param name="filter">The filter to apply.</param>
        public override void ChildrenFilter(FilterHelper filter)
        {
            filter.Filters += $"{typeof(Drawable).FullName};";
        }
    }
}
