namespace PartialTypeHelper;

[RuntimeT4Generator.T4(isIndent: true)]
public partial class TypeOpenTemplate
{
    public readonly INamedTypeSymbol Type;
    public readonly TypeOpenTemplate? Child;
    public readonly int ChildDepth;

    public TypeOpenTemplate(INamedTypeSymbol type, TypeOpenTemplate? child, int childDepth)
    {
        Type = type;
        Child = child;
        ChildDepth = childDepth;
    }

    public static TypeOpenTemplate Create(INamedTypeSymbol symbol)
    {
        var template = new TypeOpenTemplate(symbol, null, 0);
        while (symbol.ContainingType is {} container)
        {
            template = new(container, template, template.ChildDepth + 1);
            symbol = container;
        }

        return template;
    }
}
