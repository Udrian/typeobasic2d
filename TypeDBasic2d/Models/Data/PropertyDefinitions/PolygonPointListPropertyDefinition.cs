using System.Text;
using TypeD.Models.Data;
using TypeD.Models.Data.PropertyDefinitions;
using TypeD.ViewModel;
using TypeOBasic2d.Typedeaf.Basic2d.Entities.Drawables.Primitives;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;
using TypeOEngine.Typedeaf.Core.Common;

namespace TypeDBasic2d.Models.Data.PropertyDefinitions
{
    /// <summary>
    /// Defines how a list of polygon points (position and color) is stored, written as code and edited. The points are edited with
    /// <see cref="PolygonPointsPropertyTemplate"/> in the property editor panel.
    /// </summary>
    public class PolygonPointListPropertyDefinition : PropertyDefinition<List<PolygonPoint>, PropertyNode>
    {
        /// <inheritdoc/>
        public override Type PropertyTemplateType => typeof(PolygonPointsPropertyTemplate);

        /// <inheritdoc/>
        protected override string TemplateUri => "avares://TypeDitor/View/Panels/PropertyTemplates/PanelPropertyTemplate.axaml";
        /// <inheritdoc/>
        protected override string TemplateKey => "PanelPropertyTemplate";

        /// <inheritdoc/>
        public override string ToCode(List<PolygonPoint> value)
        {
            var code = new StringBuilder();
            code.Append($"new System.Collections.Generic.List<{typeof(PolygonPoint).FullName}> {{ ");
            code.Append(string.Join(", ", value.Select(point =>
                $"new {typeof(PolygonPoint).FullName}(new {typeof(Vec2).FullName}({DoublePropertyDefinition.DoubleToCode(point.Position.X)}, {DoublePropertyDefinition.DoubleToCode(point.Position.Y)}), "
                + $"new {typeof(Color).FullName}({point.Color.A}, {point.Color.R}, {point.Color.G}, {point.Color.B}), {typeof(PolygonColorBlend).FullName}.{point.Blend})")));
            code.Append(" }");
            return code.ToString();
        }
    }
}
