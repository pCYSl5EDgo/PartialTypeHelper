namespace PartialTypeHelper;

public struct TypeTemplate
{
    private readonly TypeOpenTemplate OpenTemplate;

    public TypeTemplate(INamedTypeSymbol symbol)
    {
        OpenTemplate = TypeOpenTemplate.Create(symbol);
    }

    public int Open(StringBuilder builder, int indent)
    {
        OpenTemplate.TransformAppend(builder, indent);
        return (OpenTemplate.ChildDepth << 2) + indent;
    }

    public void Close(StringBuilder builder, int indent)
    {
        var template = OpenTemplate;
        do
        {
            builder.Append(' ', (template.ChildDepth << 2) + indent);
            builder.AppendLine("}");
            template = template.Child;
        }
        while (template is not null);
    }

    public ref struct Scope
    {
        public readonly StringBuilder Builder;
        public readonly TypeTemplate Template;
        public readonly int Indent;

        public Scope(StringBuilder builder, INamedTypeSymbol symbol, int indent)
        {
            Builder = builder;
            Indent = indent;
            Template = new(symbol);
            Template.Open(Builder, Indent);
        }

        public void Dispose()
        {
            Template.Close(Builder, Indent);
        }
    }
}
