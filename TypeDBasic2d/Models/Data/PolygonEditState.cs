using TypeD.Models.Data;
using TypeOBasic2d.Typedeaf.Basic2d.Entities.Drawables.Primitives;
using TypeOEngine.Typedeaf.Core.Common;

namespace TypeDBasic2d.Models.Data
{
    /// <summary>
    /// State shared by the points editor panel and the viewer overlay for one open polygon points session.
    /// </summary>
    public class PolygonEditState
    {
        private volatile int _selectedIndex = -1;
        private volatile bool _addMode;

        /// <summary>Gets or sets the selected point index, or -1.</summary>
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex == value)
                    return;
                _selectedIndex = value;
                Changed?.Invoke();
            }
        }

        /// <summary>Gets or sets a value indicating whether clicking in the viewer appends a point.</summary>
        public bool AddMode
        {
            get => _addMode;
            set
            {
                if (_addMode == value)
                    return;
                _addMode = value;
                Changed?.Invoke();
            }
        }

        /// <summary>Raised when the selection or add mode changed, possibly from the game thread.</summary>
        public event Action Changed;

        /// <summary>The color of a point that uses the polygon color.</summary>
        public static Color DefaultColor => new Color(0, 255, 255, 255);

        /// <summary>Gets the points of a session as a new editable list.</summary>
        public static List<PolygonPoint> CopyPoints(PropertyEditorSession session)
        {
            return session.Value is List<PolygonPoint> points ? new List<PolygonPoint>(points) : new List<PolygonPoint>();
        }

        /// <summary>Gets the point positions of a session.</summary>
        public static List<Vec2> GetPositions(PropertyEditorSession session)
        {
            return DrawablePolygon.GetPositions(session.Value as List<PolygonPoint>);
        }

        /// <summary>Gets the points of a session, used for the color and blend of each point when positions are edited.</summary>
        public static List<PolygonPoint> GetColors(PropertyEditorSession session)
        {
            return CopyPoints(session);
        }

        /// <summary>A point with the default color and blend, used as the style of a new point.</summary>
        public static PolygonPoint DefaultStyle => new PolygonPoint(new Vec2(0, 0), DefaultColor);

        /// <summary>Combines positions with the color and blend of styles into points. Missing styles use the polygon color.</summary>
        public static List<PolygonPoint> Combine(IReadOnlyList<Vec2> positions, IReadOnlyList<PolygonPoint> styles)
        {
            return positions.Select((position, i) =>
            {
                var style = styles != null && i < styles.Count ? styles[i] : DefaultStyle;
                return new PolygonPoint(position, style.Color, style.Blend);
            }).ToList();
        }
    }
}
