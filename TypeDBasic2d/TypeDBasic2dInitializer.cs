using System.Collections.Generic;
using System.Linq;
using TypeD;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;
using TypeDBasic2d.Components;
using TypeDBasic2d.Typedeaf.Core.Entities.Drawables;
using TypeDCore.Commands.Data;
using TypeDCore.Models.Data.Hooks;
using TypeDCore.Models.Interfaces;
using TypeOEngine.Typedeaf.Core.Common;

namespace TypeDBasic2d
{
    internal class TypeDBasic2dInitializer : TypeDModuleInitializer
    {
        // Providers
        IComponentProvider ComponentProvider { get; set; }

        // Models
        IHookModel HookModel { get; set; }
        IEditorToolModel EditorToolModel { get; set; }
        IEditorCommandRegistry EditorCommandRegistry { get; set; }

        // Data
        Project Project { get; set; }
        Dictionary<string, Debug2DDrawable> DebugDrawablesMap { get; set; }
        SelectionController2DDrawable SelectionController { get; set; }
        List<Component> SelectedComponents { get; set; } = new List<Component>();

        public override void Initializer(Project project)
        {
            Project = project;

            // Providers
            ComponentProvider = Resources.Get<IComponentProvider>();

            // Models
            HookModel = Resources.Get<IHookModel>();
            EditorToolModel = Resources.Get<IEditorToolModel>();
            EditorCommandRegistry = Resources.Get<IEditorCommandRegistry>();

            // Data
            DebugDrawablesMap = new Dictionary<string, Debug2DDrawable>();

            ComponentProvider.AddBaseTypeComponent(Basic2dComponent.Entity2dComponent());
            ComponentProvider.AddBaseTypeComponent(Basic2dComponent.Drawable2dComponent());
            ComponentProvider.AddBaseTypeComponent(Basic2dComponent.DrawableTextureComponent());

            // Hooks
            HookModel.AddHook<TypeOObjectAddedToViewHook>(AddDebugDrawableComponent);
            HookModel.AddHook<TypeOObjectRemovedFromViewHook>(RemoveDebugDrawableComponent);
            HookModel.AddHook<CloseComponentHook>(ComponentClosed);
            HookModel.AddHook<ViewerUnloadedHook>(ViewerUnloaded);
            HookModel.AddHook<PropertyChangedHook>(ComponentPropertyChanged);
            HookModel.AddHook<ComponentFocusHook>(ComponentFocusChanged);
            HookModel.AddHook<ViewerHitTestHook>(ViewerHitTest);
        }

        public override void Uninitializer()
        {
            // Data
            ComponentProvider.RemoveBaseTypeComponent(Basic2dComponent.Entity2dComponent());
            ComponentProvider.RemoveBaseTypeComponent(Basic2dComponent.Drawable2dComponent());
            ComponentProvider.RemoveBaseTypeComponent(Basic2dComponent.DrawableTextureComponent());

            // Hooks
            HookModel.RemoveHook<TypeOObjectAddedToViewHook>(AddDebugDrawableComponent);
            HookModel.RemoveHook<TypeOObjectRemovedFromViewHook>(RemoveDebugDrawableComponent);
            HookModel.RemoveHook<CloseComponentHook>(ComponentClosed);
            HookModel.RemoveHook<ViewerUnloadedHook>(ViewerUnloaded);
            HookModel.RemoveHook<PropertyChangedHook>(ComponentPropertyChanged);
            HookModel.RemoveHook<ComponentFocusHook>(ComponentFocusChanged);
            HookModel.RemoveHook<ViewerHitTestHook>(ViewerHitTest);
            ClearDebugDrawablesMap();
        }

        internal void AddDebugDrawableComponent(TypeOObjectAddedToViewHook hook)
        {
            if (hook.TypeOObject is not (TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Entity2d
                or TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables.Drawable2d))
                return;

            // The controller owns all pointer interaction of the overlays, one per loaded view
            if (SelectionController == null)
            {
                SelectionController = hook.Context.Game.Scenes.CurrentScene.Drawables.Create<SelectionController2DDrawable>();
                SelectionController.Project = Project;
                SelectionController.HookModel = HookModel;
                SelectionController.EditorToolModel = EditorToolModel;
                SelectionController.EditorCommandRegistry = EditorCommandRegistry;
                SelectionController.GetDebugDrawables = () => DebugDrawablesMap.Values.ToList();
            }

            Vec2 position = hook.Component.Properties.FirstOrDefault(p => p.Name == "Position")?.ToValue<Vec2>() ?? Vec2.Zero;
            Vec2 size = hook.Component.Properties.FirstOrDefault(p => p.Name == "Size")?.ToValue<Vec2>() ?? Vec2.Zero;
            var drawable = hook.Context.Game.Scenes.CurrentScene.Drawables.Create<Debug2DDrawable>();
            drawable.TypeOObject = hook.TypeOObject;
            drawable.Component = hook.Component;
            drawable.EditorToolModel = EditorToolModel;
            drawable.Position = position;
            drawable.Size = size;
            drawable.UpdateLines();
            DebugDrawablesMap.Add(hook.Component.FullParentPath + hook.Component.ID, drawable);
            UpdateSelectionRoles();
        }

