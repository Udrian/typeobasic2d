using TypeD.Components;
using TypeD.Helpers;
using TypeDBasic2d.Code.Drawable;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeDBasic2d.Components
{
    /// <summary>
    /// Template for polygon drawable components, which can have drawables as children.
    /// </summary>
    public class DrawablePolygonComponentTemplate : ComponentTemplate<DrawablePolygonCode>
    {
        /// <inheritdoc/>
        public override void ChildrenFilter(FilterHelper filter)
        {
            filter.Filters += $"{typeof(Drawable).FullName};";
        }
    }
}
