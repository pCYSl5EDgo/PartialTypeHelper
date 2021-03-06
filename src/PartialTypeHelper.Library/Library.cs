namespace PartialTypeHelper
{
    public readonly struct NamespaceTemplate
    {
        public readonly string? Namespace;
    
        public NamespaceTemplate(global::Microsoft.CodeAnalysis.INamespaceSymbol? symbol)
        {
            Namespace = symbol?.ToDisplayString();
        }
    
        /// <return>Indent space count.</return>
        public int Open(global::System.Text.StringBuilder builder)
        {
            if (Namespace is null)
            {
                return 0;
            }
    
            builder.Append("namespace ");
            builder.AppendLine(Namespace);
            builder.AppendLine("{");
            return 4;
        }
    
        public void Close(global::System.Text.StringBuilder builder)
        {
            builder.AppendLine("}");
        }
    
        public ref struct Scope
        {
            public readonly global::System.Text.StringBuilder Builder;
            public readonly NamespaceTemplate Template;
            public readonly int Indent;
            
            public Scope(global::System.Text.StringBuilder builder, global::Microsoft.CodeAnalysis.INamespaceSymbol? symbol)
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

    public sealed class TypeOpenTemplate
    {
        public readonly global::Microsoft.CodeAnalysis.INamedTypeSymbol Type;
        public readonly TypeOpenTemplate? Child;
        public readonly int ChildDepth;

        public TypeOpenTemplate(global::Microsoft.CodeAnalysis.INamedTypeSymbol type, TypeOpenTemplate? child, int childDepth)
        {
            Type = type;
            Child = child;
            ChildDepth = childDepth;
        }

        public static TypeOpenTemplate Create(global::Microsoft.CodeAnalysis.INamedTypeSymbol symbol)
        {
            var template = new TypeOpenTemplate(symbol, null, 0);
            while (symbol.ContainingType is {} container)
            {
                template = new(container, template, template.ChildDepth + 1);
                symbol = container;
            }

            return template;
        }

        public void TransformAppend(global::System.Text.StringBuilder builder, int indent)
        {
            if (indent > 0) builder.Append(' ', indent);
            builder.Append(@"partial ");
            if (Type.IsValueType && Type.IsRecord)
            {
                builder.Append(@"record struct");
            }
            else if (Type.IsValueType)
            {
                builder.Append("struct");
            }
            else if (Type.IsRecord)
            {
                builder.Append("record");
            }
            else
            {
                builder.Append("class");
            }

            builder.Append(' ');
            builder.Append(Type.Name);
            if (Type.TypeParameters.Length != 0)
            {
                builder.Append('<');
                builder.Append(Type.TypeParameters[0].ToDisplayString());
                for (int i = 1; i < Type.TypeParameters.Length; ++i)
                {
                    builder.Append(", ");
                    builder.Append(Type.TypeParameters[i].ToDisplayString());
                }
                builder.Append('>');
            }
            builder.AppendLine();
            if (indent > 0) builder.Append(' ', indent);
            builder.Append('{');
            builder.AppendLine();
            if (Child is not null)
            {
                Child.TransformAppend(builder, indent + 4);
            }
        }
    }

    public readonly struct TypeTemplate
    {
        public readonly TypeOpenTemplate Template;

        public TypeTemplate(global::Microsoft.CodeAnalysis.INamedTypeSymbol symbol)
        {
            Template = TypeOpenTemplate.Create(symbol);
        }

        public int Open(global::System.Text.StringBuilder builder, int indent)
        {
            Template.TransformAppend(builder, indent);
            return (Template.ChildDepth << 2) + indent + 4;
        }

        public void Close(global::System.Text.StringBuilder builder, int indent)
        {
            var template = Template;
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
            public readonly global::System.Text.StringBuilder Builder;
            public readonly TypeTemplate Template;
            public readonly int Indent;

            public Scope(global::System.Text.StringBuilder builder, global::Microsoft.CodeAnalysis.INamedTypeSymbol symbol, int indent)
            {
                Builder = builder;
                Template = new(symbol);
                Indent = Template.Open(Builder, indent);
            }

            public void Dispose()
            {
                Template.Close(Builder, Indent);
            }
        }
    }
}
