using System.Linq;
using TypeOBasic2dTest.TypeTest;
using TypeOEngine.Typedeaf.Basic2d;
using TypeOEngine.Typedeaf.Basic2d.Engine.Services;
using Xunit;

namespace TypeOBasic2dTest
{
    public class Basic2dModuleTest
    {
        [Fact]
        public void LoadBasic2dModule()
        {
            var typeO = Utility.CreateTypeO();
            typeO.Start();
            var module = typeO.Context.Modules.FirstOrDefault(m => m.GetType() == typeof(Basic2dModule)) as Basic2dModule;
            Assert.NotNull(module);
            Assert.IsType<Basic2dModule>(module);
            Assert.NotEmpty(typeO.Context.Modules);

            Assert.NotEmpty(typeO.Context.Services);

            Assert.NotNull(typeO.Context.Services[typeof(BasicCamera2dService)]);
        }
    }
}
