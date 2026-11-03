using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Contents;

namespace TypeOBasic2dTest.TypeTest.Mock
{
    internal class StubTexture : Texture
    {
        public StubTexture(Vec2i size) { Size = size; }

        protected override void Create(Vec2i size, byte[] data) { }
        protected override void Load(string path) { }
        protected override void Cleanup() { }
        public override void Save(string path) { }
        public override Color PixelAt(int x, int y) => Color.White;
        public override void FlipVertical() { }
        public override void FlipHorizontal() { }
    }

    // Every character measures 10x20, which makes expected sizes easy to compute.
    internal class StubFont : Font
    {
        protected override void Load(string path) { }
        protected override void Cleanup() { }
        public override Vec2 MeasureString(string text) => new Vec2(text.Length * 10, 20);
    }
}
