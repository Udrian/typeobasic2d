using System.Reflection;
using TypeOEngine.Typedeaf.Core.Attributes;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities;
using TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Drawables;

namespace TypeOBasic2dTest;

public class PropertyGroupTests
{
    [Theory]
    [InlineData(typeof(Entity2d), "Position", "Transform")]
    [InlineData(typeof(Entity2d), "Scale", "Transform")]
    [InlineData(typeof(Entity2d), "Rotation", "Transform")]
    [InlineData(typeof(Entity2d), "Size", "Transform")]
    [InlineData(typeof(Entity2d), "Origin", "Transform")]
    [InlineData(typeof(Drawable2d), "Position", "Transform")]
    [InlineData(typeof(Drawable2d), "Scale", "Transform")]
    [InlineData(typeof(Drawable2d), "Rotation", "Transform")]
    [InlineData(typeof(Drawable2d), "Size", "Transform")]
    [InlineData(typeof(Drawable2d), "Origin", "Transform")]
    [InlineData(typeof(DrawableTexture), "Size", "Transform")]
    [InlineData(typeof(DrawableTexture), "Texture", "Rendering")]
    [InlineData(typeof(DrawableTexture), "Color", "Rendering")]
    [InlineData(typeof(DrawableTexture), "Flipped", "Rendering")]
    [InlineData(typeof(DrawableTexture), "SizeMode", "Rendering")]
    public void BuiltInProperties_PreserveGroupsIncludingInheritedAttributes(Type type, string name, string group)
    {
        var attribute = type.GetProperty(name).GetCustomAttribute<TypeOPropertyAttribute>(true);

        Assert.NotNull(attribute);
        Assert.Equal(group, attribute.Group);
    }
}
