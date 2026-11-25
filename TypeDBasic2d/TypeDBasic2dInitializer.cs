using TypeD;
using System.Collections.Concurrent;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;
using TypeD.View;
using TypeDBasic2d.Components;
using TypeDBasic2d.Models.Data;
using TypeDBasic2d.Models.Data.PropertyDefinitions;
using TypeOBasic2d.Typedeaf.Basic2d.Entities.Drawables.Primitives;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities;
using TypeDBasic2d.Typedeaf.Core.Entities.Drawables;
using TypeDCore.Models.Data.Hooks;
using TypeDCore.Models.Data;
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
        IPropertyModel PropertyModel { get; set; }

        // Data
        Project Project { get; set; }
        ConcurrentDictionary<string, Debug2DDrawable> DebugDrawablesMap { get; set; }
        Dictionary<object, string> DebugDrawableKeys { get; set; }
        List<Debug2DDrawable> DebugDrawablesSnapshot { get; set; }
        SelectionController2DDrawable SelectionController { get; set; }
        PolygonEditor2DDrawable PolygonEditor { get; set; }
        volatile PropertyEditorSession PolygonSession;
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
            PropertyModel = Resources.Get<IPropertyModel>();

            // Data
            DebugDrawablesMap = new ConcurrentDictionary<string, Debug2DDrawable>();
            DebugDrawableKeys = new Dictionary<object, string>(ReferenceEqualityComparer.Instance);

            ComponentProvider.AddBaseTypeComponent(Basic2dComponent.Entity2dComponent());
            ComponentProvider.AddBaseTypeComponent(Basic2dComponent.Drawable2dComponent());
            ComponentProvider.AddBaseTypeComponent(Basic2dComponent.DrawableTextureComponent());
            ComponentProvider.AddBaseTypeComponent(Basic2dComponent.DrawableFontComponent());
            ComponentProvider.AddBaseTypeComponent(Basic2dComponent.DrawableRectangleComponent());
            ComponentProvider.AddBaseTypeComponent(Basic2dComponent.DrawableCircleComponent());
            ComponentProvider.AddBaseTypeComponent(Basic2dComponent.DrawableTriangleComponent());
            ComponentProvider.AddBaseTypeComponent(Basic2dComponent.DrawablePolygonComponent());
            PropertyModel.Register(new PolygonPointListPropertyDefinition());
            PropertyModel.RegisterTemplate(new PolygonPointsPropertyTemplate());

            // Hooks
            HookModel.AddHook<TypeOObjectAddedToViewHook>(AddDebugDrawableComponent);
            HookModel.AddHook<TypeOObjectRemovedFromViewHook>(RemoveDebugDrawableComponent);
            HookModel.AddHook<CloseComponentHook>(ComponentClosed);
            HookModel.AddHook<ViewerUnloadedHook>(ViewerUnloaded);
            HookModel.AddHook<PropertyChangedHook>(ComponentPropertyChanged);
            HookModel.AddHook<ComponentFocusHook>(ComponentFocusChanged);
            HookModel.AddHook<ViewerHitTestHook>(ViewerHitTest);
            HookModel.AddHook<ViewerNudgeHook>(ViewerNudge);
            HookModel.AddHook<PropertyEditorOpenedHook>(PropertyEditorOpened);
            HookModel.AddHook<PropertyEditorClosedHook>(PropertyEditorClosed);
            HookModel.AddHook<ExtractPropertiesHook>(ExtractPolygonProperties);
            HookModel.AddHook<ViewerDoubleClickHook>(ViewerDoubleClicked);
            HookModel.AddHook<ComponentContextMenuHook>(ComponentContextMenuOpened);

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
            ComponentProvider.RemoveBaseTypeComponent(Basic2dComponent.DrawablePolygonComponent());
            PropertyModel.Unregister(typeof(List<PolygonPoint>));
            PropertyModel.UnregisterTemplate(typeof(PolygonPointsPropertyTemplate));
            EditorToolModel.SelectionInputSuspended = false;

            // Hooks
            HookModel.RemoveHook<TypeOObjectAddedToViewHook>(AddDebugDrawableComponent);
            HookModel.RemoveHook<TypeOObjectRemovedFromViewHook>(RemoveDebugDrawableComponent);
            HookModel.RemoveHook<CloseComponentHook>(ComponentClosed);
            HookModel.RemoveHook<ViewerUnloadedHook>(ViewerUnloaded);
            HookModel.RemoveHook<PropertyChangedHook>(ComponentPropertyChanged);
            HookModel.RemoveHook<ComponentFocusHook>(ComponentFocusChanged);
            HookModel.RemoveHook<ViewerHitTestHook>(ViewerHitTest);
            HookModel.RemoveHook<ViewerNudgeHook>(ViewerNudge);
            HookModel.RemoveHook<PropertyEditorOpenedHook>(PropertyEditorOpened);
            HookModel.RemoveHook<PropertyEditorClosedHook>(PropertyEditorClosed);
            HookModel.RemoveHook<ExtractPropertiesHook>(ExtractPolygonProperties);
            HookModel.RemoveHook<ViewerDoubleClickHook>(ViewerDoubleClicked);
            HookModel.RemoveHook<ComponentContextMenuHook>(ComponentContextMenuOpened);
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
            // The size of a polygon is derived from its points
            if (hook.TypeOObject is DrawablePolygon polygon)
                size = polygon.Size;

            if (PolygonEditor == null)
            {
                PolygonEditor = hook.Context.Game.Scenes.CurrentScene.Drawables.Create<PolygonEditor2DDrawable>();
                PolygonEditor.Project = Project;
                PolygonEditor.EditorToolModel = EditorToolModel;
                PolygonEditor.EditorCommandRegistry = EditorCommandRegistry;
                PolygonEditor.GetSession = () => PolygonSession;
                PolygonEditor.FindOverlay = component =>
                    DebugDrawablesMap.TryGetValue(component.FullParentPath + component.ID, out var overlay) ? overlay : null;
            }
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
            if (!DebugDrawablesMap.TryAdd(key, drawable))
                throw new InvalidOperationException($"A viewer overlay already exists for '{key}'.");
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
                DebugDrawablesMap.TryRemove(key, out _);
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
            PolygonEditor = null;
        }

        internal void PropertyEditorOpened(PropertyEditorOpenedHook hook)
        {
            if (hook.Session.Template is PolygonPointsPropertyTemplate)
                PolygonSession = hook.Session;
        }

        internal void PropertyEditorClosed(PropertyEditorClosedHook hook)
        {
            if (ReferenceEquals(PolygonSession, hook.Session))
                PolygonSession = null;
        }

        // The polygon size is the extent of its points, so it cannot be edited directly
        internal void ExtractPolygonProperties(ExtractPropertiesHook hook)
        {
            if (hook.Component == null || hook.Component.FullName != typeof(DrawablePolygon).FullName && !IsDerivedFromPolygon(hook.Component))
                return;
            // Size is applied by scaling Points, which is what gets generated
            foreach (var property in hook.Properties.Where(candidate => candidate.Name == "Size"))
                property.GenerateCode = false;
        }

        private static bool IsOrDerivesFrom(Component component, Type type)
        {
            if (component == null)
                return false;
            return component.FullName == type.FullName || IsDerivedFrom(component, type);
        }

        private static bool IsDerivedFrom(Component component, Type type)
        {
            for (var parent = component.BaseInheritedComponent; parent != null; parent = parent.BaseInheritedComponent)
                if (parent.FullName == type.FullName)
                    return true;
            return false;
        }

        private static bool Is2dDrawableComponent(Component component, IReadOnlyList<Debug2DDrawable> overlays)
        {
            if (overlays.Any(overlay => ReferenceEquals(overlay.Component, component) && overlay.TypeOObject is Drawable2d))
                return true;

            // Built-in drawable metadata skips Drawable2d in its editor inheritance chain.
            for (var current = component; current != null; current = current.BaseInheritedComponent)
                if (typeof(Drawable2d).IsAssignableFrom(typeof(Drawable2d).Assembly.GetType(current.FullName)))
                    return true;
            return false;
        }

        private bool EditPolygonPoints(Project project, Component component)
        {
            return IsOrDerivesFrom(component, typeof(DrawablePolygon))
                && Resources.Get<IPropertyEditorModel>().OpenProperty(project, component, "Points");
        }

        internal void ViewerDoubleClicked(ViewerDoubleClickHook hook)
        {
            if (!hook.Handled && EditPolygonPoints(hook.Project ?? Project, hook.Component))
                hook.Handled = true;
        }

        internal void ComponentContextMenuOpened(ComponentContextMenuHook hook)
        {
            var component = hook.SelectedComponent ?? hook.OpenedComponent;
            var project = hook.Project ?? Project;
            if (component == null || hook.Menu == null)
                return;

            var isPolygon = IsOrDerivesFrom(component, typeof(DrawablePolygon));
            var hasTexture = isPolygon || IsOrDerivesFrom(component, typeof(DrawableTexture));
            var hasFont = IsOrDerivesFrom(component, typeof(DrawableFont));
            var overlays = DebugDrawablesMap.Values.ToList();
            var isDrawable = Is2dDrawableComponent(component, overlays);
            if (isPolygon || hasTexture || hasFont || isDrawable)
                hook.Menu.Items.Add(new MenuItem { IsSeparator = true });

            if (isDrawable)
            {
                MenuItem OrderItem(string name, DrawOrderMove move) => new(name, _ => ChangeDrawOrder(project, component, move))
                {
                    IsEnabled = DrawOrderPlanner.Plan(project, component, overlays, move) != null
                };
                hook.Menu.Items.Add(new MenuItem
                {
                    Name = "_Send to...",
                    Items = new List<MenuItem>
                    {
                        OrderItem("_Forward", DrawOrderMove.Forward),
                        OrderItem("_Front", DrawOrderMove.Front),
                        OrderItem("_Backward", DrawOrderMove.Backward),
                        OrderItem("_Back", DrawOrderMove.Back)
                    }
                });
            }
            if (isPolygon)
                hook.Menu.Items.Add(new MenuItem("_Edit points", _ => EditPolygonPoints(project, component)));
            if (hasTexture)
                hook.Menu.Items.Add(new MenuItem("_Change Texture", _ => _ = ChangeContentAsync(project, component, "Texture")));
            if (hasFont)
                hook.Menu.Items.Add(new MenuItem("_Change Font", _ => _ = ChangeContentAsync(project, component, "Font")));
        }

        private void ChangeDrawOrder(Project project, Component component, DrawOrderMove move)
        {
            var changes = DrawOrderPlanner.Plan(project, component, DebugDrawablesMap.Values.ToList(), move);
            if (changes == null)
            {
                Resources.Get<ILogModel>().Log($"Could not change draw order for '{component.Name}': no editable overlapping target is available.");
                return;
            }

            var first = changes[0];
            first.AdditionalChanges = changes.Skip(1).ToList();
            if (!EditorCommandRegistry.Execute("component.change-property", first))
                Resources.Get<ILogModel>().Log($"Could not change draw order for '{component.Name}'.");
        }

        private async Task ChangeContentAsync(Project project, Component component, string propertyName)
        {
            var property = component.Properties.FirstOrDefault(p => p.Name == propertyName);
            if (property == null)
                return;

            var extensions = Resources.Get<IContentProvider>().GetAssociationsForEngineType(property.Type).Select(a => a.Extension);
            var path = await Resources.Get<IEditorDialogModel>().SelectContentAsync(project, extensions);
            if (path == null)
                return;

            EditorCommandRegistry.Execute("component.change-property", new ChangePropertyCommandData
            {
                Project = project,
                Property = property,
                Value = path
            });
        }

        private static bool IsDerivedFromPolygon(Component component)
        {
            return IsDerivedFrom(component, typeof(DrawablePolygon));
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
                if (hook.Property.Name == "Points" && hook.Property.Value is List<PolygonPoint> points)
                {
                    drawable.Size = DrawablePolygon.GetSize(DrawablePolygon.GetPositions(points));
                    drawable.UpdateLines();
                }
                if (hook.Property.Name == "Position" || hook.Property.Name == "Size")
                {
                    Vec2 position = hook.Property.Name == "Position" ? hook.Property.ToValue<Vec2>() : drawable.Position;
                    Vec2 size = hook.Property.Name == "Size" ? hook.Property.ToValue<Vec2>() : drawable.Size;
                    drawable.Position = position;
                    drawable.Size = size;
                    drawable.UpdateLines();
                    if (hook.Property.Name == "Size" && IsOrDerivesFrom(hook.Component, typeof(DrawablePolygon)))
                        ApplyPolygonScale(hook.Component, size);
                }
            }
        }

        // Size changes are stored as scaled points, which is what the polygon is generated from
        private void ApplyPolygonScale(Component component, Vec2 size)
        {
            var property = component.Properties.FirstOrDefault(p => p.Name == nameof(DrawablePolygon.Points));
            if (property == null || property.Value is not List<PolygonPoint> current)
                return;
            var scaled = DrawablePolygon.ScalePoints(current, size);
            if (current.SequenceEqual(scaled))
                return;
            EditorCommandRegistry.Execute("component.change-property", new ChangePropertyCommandData
            {
                Project = Project,
                Component = component,
                Property = property,
                Value = scaled
            });
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

        internal void ViewerNudge(ViewerNudgeHook hook)
        {
            if (hook.Project != Project || hook.Components == null || hook.Handled)
                return;

            var selected = new HashSet<Component>(hook.Components, ReferenceEqualityComparer.Instance);
            var overlays = DebugDrawablesMap.Values.Where(overlay => selected.Contains(overlay.Component)).ToList();
            if (overlays.Count == 0)
                return;
            hook.Handled = true;

            if (hook.Tool != EditorTool.Move)
            {
                ViewerTransformNudge(hook, overlays);
                return;
            }

            var targets = overlays.Select(overlay => (Overlay: overlay, Component: overlay.Component,
                    Property: overlay.Component.Properties.FirstOrDefault(property => property.Name == "Position")))
                .Where(target => !target.Component.ReadOnly && !target.Component.Removed && target.Component.IsVisible
                    && target.Property is { ReadOnly: false, Value: Vec2 })
                .ToList();
            var movable = new HashSet<Component>(targets.Select(target => target.Component), ReferenceEqualityComparer.Instance);
            var changes = new List<ChangePropertyCommandData>();
            foreach (var target in targets)
            {
                var ancestor = target.Component.ParentComponent;
                while (ancestor != null && !movable.Contains(ancestor))
                    ancestor = ancestor.ParentComponent;
                if (ancestor != null)
                    continue;

                changes.Add(new ChangePropertyCommandData
                {
                    Project = hook.Project,
                    Component = target.Component,
                    Property = target.Property,
                    Value = (Vec2)target.Property.Value + target.Overlay.GetObjectLocalDelta(hook.Delta)
                });
            }
            if (changes.Count == 0)
                return;

            var first = changes[0];
            first.AdditionalChanges = changes.Skip(1).ToList();
            if (!EditorCommandRegistry.Execute("component.change-property", first))
                Resources.Get<ILogModel>().Log("Could not move the selected components.");
        }

        private void ViewerTransformNudge(ViewerNudgeHook hook, List<Debug2DDrawable> overlays)
        {
            var changes = new List<ChangePropertyCommandData>();
            bool AutoSized(Debug2DDrawable overlay) => overlay.TypeOObject is Entity2d entity
                && (overlay.Component.Properties.FirstOrDefault(property => property.Name == "SizeMode")?.Value is SizeMode mode
                    ? mode : entity.SizeMode) == SizeMode.Auto;
            var selected = new HashSet<Component>(overlays.Where(overlay => !overlay.Component.ReadOnly
                    && !overlay.Component.Removed && overlay.Component.IsVisible
                    && !AutoSized(overlay)
                    && overlay.Component.Properties.Any(property => property.Name == "Size" && !property.ReadOnly && property.Value is Vec2)
                    && overlay.Component.Properties.Any(property => property.Name == "Position" && !property.ReadOnly && property.Value is Vec2))
                .Select(overlay => overlay.Component), ReferenceEqualityComparer.Instance);
            foreach (var overlay in overlays)
            {
                var component = overlay.Component;
                if (component.ReadOnly || component.Removed || !component.IsVisible)
                    continue;
                if (hook.Tool == EditorTool.Resize && AutoSized(overlay))
                {
                    Resources.Get<ILogModel>().Log("Cannot resize an auto-sized entity. Change its SizeMode first.");
                    continue;
                }
                if (hook.Tool == EditorTool.Resize && HasSelectedAncestor(component, selected))
                    continue;

                Property Find(string name) => component.Properties.FirstOrDefault(property => property.Name == name);
                Vec2 VectorValue(string name, Vec2 fallback) => Find(name)?.Value is Vec2 value ? value : fallback;
                double NumberValue(string name, double fallback) => Find(name)?.Value is double value ? value : fallback;
                void Add(Property property, object value) => changes.Add(new ChangePropertyCommandData
                {
                    Project = hook.Project, Component = component, Property = property, Value = value
                });

                var position = Find("Position");
                var size = Find("Size");
                var rotation = Find("Rotation");
                var origin = Find("Origin");
                var angle = NumberValue("Rotation", overlay.GetObjectRotation()) * Math.PI / 180;
                var cos = Math.Cos(angle);
                var sin = Math.Sin(angle);
                var scale = VectorValue("Scale", overlay.TypeOObject switch
                {
                    Drawable2d drawable => drawable.Scale,
                    Entity2d entity => entity.Scale,
                    _ => Vec2.One
                });

                if (hook.Tool == EditorTool.Rotate && rotation is { ReadOnly: false, Value: double degrees })
                {
                    Add(rotation, degrees + hook.Delta.X);
                }
                else if (hook.Tool == EditorTool.Origin && overlay.TypeOObject is Drawable2d
                    && origin is { ReadOnly: false, Value: Vec2 previousOrigin }
                    && position is { ReadOnly: false, Value: Vec2 originPosition }
                    && size?.Value is Vec2 originSize)
                {
                    var local = overlay.GetObjectLocalDelta(hook.Delta);
                    var x = local.X * cos + local.Y * sin;
                    var y = -local.X * sin + local.Y * cos;
                    if (Math.Abs(x) < 1e-12) x = 0;
                    if (Math.Abs(y) < 1e-12) y = 0;
                    if ((x != 0 && (scale.X == 0 || originSize.X == 0))
                        || (y != 0 && (scale.Y == 0 || originSize.Y == 0)))
                    {
                        Resources.Get<ILogModel>().Log("Cannot move the origin of a drawable with zero size or scale.");
                        continue;
                    }
                    Add(origin, previousOrigin + new Vec2(x == 0 ? 0 : x / (scale.X * originSize.X),
                        y == 0 ? 0 : y / (scale.Y * originSize.Y)));
                    Add(position, originPosition + local);
                }
                else if (hook.Tool == EditorTool.Resize && size is { ReadOnly: false, Value: Vec2 currentSize }
                    && position is { ReadOnly: false, Value: Vec2 currentPosition })
                {
                    var local = overlay.GetObjectLocalDelta(hook.Delta);
                    var x = local.X * cos + local.Y * sin;
                    var y = -local.X * sin + local.Y * cos;
                    if (Math.Abs(x) < 1e-12) x = 0;
                    if (Math.Abs(y) < 1e-12) y = 0;
                    if ((x != 0 && scale.X == 0) || (y != 0 && scale.Y == 0))
                    {
                        Resources.Get<ILogModel>().Log("Cannot resize a component with zero scale.");
                        continue;
                    }
                    var edge = new Vec2(x == 0 ? 0 : x / scale.X, y == 0 ? 0 : y / scale.Y);
                    double SizeChange(double delta, double length) => hook.Shrink
                        ? -Math.Min(Math.Abs(delta), Math.Max(0, length - 1)) : Math.Abs(delta);
                    var change = new Vec2(SizeChange(edge.X, currentSize.X), SizeChange(edge.Y, currentSize.Y));
                    if (change == Vec2.Zero)
                        continue;
                    var currentOrigin = VectorValue("Origin", overlay.TypeOObject switch
                    {
                        Drawable2d drawable => drawable.Origin,
                        Entity2d entity => entity.Origin,
                        _ => Vec2.Zero
                    });
                    var offset = new Vec2(change.X * (currentOrigin.X - (edge.X < 0 ? 1 : 0)) * scale.X,
                        change.Y * (currentOrigin.Y - (edge.Y < 0 ? 1 : 0)) * scale.Y);
                    var positionDelta = new Vec2(offset.X * cos - offset.Y * sin, offset.X * sin + offset.Y * cos);
                    Add(size, currentSize + change);
                    if (positionDelta != Vec2.Zero)
                        Add(position, currentPosition + positionDelta);
                }
                else
                {
                    Resources.Get<ILogModel>().Log("Cannot transform the selection: the required properties are unavailable or read-only.");
                }
            }

            if (changes.Count == 0)
                return;
            var first = changes[0];
            first.AdditionalChanges = changes.Skip(1).ToList();
            if (!EditorCommandRegistry.Execute("component.change-property", first))
                Resources.Get<ILogModel>().Log("Could not transform the selected components.");
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