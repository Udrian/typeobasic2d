using TypeOEngine.Typedeaf.Basic2d.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Attributes;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities;

namespace TypeOEngine.Typedeaf.TypeOBasic2d
{
    namespace Entities.Drawables
    {
        /// <summary>
        /// Represents a drawable text element that can be rendered on a 2D canvas using a specified font.
        /// </summary>
        /// <remarks>This class allows rendering multi-line text with customizable properties such as
        /// font, scale, rotation, color, flipping, and size mode. The text is automatically split into lines based on newline
        /// characters, and the size of the drawable is updated accordingly.</remarks>
        public class DrawableFont : Drawable2d
        {
            /// <summary>
            /// Gets or sets the font used for rendering text.
            /// </summary>
            /// <remarks>Changing the font triggers an update to recalculate the size and layout of
            /// the text.</remarks>
            [TypeOProperty("Gets or sets the font used for rendering text.", Group = "Rendering")]
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
            [TypeOProperty("Gets or sets the text content.", Group = "Rendering")]
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
            /// Gets or sets the font size.
            /// </summary>
            [TypeOProperty("Gets or sets the font size.", Group = "Rendering")]
            public int FontSize
            {
                get { return _fontSize; }
                set
                {
                    var update = _fontSize != value;
                    _fontSize = value;
                    if(update)
                    {
                        if(Font != null)
                        {
                            Font.FontSize = FontSize;
                        }
                        UpdateSizeAndLines();
                    }
                }
            }
            private int _fontSize;

            /// <summary>
            /// Gets the collection of lines associated with this instance.
            /// </summary>
            public List<string> Lines { get; protected set; }

            /// <summary>
            /// Gets or sets the color associated with the object.
            /// </summary>
            [TypeOProperty("Gets or sets the color associated with the object.", null, Group = "Rendering")]
            public Color Color { get; set; }

            /// <summary>
            /// Gets or sets the flipped state of the object.
            /// </summary>
            [TypeOProperty("Gets or sets the flipped state of the object.", Group = "Rendering")]
            public Flipped Flipped { get; set; }

            /// <summary>
            /// Gets or sets how the text is rendered within its size bounds.
            /// </summary>
            [TypeOProperty("Gets or sets how the text is rendered within its size bounds.", Group = "Rendering")]
            public SizeMode SizeMode { get; set; }

            private Vec2 TextSize { get; set; }

            private void UpdateSizeAndLines()
            {
                Lines.Clear();
                if(Text == null || Font == null)
                {
                    TextSize = Vec2.Zero;
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
                TextSize = new Vec2(width, height);
                Size = TextSize;
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
                SizeMode = SizeMode.Clip;
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
                
                var drawScale = Scale;
                if (SizeMode == SizeMode.Stretch && Size != Vec2.Zero && TextSize != Vec2.Zero)
                {
                    var stretchScale = new Vec2(
                        TextSize.X != 0 ? Size.X / TextSize.X : 1,
                        TextSize.Y != 0 ? Size.Y / TextSize.Y : 1);
                    drawScale *= stretchScale;
                }

                var actualSize = TextSize * drawScale;
                var originSize = SizeMode == SizeMode.Clip && Size != Vec2.Zero
                    ? Size * Scale
                    : actualSize;
                var originOffset = originSize * Origin;
                var position = new Vec2(Position.X, Position.Y) - originOffset;

                var lineOffset = Vec2.Zero;
                Rectangle? clipRect = SizeMode == SizeMode.Clip && Size != Vec2.Zero
                    ? new Rectangle(position, Size * Scale)
                    : null;
                foreach (var line in Lines)
                {
                    canvas.DrawText(
                        line,
                        Font,
                        position,
                        scale: drawScale,
                        rotation: Rotation,
                        origin: originOffset - lineOffset,
                        color: Color,
                        flipped: Flipped,
                        source: clipRect,
                        anchor: Parent as Entity2d
                    );

                    var lineHeight = Font.MeasureString(line).Y * drawScale.Y;
                    position.Y += lineHeight;
                    lineOffset.Y += lineHeight;
                }
            }
        }
    }
}