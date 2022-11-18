using System.Collections.Generic;
using TypeOEngine.Typedeaf.Basic2d.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;

namespace TypeOEngine.Typedeaf.TypeOBasic2d
{
    namespace Entities.Drawables
    {
        public class DrawableFont : Drawable2d
        {
            private Font _font;
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
            private string _text;
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

            public List<string> Lines { get; protected set; }

            public Vec2 Scale { get; set; }
            public double Rotation { get; set; }
            public Color Color { get; set; }
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
            public override Vec2 Size { get; protected set; }

            public DrawableFont() : base()
            {
                Scale = Vec2.One;
                Rotation = 0;
                Color = Color.White;
                Flipped = Flipped.None;
                Lines = new List<string>();
            }

            public override void Initialize() { }

            public override void Cleanup()
            {
                //Font?.Cleanup();
            }

            public override void Draw(ICanvas canvas)
            {
                if(Font == null || Text == null) return;
                var position = new Vec2(Position.X, Position.Y);
                var ySize = Font.MeasureString(Text).Y;
                foreach (var line in Lines)
                {
                    canvas.DrawText(
                        Font,
                        line,
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