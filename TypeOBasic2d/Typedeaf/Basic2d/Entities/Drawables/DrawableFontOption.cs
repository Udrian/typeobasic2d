using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Contents;

namespace TypeOEngine.Typedeaf.TypeOBasic2d
{
    namespace Entities.Drawables
    {
        /// <summary>
        /// Options for creating a <see cref="DrawableFont"/>.
        /// </summary>
        public class DrawableFontOption : Drawable2dOption<DrawableFont>
        {
            /// <summary>Gets or sets the font to use.</summary>
            public Font Font { get; set; }
            /// <summary>Gets or sets the text to draw.</summary>
            public string Text { get; set; }
            /// <summary>Gets or sets the text color.</summary>
            public Color? Color { get; set; }
            /// <summary>Gets or sets how the text is flipped.</summary>
            public Flipped? Flipped { get; set; }

            /// <summary>
            /// Applies the options that were set to the given drawable.
            /// </summary>
            /// <param name="obj">The drawable to configure.</param>
            /// <returns><c>true</c> if the drawable was configured; otherwise <c>false</c>.</returns>
            public override bool Create(DrawableFont obj)
            {
                if(!base.Create(obj)) return false;

                if(Font != null)
                    obj.Font = Font;
                if(Text != null)
                    obj.Text = Text;
                if(Color.HasValue)
                    obj.Color = Color.Value;
                if(Flipped.HasValue)
                    obj.Flipped = Flipped.Value;

                return true;
            }
        }
    }
}
