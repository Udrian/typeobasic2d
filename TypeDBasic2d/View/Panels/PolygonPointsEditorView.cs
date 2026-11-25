using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Threading;
using TypeD.Models.Data;
using TypeDBasic2d.Models.Data;
using TypeOBasic2d.Typedeaf.Basic2d.Entities.Drawables.Primitives;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;

namespace TypeDBasic2d.View.Panels
{
    /// <summary>
    /// Panel for editing the points of a polygon. The points can also be edited in the viewer, where the
    /// <see cref="TypeDBasic2d.Typedeaf.Core.Entities.Drawables.PolygonEditor2DDrawable"/> overlay is shown while this panel is open.
    /// </summary>
    public class PolygonPointsEditorView : UserControl
    {
        private readonly PropertyEditorSession _session;
        private readonly PolygonEditState _state;
        private readonly StackPanel _rows = new() { Spacing = 2 };
        private readonly ToggleButton _addMode;
        private int _renderedCount = -1;
        private bool _updatingColors;
        private int _renderedSelection = -2;

        /// <summary>Creates the editor for a session.</summary>
        public PolygonPointsEditorView(PropertyEditorSession session, PolygonEditState state)
        {
            _session = session;
            _state = state;

            var add = CreateIconButton(AddIcon, "Add point", AddPoint, true);
            add.Margin = new Avalonia.Thickness(0, 0, 4, 0);
            _addMode = new ToggleButton { Content = "Add in viewer", IsChecked = state.AddMode };
            _addMode.IsCheckedChanged += (_, _) => state.AddMode = _addMode.IsChecked == true;

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Avalonia.Thickness(2, 6, 0, 8), Children = { add, _addMode } };

            var hint = new TextBlock
            {
                Text = "Drag points in the viewer.                 click an edge to insert a point, middle-click a point to remove it.",
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Avalonia.Thickness(8, 0, 8, 6),
                Opacity = 0.7
            };

            DockPanel.SetDock(hint, Avalonia.Controls.Dock.Top);
            Content = new DockPanel
            {
                Children =
                {
                    hint,
                    new ScrollViewer { Content = new StackPanel { Children = { _rows, buttons } }, Padding = new Avalonia.Thickness(8, 0) }
                }
            };

            session.Changed += OnExternalChange;
            state.Changed += OnExternalChange;
            Rebuild();
        }

