namespace PartialTypeHelper;

public readonly struct NamespaceTemplate
{
    private readonly string? @namespace;

    public NamespaceTemplate(INamespaceSymbol? symbol)
    {
        @namespace = symbol?.ToDisplayString();
    }

    /// <return>Indent space count.</return>
    public int Open(StringBuilder builder)
    {
        if (@namespace is null)
        {
            return 0;
        }

        builder.Append("namespace ");
        builder.AppendLine(@namespace);
        builder.AppendLine("{");
        return 4;
    }

    public void Close(StringBuilder builder)
    {
        builder.AppendLine("}");
    }

    public ref struct Scope
    {
        public readonly StringBuilder Builder;
        public readonly NamespaceTemplate Template;
        public readonly int Indent;
        
        public Scope(StringBuilder builder, INamespaceSymbol? symbol)
        {
            Builder = builder;
            Template = new(symbol);
            Indent = Template.Open(Builder);
        }

        public void Dispose()
        {
            Template.Close(Builder);
        }
    }
}
