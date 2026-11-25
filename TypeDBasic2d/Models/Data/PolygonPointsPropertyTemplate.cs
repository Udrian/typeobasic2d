using System.Runtime.CompilerServices;
using Avalonia.Controls;
using TypeD.Models.Data;
using TypeDBasic2d.View.Panels;
using TypeOBasic2d.Typedeaf.Basic2d.Entities.Drawables.Primitives;
using TypeOEngine.Typedeaf.Core.Common;

namespace TypeDBasic2d.Models.Data
{
    /// <summary>
    /// Opens the polygon points editor: a panel listing the points, together with a viewer overlay where the
    /// points are added, moved and removed directly on the polygon.
    /// </summary>
    public class PolygonPointsPropertyTemplate : PanelPropertyTemplate
    {
        private static readonly ConditionalWeakTable<PropertyEditorSession, PolygonEditState> States = new();

        /// <inheritdoc/>
        public override string EditorTitle => "Polygon points";

        /// <summary>Gets the state shared by the panel and the viewer overlay of a session.</summary>
        public static PolygonEditState GetState(PropertyEditorSession session)
        {
            return States.GetValue(session, _ => new PolygonEditState());
        }

        /// <inheritdoc/>
        public override Control CreateEditor(PropertyEditorSession session)
        {
            return new PolygonPointsEditorView(session, GetState(session));
        }

        /// <inheritdoc/>
        public override string GetSummary(object value)
        {
            return value is List<PolygonPoint> points ? $"{points.Count} points" : "No points";
        }
    }
}
