using TypeD;
using TypeD.Models.Data;
using TypeD.Models.Providers.Interfaces;
using TypeDBasic2d.Components;

namespace TypeDBasic2d
{
    internal class TypeDBasic2dInitializer : TypeDModuleInitializer
    {
        // Providers
        IComponentProvider ComponentProvider { get; set; }

        public override void Initializer(Project project)
        {
            // Providers
            ComponentProvider = Resources.Get<IComponentProvider>();

            // Data
            ComponentProvider.AddBaseTypeComponent(Basic2dComponent.Drawable2dComponent());
            ComponentProvider.AddBaseTypeComponent(Basic2dComponent.Entity2dComponent());
        }

        public override void Uninitializer()
        {
            // Data
            ComponentProvider.RemoveBaseTypeComponent(Basic2dComponent.Drawable2dComponent());
            ComponentProvider.RemoveBaseTypeComponent(Basic2dComponent.Entity2dComponent());
        }
    }
}