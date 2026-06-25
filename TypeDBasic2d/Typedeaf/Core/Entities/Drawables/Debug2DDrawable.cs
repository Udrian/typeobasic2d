using System.Collections.Generic;
using TypeOEngine.Typedeaf.Basic2d.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Desktop.Engine.Services;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;

namespace TypeDBasic2d.Typedeaf.Core.Entities.Drawables
{
    public class Debug2DDrawable : Drawable2d
    {
        public override Vec2 Size { get; set; }
        private List<Vec2> Lines { get; set; }

        public bool Focused { get; set; }
        public MouseInputService MouseInputService { get; set; }
        public TypeOObject TypeOObject { get; set; }

        public override void Draw(ICanvas canvas)
        {
            var mousePos = MouseInputService.MousePosition;
            Color drawColor = Focused ? Color.Green : Color.Red;

            var pos = GetPos();

            if (mousePos.X >= pos.X && mousePos.X <= (pos.X + Size.X) && mousePos.Y >= pos.Y && mousePos.Y <= (pos.Y + Size.Y))
                drawColor = Color.MarineBlue;

            UpdateLines();
            canvas.DrawLines(Lines, drawColor);
        }

        protected override void Cleanup()
        {
        }

        protected override void Initialize()
        {
            DrawOrder = int.MaxValue;
            Lines = new List<Vec2>() { new Vec2(), new Vec2(), new Vec2(), new Vec2(), new Vec2() };
            Focused = false;
            UpdateLines();
        }

        public void UpdateLines()
        {
            var pos = GetPos();

            Lines[0] = new Vec2(0, 0) + pos;
            Lines[1] = new Vec2(Size.X, 0) + pos;
            Lines[2] = new Vec2(Size.X, Size.Y) + pos;
            Lines[3] = new Vec2(0, Size.Y) + pos;
            Lines[4] = new Vec2(0, 0) + pos;
        }

        private Vec2 GetPos()
        {

            if (TypeOObject is Entity2d)
            {
                return new Vec2(((Entity2d)TypeOObject).ScreenBounds.Pos.X, ((Entity2d)TypeOObject).ScreenBounds.Pos.Y);
            }
            else if(TypeOObject is Drawable2d)
            {
                var pos = new Vec2(Position.X, Position.Y);
                pos.X = pos.X + ((((Drawable2d)TypeOObject).Entity as Entity2d)?.ScreenBounds.Pos.X ?? 0);
                pos.Y = pos.Y + ((((Drawable2d)TypeOObject).Entity as Entity2d)?.ScreenBounds.Pos.Y ?? 0);
                return pos;
            }

            return Position;
        }
    }
}