        private void OnExternalChange()
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (_addMode.IsChecked != _state.AddMode)
                    _addMode.IsChecked = _state.AddMode;
                Rebuild();
            });
        }

        private void Rebuild()
        {
            var points = PolygonEditState.CopyPoints(_session);
            if (points.Count != _renderedCount || _rows.Children.Count != points.Count)
            {
                _rows.Children.Clear();
                for (var i = 0; i < points.Count; i++)
                    _rows.Children.Add(CreateRow(i, points.Count));
                _renderedCount = points.Count;
                _renderedSelection = -2;
            }

            for (var i = 0; i < points.Count; i++)
            {
                var row = (Border)_rows.Children[i];
                var fields = (StackPanel)row.Child;
                SetText((TextBox)fields.Children[1], points[i].Position.X);
                SetText((TextBox)fields.Children[2], points[i].Position.Y);
                var color = points[i].Color;
                _updatingColors = true;
                ((ColorPicker)fields.Children[3]).Color = Avalonia.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
                ((ComboBox)fields.Children[4]).SelectedItem = points[i].Blend;
                _updatingColors = false;
            }

            if (_renderedSelection != _state.SelectedIndex)
            {
                _renderedSelection = _state.SelectedIndex;
                for (var i = 0; i < _rows.Children.Count; i++)
                    ((Border)_rows.Children[i]).Background = i == _renderedSelection
                        ? Avalonia.Media.Brushes.DimGray
                        : Avalonia.Media.Brushes.Transparent;
            }
        }

        private static void SetText(TextBox box, double value)
        {
            // Do not overwrite what the user is typing
            if (box.IsFocused)
                return;
            var text = Math.Round(value, 3).ToString(CultureInfo.InvariantCulture);
            if (box.Text != text)
                box.Text = text;
        }

        private const int MinimumPoints = 3;
        private const string AddIcon = "M19,13H13V19H11V13H5V11H11V5H13V11H19V13Z";
        private const string UpIcon = "M7.41,15.41L12,10.83L16.59,15.41L18,14L12,8L6,14L7.41,15.41Z";
        private const string DownIcon = "M7.41,8.58L12,13.17L16.59,8.58L18,10L12,16L6,10L7.41,8.58Z";
        private const string DeleteIcon = "M19,4H15.5L14.5,3H9.5L8.5,4H5V6H19M6,19A2,2 0 0,0 8,21H16A2,2 0 0,0 18,19V7H6V19Z";

        private static Button CreateIconButton(string path, string tip, Action click, bool enabled)
        {
            var button = new Button
            {
                Content = new PathIcon { Data = Avalonia.Media.Geometry.Parse(path), Width = 14, Height = 14 },
                Margin = new Avalonia.Thickness(4, 0, 0, 0),
                Padding = new Avalonia.Thickness(4),
                IsEnabled = enabled,
                Focusable = false
            };
            ToolTip.SetTip(button, tip);
            button.Click += (_, _) => click();
            return button;
        }

        private Border CreateRow(int index, int count)
        {
            var label = new TextBlock { Text = $"{index}", Width = 24, VerticalAlignment = VerticalAlignment.Center };
            var x = new TextBox { Width = 90, Watermark = "X" };
            var y = new TextBox { Width = 90, Watermark = "Y", Margin = new Avalonia.Thickness(4, 0, 0, 0) };
            x.LostFocus += (_, _) => CommitField(index, x, isX: true);
            y.LostFocus += (_, _) => CommitField(index, y, isX: false);
            x.KeyDown += (_, e) => { if (e.Key == Avalonia.Input.Key.Enter) CommitField(index, x, isX: true); };
            y.KeyDown += (_, e) => { if (e.Key == Avalonia.Input.Key.Enter) CommitField(index, y, isX: false); };
            x.GotFocus += (_, _) => _state.SelectedIndex = index;
            y.GotFocus += (_, _) => _state.SelectedIndex = index;

            var fields = new StackPanel { Orientation = Orientation.Horizontal, Children = { label, x, y } };
            var picker = new ColorPicker { IsAlphaEnabled = true, Margin = new Avalonia.Thickness(4, 0, 0, 0), Width = 40 };
            picker.ColorChanged += (_, e) =>
            {
                if (!_updatingColors)
                    CommitColor(index, e.NewColor);
            };
            TypeDCore.Models.Data.ColorClipboard.Attach(picker);
            fields.Children.Add(picker);
            ToolTip.SetTip(picker, "Point color. Transparent uses the polygon color; otherwise it is blended with it.");
            var blend = new ComboBox
            {
                ItemsSource = Enum.GetValues<PolygonColorBlend>(),
                Width = 110,
                Margin = new Avalonia.Thickness(4, 0, 0, 0),
                SelectedItem = PolygonColorBlend.Default
            };
            blend.SelectionChanged += (_, _) =>
            {
                if (!_updatingColors && blend.SelectedItem is PolygonColorBlend value)
                    CommitBlend(index, value);
            };
            ToolTip.SetTip(blend, "How the point color is blended into the polygon color. Default uses the blend of the polygon.");
            fields.Children.Add(blend);
            fields.Children.Add(CreateIconButton(UpIcon, "Move up", () => MovePoint(index, -1), index > 0));
            fields.Children.Add(CreateIconButton(DownIcon, "Move down", () => MovePoint(index, 1), index < count - 1));
            fields.Children.Add(CreateIconButton(DeleteIcon, "Remove point", () => RemovePoint(index), count > MinimumPoints));
            var row = new Border { Child = fields, Padding = new Avalonia.Thickness(2), Background = Avalonia.Media.Brushes.Transparent };
            row.PointerPressed += (_, _) => _state.SelectedIndex = index;
            return row;
        }

        private void CommitField(int index, TextBox box, bool isX)
        {
            var points = PolygonEditState.CopyPoints(_session);
            if (index >= points.Count)
                return;
            if (!double.TryParse(box.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) || double.IsNaN(value) || double.IsInfinity(value))
            {
                Rebuild();
                return;
            }

            var point = points[index];
            var position = point.Position;
            if (isX)
                position.X = value;
            else
                position.Y = value;
            if (position == point.Position)
                return;
            point.Position = position;
            points[index] = point;
            _session.SetValue(points);
        }

        private void CommitColor(int index, Avalonia.Media.Color value)
        {
            var points = PolygonEditState.CopyPoints(_session);
            if (index >= points.Count)
                return;
            var point = points[index];
            point.Color = new Color(value.A, value.R, value.G, value.B);
            if (point == points[index])
                return;
            points[index] = point;
            _session.SetValue(points);
        }

        private void CommitBlend(int index, PolygonColorBlend value)
        {
            var points = PolygonEditState.CopyPoints(_session);
            if (index >= points.Count || points[index].Blend == value)
                return;
            var point = points[index];
            point.Blend = value;
            points[index] = point;
            _session.SetValue(points);
        }

        private void AddPoint()
        {
            var points = PolygonEditState.CopyPoints(_session);
            var insertAt = _state.SelectedIndex >= 0 && _state.SelectedIndex < points.Count ? _state.SelectedIndex + 1 : points.Count;
            Vec2 position;
            if (points.Count == 0)
                position = new Vec2(0, 0);
            else
            {
                var before = points[insertAt - 1].Position;
                var after = points[insertAt % points.Count].Position;
                position = new Vec2((before.X + after.X) / 2, (before.Y + after.Y) / 2);
            }
            points.Insert(insertAt, new PolygonPoint(position, PolygonEditState.DefaultColor));
            _state.SelectedIndex = insertAt;
            _session.SetValue(points);
        }

        private void RemovePoint(int index)
        {
            var points = PolygonEditState.CopyPoints(_session);
            if (index < 0 || index >= points.Count || points.Count <= MinimumPoints)
                return;
            points.RemoveAt(index);
            _state.SelectedIndex = Math.Min(index, points.Count - 1);
            _session.SetValue(points);
        }

        private void MovePoint(int index, int direction)
        {
            var points = PolygonEditState.CopyPoints(_session);
            var target = index + direction;
            if (index < 0 || index >= points.Count || target < 0 || target >= points.Count)
                return;
            (points[index], points[target]) = (points[target], points[index]);
            _state.SelectedIndex = target;
            _session.SetValue(points);
        }
    }
}
