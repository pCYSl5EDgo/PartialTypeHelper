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

    public ref struct Util
    {
        private readonly StringBuilder builder;
        private readonly TypeTemplate parent;
        private readonly int indent;

        public Util(StringBuilder builder, INamedTypeSymbol symbol, int indent)
        {
            this.builder = builder;
            this.indent = indent;
            parent = new(symbol);
            parent.Open(builder, indent);
        }

        public void Dispose()
        {
            parent.Close(builder, indent);
        }
    }
}
