using TypeD;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;
using TypeDBasic2d.Components;
using TypeDBasic2d.Typedeaf.Core.Entities.Drawables;
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
        IIconModel IconModel { get; set; }

        // Data
        Project Project { get; set; }
        Dictionary<string, Debug2DDrawable> DebugDrawablesMap { get; set; }
        Dictionary<object, string> DebugDrawableKeys { get; set; }
        List<Debug2DDrawable> DebugDrawablesSnapshot { get; set; }
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
            IconModel = Resources.Get<IIconModel>();

            // Data
            DebugDrawablesMap = new Dictionary<string, Debug2DDrawable>();
            DebugDrawableKeys = new Dictionary<object, string>(ReferenceEqualityComparer.Instance);

            ComponentProvider.AddBaseTypeComponent(Basic2dComponent.Entity2dComponent());
            ComponentProvider.AddBaseTypeComponent(Basic2dComponent.Drawable2dComponent());
            ComponentProvider.AddBaseTypeComponent(Basic2dComponent.DrawableTextureComponent());
            ComponentProvider.AddBaseTypeComponent(Basic2dComponent.DrawableFontComponent());
            ComponentProvider.AddBaseTypeComponent(Basic2dComponent.DrawableRectangleComponent());
            ComponentProvider.AddBaseTypeComponent(Basic2dComponent.DrawableCircleComponent());
            ComponentProvider.AddBaseTypeComponent(Basic2dComponent.DrawableTriangleComponent());

            // Hooks
            HookModel.AddHook<TypeOObjectAddedToViewHook>(AddDebugDrawableComponent);
            HookModel.AddHook<TypeOObjectRemovedFromViewHook>(RemoveDebugDrawableComponent);
            HookModel.AddHook<CloseComponentHook>(ComponentClosed);
            HookModel.AddHook<ViewerUnloadedHook>(ViewerUnloaded);
            HookModel.AddHook<PropertyChangedHook>(ComponentPropertyChanged);
            HookModel.AddHook<ComponentFocusHook>(ComponentFocusChanged);
            HookModel.AddHook<ViewerHitTestHook>(ViewerHitTest);

            // Content
            IconModel.AddDefaultIconsPath(Path.Combine(project.Location, "Assets", "Icons"));
        }

        public override void Uninitializer()
        {
            // Data
            ComponentProvider.RemoveBaseTypeComponent(Basic2dComponent.Entity2dComponent());
            ComponentProvider.RemoveBaseTypeComponent(Basic2dComponent.Drawable2dComponent());
            ComponentProvider.RemoveBaseTypeComponent(Basic2dComponent.DrawableTextureComponent());
            ComponentProvider.RemoveBaseTypeComponent(Basic2dComponent.DrawableFontComponent());
            ComponentProvider.RemoveBaseTypeComponent(Basic2dComponent.DrawableRectangleComponent());
            ComponentProvider.RemoveBaseTypeComponent(Basic2dComponent.DrawableCircleComponent());
            ComponentProvider.RemoveBaseTypeComponent(Basic2dComponent.DrawableTriangleComponent());

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
                SelectionController.GetDebugDrawables = GetDebugDrawablesSnapshot;
            }

            Vec2 position = hook.Component.Properties.FirstOrDefault(p => p.Name == "Position")?.ToValue<Vec2>() ?? Vec2.Zero;
            Vec2 size = hook.Component.Properties.FirstOrDefault(p => p.Name == "Size")?.ToValue<Vec2>() ?? Vec2.Zero;
            // The overlay always lives on the default draw stack and follows the camera. Objects on draw stacks
            // that ignore the camera are mapped into editor world space by the overlay itself.
            var scene = hook.Context.Game.Scenes.CurrentScene;
            var drawable = scene.Drawables.Create<Debug2DDrawable>();
            drawable.CameraCanvas = scene.Canvas;
            drawable.TypeOObject = hook.TypeOObject;
            drawable.Component = hook.Component;
            drawable.EditorToolModel = EditorToolModel;
            drawable.Position = position;
            drawable.Size = size;
            drawable.UpdateLines();
            var key = hook.Component.FullParentPath + hook.Component.ID;
            DebugDrawablesMap.Add(key, drawable);
            DebugDrawableKeys.Add(hook.TypeOObject, key);
            DebugDrawablesSnapshot = null;
            UpdateSelectionRole(drawable);
        }

        internal void RemoveDebugDrawableComponent(TypeOObjectRemovedFromViewHook hook)
        {
            if (hook.TypeOObject is not (TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Entity2d
                or TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables.Drawable2d))
                return;

            if (DebugDrawableKeys.TryGetValue(hook.TypeOObject, out var key)
                && DebugDrawablesMap.TryGetValue(key, out var drawable))
            {
                hook.Context.Game.Scenes.CurrentScene.Drawables.Destroy(drawable);
                DebugDrawablesMap.Remove(key);
                DebugDrawableKeys.Remove(hook.TypeOObject);
                DebugDrawablesSnapshot = null;
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
            DebugDrawableKeys.Clear();
            DebugDrawablesSnapshot = null;
            SelectionController = null;
        }

        private IReadOnlyList<Debug2DDrawable> GetDebugDrawablesSnapshot()
        {
            return DebugDrawablesSnapshot ??= DebugDrawablesMap.Values.ToList();
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
            var selected = new HashSet<Component>(SelectedComponents, ReferenceEqualityComparer.Instance);
            var parentsOfSelected = new HashSet<Component>(ReferenceEqualityComparer.Instance);
            foreach(var component in selected)
                for(var parent = component?.ParentComponent; parent != null; parent = parent.ParentComponent)
                    parentsOfSelected.Add(parent);

            foreach (var drawable in DebugDrawablesMap.Values)
            {
                UpdateSelectionRole(drawable, selected, parentsOfSelected);
            }
        }

        private void UpdateSelectionRole(Debug2DDrawable drawable)
        {
            drawable.Focused = SelectedComponents.Any(component => ReferenceEquals(component, drawable.Component));
            drawable.IsParentOfSelected = !drawable.Focused
                && SelectedComponents.Any(component => IsAncestorOf(drawable.Component, component));
            drawable.IsChildOfSelected = !drawable.Focused
                && SelectedComponents.Any(component => IsAncestorOf(component, drawable.Component));
        }

        private static void UpdateSelectionRole(
            Debug2DDrawable drawable,
            HashSet<Component> selected,
            HashSet<Component> parentsOfSelected)
        {
            drawable.Focused = selected.Contains(drawable.Component);
            drawable.IsParentOfSelected = !drawable.Focused && parentsOfSelected.Contains(drawable.Component);
            drawable.IsChildOfSelected = !drawable.Focused
                && HasSelectedAncestor(drawable.Component, selected);
        }

        private static bool HasSelectedAncestor(Component component, HashSet<Component> selected)
        {
            for(var parent = component?.ParentComponent; parent != null; parent = parent.ParentComponent)
                if(selected.Contains(parent))
                    return true;
            return false;
        }

        private static bool IsAncestorOf(Component ancestor, Component node)
        {
            for(var parent = node?.ParentComponent; parent != null; parent = parent.ParentComponent)
                if(ReferenceEquals(parent, ancestor))
                    return true;
            return false;
        }
    }
}