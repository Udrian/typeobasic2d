using TypeD.Helpers;
using TypeD.Models.Data;
using TypeDBasic2d.Components;
using TypeDCore.Code.Entity;
using TypeDCore.Components;

namespace TypeOBasic2dTest;

/// <summary>
/// Pins down the root engine type of the 2D component definitions and how it is used for filtering and code generation.
/// </summary>
public class ComponentBaseTypeTests
{
    private const string EntityName = "TypeOEngine.Typedeaf.Core.Entities.Entity";
    private const string DrawableName = "TypeOEngine.Typedeaf.Core.Entities.Drawables.Drawable";

    public static IEnumerable<object[]> Definitions()
    {
        yield return new object[] { "Entity2d", (Func<Component>)Basic2dComponent.Entity2dComponent, EntityName };
        yield return new object[] { "Drawable2d", (Func<Component>)Basic2dComponent.Drawable2dComponent, DrawableName };
        yield return new object[] { "DrawableTexture", (Func<Component>)Basic2dComponent.DrawableTextureComponent, DrawableName };
    }

    [Theory]
    [MemberData(nameof(Definitions))]
    public void Definition_HasExpectedRootType(string _, Func<Component> create, string expected)
    {
        Assert.Equal(expected, create().RootBaseComponent.FullName);
    }

    [Fact]
    public void DerivedComponent_HasRootTypeOfItsBase()
    {
        var player = Derive("Player", Basic2dComponent.Entity2dComponent());
        var boss = Derive("Boss", player);

        Assert.Equal(EntityName, player.RootBaseComponent.FullName);
        Assert.Equal(EntityName, boss.RootBaseComponent.FullName);
    }

    [Theory]
    [InlineData("Entity2d", false, true)]
    [InlineData("Drawable2d", false, true)]
    public void ChildFilter_AcceptsExpectedRootTypes(string name, bool acceptsEntity, bool acceptsDrawable)
    {
        var create = Definitions().First(d => (string)d[0] == name)[1] as Func<Component>;
        var filter = new FilterHelper();
        create!().Template.ChildrenFilter(filter);

        Assert.Equal(acceptsEntity, filter.Filters.Contains($"{EntityName};"));
        Assert.Equal(acceptsDrawable, filter.Filters.Contains($"{DrawableName};"));
    }

    [Fact]
    public void ChildFilter_DecidesByRootTypeOfDerivedChildren()
    {
        var filter = new FilterHelper();
        Basic2dComponent.Entity2dComponent().Template.ChildrenFilter(filter);

        var derivedDrawable = Derive("Sprite", Basic2dComponent.DrawableTextureComponent());
        var derivedScene = Derive("Level", CoreComponent.SceneComponent());

        Assert.Contains($"{derivedDrawable.RootBaseComponent.FullName};", filter.Filters);
        Assert.DoesNotContain($"{derivedScene.RootBaseComponent.FullName};", filter.Filters);
    }

    [Fact]
    public void EntityCode_ForEntity2dChild_InheritsFromBaseComponent()
    {
        var player = Derive("Player", Basic2dComponent.Entity2dComponent());
        var code = new EntityCode(player);

        Assert.Equal("TypeOEngine.Typedeaf.TypeOBasic2d.Entities.Entity2d", code.BaseClass);
        Assert.False(code.IsBaseComponentType);
    }

    [Fact]
    public void PublicChildren_ArePickedUpFor2dEntitiesAndDrawables()
    {
        var parent = Derive("Level", CoreComponent.EntityComponent());
        var enemy = Child("Enemy", Basic2dComponent.Entity2dComponent(), true);
        var enemySprite = Child("EnemySprite", Basic2dComponent.DrawableTextureComponent(), true);
        var shadow = Child("Shadow", Basic2dComponent.Drawable2dComponent(), false);
        enemy.Children.Add(enemySprite);
        parent.Children.Add(enemy);
        parent.Children.Add(shadow);

        var method = typeof(TypeD.Code.ComponentTypeCode).GetMethod("GetPublicChildNames", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var names = (Dictionary<Component, string>)method.Invoke(new EntityCode(parent), null)!;

        Assert.Equal(new[] { "Enemy", "EnemySprite" }, names.Values.OrderBy(n => n).ToArray());
    }

    private static Component Derive(string name, Component baseComponent) => new()
    {
        ClassName = name,
        Namespace = "Game",
        BaseInheritedComponent = baseComponent
    };

    private static Component Child(string name, Component baseComponent, bool isPublic)
    {
        var child = Derive(name, baseComponent);
        child.Public = isPublic;
        return child;
    }
}
