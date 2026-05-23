using System.Collections.Generic;
using System.Linq;
using TypeD;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;
using TypeDBasic2d.Components;
using TypeDBasic2d.Typedeaf.Core.Entities.Drawables;
using TypeDCore.Models.Data.Hooks;
using TypeOEngine.Typedeaf.Core.Common;

namespace TypeDBasic2d
{
    internal class TypeDBasic2dInitializer : TypeDModuleInitializer
    {
        // Providers
        IComponentProvider ComponentProvider { get; set; }

        // Models
        IHookModel HookModel { get; set; }

        // Data
        Dictionary<string, Debug2DDrawable> DebugDrawablesMap { get; set; }
        Debug2DDrawable CurrentFocusedDebug2DDrawable { get; set; }

        public override void Initializer(Project project)
        {
            // Providers
            ComponentProvider = Resources.Get<IComponentProvider>();

            // Models
            HookModel = Resources.Get<IHookModel>();

            // Data
            DebugDrawablesMap = new Dictionary<string, Debug2DDrawable>();

            ComponentProvider.AddBaseTypeComponent(Basic2dComponent.Entity2dComponent());
            ComponentProvider.AddBaseTypeComponent(Basic2dComponent.Drawable2dComponent());
            //ComponentProvider.AddBaseTypeComponent(Basic2dComponent.)

            // Hooks
            HookModel.AddHook<TypeOObjectAddedToViewHook>(AddDebugDrawableComponent);
            HookModel.AddHook<PropertyChangedHook>(ComponentPropertyChanged);
            HookModel.AddHook<ComponentFocusHook>(ComponentFocusChanged);
        }

        public override void Uninitializer()
        {
            // Data
            ComponentProvider.RemoveBaseTypeComponent(Basic2dComponent.Entity2dComponent());
            ComponentProvider.RemoveBaseTypeComponent(Basic2dComponent.Drawable2dComponent());

            // Hooks
            HookModel.RemoveHook<TypeOObjectAddedToViewHook>(AddDebugDrawableComponent);
            HookModel.RemoveHook<PropertyChangedHook>(ComponentPropertyChanged);
            HookModel.RemoveHook<ComponentFocusHook>(ComponentFocusChanged);
        }

        internal void AddDebugDrawableComponent(TypeOObjectAddedToViewHook hook)
        {
            Vec2 position = hook.Component.Properties.FirstOrDefault(p => p.Name == "Position")?.ToValue<Vec2>() ?? Vec2.Zero;
            Vec2 size = hook.Component.Properties.FirstOrDefault(p => p.Name == "Size")?.ToValue<Vec2>() ?? Vec2.Zero;
            var drawable = hook.Context.Game.Scenes.CurrentScene.Drawables.Create<Debug2DDrawable>();
            drawable.TypeOObject = hook.TypeOObject;
            drawable.Position = position;
            drawable.Size = size;
            drawable.UpdateLines();
            DebugDrawablesMap.Add(hook.Component.ID, drawable);
        }

        internal void ComponentPropertyChanged(PropertyChangedHook hook)
        {
            if (DebugDrawablesMap.ContainsKey(hook.ID))
            {
                var drawable = DebugDrawablesMap[hook.ID];
                if (hook.Property.Name == "Position" || hook.Property.Name == "Size")
                {
                    Vec2 position = hook.Property.Name == "Position" ? hook.Property.ToValue<Vec2>() : drawable.Position;
                    Vec2 size = hook.Property.Name == "Size" ? hook.Property.ToValue<Vec2>() : drawable.Size;
                    drawable.Position = position;
                    drawable.Size = size;
                    drawable.UpdateLines();
                }
            }
        }

        internal void ComponentFocusChanged(ComponentFocusHook hook)
        {
            if (CurrentFocusedDebug2DDrawable != null)
            {
                CurrentFocusedDebug2DDrawable.Focused = false;
                CurrentFocusedDebug2DDrawable = null;
            }
            if (DebugDrawablesMap.ContainsKey(hook.Component.ID))
            {
                CurrentFocusedDebug2DDrawable = DebugDrawablesMap[hook.Component.ID];
                CurrentFocusedDebug2DDrawable.Focused = true;
            }
        }
    }
}