        internal void RemoveDebugDrawableComponent(TypeOObjectRemovedFromViewHook hook)
        {
            if (hook.TypeOObject is not (TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Entity2d
                or TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables.Drawable2d))
                return;

            // Deferred removal runs after the component may have been detached or reparented.
            var entry = DebugDrawablesMap.FirstOrDefault(pair => ReferenceEquals(pair.Value.TypeOObject, hook.TypeOObject));
            if (entry.Value != null)
            {
                var drawable = entry.Value;
                hook.Context.Game.Scenes.CurrentScene.Drawables.Destroy(drawable);
                DebugDrawablesMap.Remove(entry.Key);
            }
        }

        internal void ComponentClosed(CloseComponentHook hook)
        {
            if (hook.Project == Project)
                ClearDebugDrawablesMap();
        }

        internal void ViewerUnloaded(ViewerUnloadedHook hook)
        {
            if (hook.Project == Project)
                ClearDebugDrawablesMap();
        }

        private void ClearDebugDrawablesMap()
        {
            DebugDrawablesMap.Clear();
            SelectionController = null;
        }

        internal void ComponentPropertyChanged(PropertyChangedHook hook)
        {
            if (DebugDrawablesMap.ContainsKey(hook.Component.FullParentPath + hook.Component.ID))
            {
                var drawable = DebugDrawablesMap[hook.Component.FullParentPath + hook.Component.ID];
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

        internal void ViewerHitTest(ViewerHitTestHook hook)
        {
            if (hook.Project != Project)
                return;

            if (hook.Target != null)
            {
                if (DebugDrawablesMap.TryGetValue(hook.Target.FullParentPath + hook.Target.ID, out var targetDrawable))
                {
                    var origin = targetDrawable.ScreenPosition;
                    hook.OriginX = origin.X;
                    hook.OriginY = origin.Y;
                    var (pos, size) = targetDrawable.GetBounds();
                    hook.TargetFound = true;
                    hook.BoundsX = pos.X;
                    hook.BoundsY = pos.Y;
                    hook.BoundsWidth = size.X;
                    hook.BoundsHeight = size.Y;
                }
                return;
            }

            if (hook.Result != null)
                return;

            var point = new Vec2(hook.X, hook.Y);
            Component hit = null;
            try
            {
                // The hit with the highest draw order wins; entities without one fall back to the hierarchy
                hit = SelectionController2DDrawable.PickTopMost(DebugDrawablesMap.Values.ToList(), point)?.Component;
            }
            catch (System.InvalidOperationException)
            {
            }

            hook.Result = hit;
        }

        internal void ComponentFocusChanged(ComponentFocusHook hook)
        {
            SelectedComponents = hook.Selection != null && hook.Selection.Contains(hook.Component)
                ? hook.Selection.ToList()
                : new List<Component> { hook.Component };
            UpdateSelectionRoles();
        }

        private void UpdateSelectionRoles()
        {
            foreach (var drawable in DebugDrawablesMap.Values)
            {
                drawable.Focused = SelectedComponents.Any(c => ReferenceEquals(c, drawable.Component));
                drawable.IsParentOfSelected = !drawable.Focused && SelectedComponents.Any(c => IsAncestorOf(drawable.Component, c));
                drawable.IsChildOfSelected = !drawable.Focused && SelectedComponents.Any(c => IsAncestorOf(c, drawable.Component));
            }
        }

        private static bool IsAncestorOf(Component ancestor, Component node)
        {
            for (var parent = node?.ParentComponent; parent != null; parent = parent.ParentComponent)
            {
                if (ReferenceEquals(parent, ancestor))
                    return true;
            }
            return false;
        }
    }
}