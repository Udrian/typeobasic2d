using System.Collections.Generic;
using TypeOEngine.Typedeaf.Basic2d.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;

namespace TypeOEngine.Typedeaf.TypeOBasic2d
{
    namespace Entities.Drawables
    {
        /// <summary>
        /// Represents a drawable text element that can be rendered on a 2D canvas using a specified font.
        /// </summary>
        /// <remarks>This class allows rendering multi-line text with customizable properties such as
        /// font, scale, rotation, color, and flipping. The text is automatically split into lines based on newline
        /// characters, and the size of the drawable is updated accordingly.</remarks>
        public class DrawableFont : Drawable2d
        {
            /// <summary>
            /// Gets or sets the font used for rendering text.
            /// </summary>
            /// <remarks>Changing the font triggers an update to recalculate the size and layout of
            /// the text.</remarks>
            public Font Font {
                get { return _font; }
                set {
                    var update = _font != value;
                    _font = value;
                    if(update)
                    {
                        UpdateSizeAndLines();
                    }
                }
            }
            private Font _font;

            /// <summary>
            /// Gets or sets the text content.
            /// </summary>
            public string Text {
                get { return _text; }
                set {
                    var update = _text != value;
                    _text = value;
                    if(update)
                    {
                        UpdateSizeAndLines();
                    }
                }
            }
            private string _text;

            /// <summary>
            /// Gets the collection of lines associated with this instance.
            /// </summary>
            public List<string> Lines { get; protected set; }

            /// <summary>
            /// Gets or sets the scale of the object as a two-dimensional vector.
            /// </summary>
            public Vec2 Scale { get; set; }

            /// <summary>
            /// Gets or sets the size of the object as a two-dimensional vector.
            /// </summary>
            public override Vec2 Size { get; protected set; }

            /// <summary>
            /// Gets or sets the rotation angle, in degrees.
            /// </summary>
            public double Rotation { get; set; }

            /// <summary>
            /// Gets or sets the color associated with the object.
            /// </summary>
            public Color Color { get; set; }

            /// <summary>
            /// Gets or sets the flipped state of the object.
            /// </summary>
            public Flipped Flipped { get; set; }

            private void UpdateSizeAndLines()
            {
                Lines.Clear();
                if(Text == null || Font == null)
                {
                    Size = new Vec2(0);
                    return;
                }
                double width, height;
                width = height = 0;
                int startIndex = 0;
                for(int i = 0; i < Text.Length; i++)
                {
                    if(Text[i] == '\n' || i == Text.Length - 1)
                    {
                        var text = Text.Substring(startIndex, i - startIndex + (i == Text.Length - 1 ? 1 : 0));
                        Lines.Add(text);
                        var size = Font.MeasureString(text);
                        if(size.X > width) width = size.X;
                        height += size.Y;
                        startIndex = i + 1;
                    }
                }
                Size = new Vec2(width, height);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="DrawableFont"/> class with default settings.
            /// </summary>
            /// <remarks>The default settings include a scale of <see cref="Vec2.One"/>, a rotation of 0,
            /// a color of <see cref="Color.White"/>, no flipping (<see cref="Flipped.None"/>), and an empty list of
            /// lines.</remarks>
            public DrawableFont() : base()
            {
                Scale = Vec2.One;
                Rotation = 0;
                Color = Color.White;
                Flipped = Flipped.None;
                Lines = new List<string>();
            }

            /// <summary>
            /// Performs initialization logic for the component.
            /// </summary>
            /// <remarks>This method is called during the component's lifecycle to set up any
            /// necessary state or resources. Override this method to provide custom initialization logic.</remarks>
            protected override void Initialize() { }

            /// <summary>
            /// Releases resources used by the object and performs any necessary cleanup operations.
            /// </summary>
            /// <remarks>This method is called to ensure that any unmanaged resources or other
            /// disposable objects associated with the instance are properly released. Override this method to
            /// implement custom cleanup logic specific to the derived class.</remarks>
            protected override void Cleanup()
            {
                //Font?.Cleanup();
            }

            /// <summary>
            /// Draws the text onto the specified canvas using the configured font, position, and other rendering options.
            /// </summary>
            /// <remarks>This method renders the text line by line, starting at the specified position
            /// and applying the configured font, scale, rotation, color, and other properties. If the font or text is
            /// not set, the method does nothing.</remarks>
            /// <param name="canvas">The canvas on which the text will be drawn. Cannot be <see langword="null"/>.</param>
            public override void Draw(ICanvas canvas)
            {
                if(Font == null || Text == null) return;
                var position = new Vec2(Position.X, Position.Y);
                var ySize = Font.MeasureString(Text).Y;
                foreach (var line in Lines)
                {
                    canvas.DrawText(
                        line,
                        Font,
                        position,
                        scale: Scale,
                        rotation: Rotation,
                        color: Color,
                        flipped: Flipped,
                        anchor: Entity as Entity2d
                    );

                    position.Y += ySize;
                }
            }
        }
    }
}