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

    public ref struct Util
    {
        private readonly StringBuilder builder;
        private readonly NamespaceTemplate parent;

        public readonly int Indent;
        
        public Util(StringBuilder builder, INamespaceSymbol? symbol)
        {
            parent = new(symbol);
            this.builder = builder;
            Indent = parent.Open(builder);
        }

        public void Dispose()
        {
            parent.Close(builder);
        }
    }
}
