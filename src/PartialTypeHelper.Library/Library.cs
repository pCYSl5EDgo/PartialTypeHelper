namespace PartialTypeHelper
{
    internal readonly struct NamespaceTemplate
    {
        internal readonly string? Namespace;
    
        internal NamespaceTemplate(global::Microsoft.CodeAnalysis.INamespaceSymbol? symbol)
        {
            Namespace = symbol?.ToDisplayString();
        }
    
        /// <return>Indent space count.</return>
        internal int Open(global::System.Text.StringBuilder builder)
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
    
        internal void Close(global::System.Text.StringBuilder builder)
        {
            builder.AppendLine("}");
        }
    
        internal ref struct Scope
        {
            internal readonly global::System.Text.StringBuilder Builder;
            internal readonly NamespaceTemplate Template;
            internal readonly int Indent;
            
            internal Scope(global::System.Text.StringBuilder builder, global::Microsoft.CodeAnalysis.INamespaceSymbol? symbol)
            {
                Builder = builder;
                Template = new(symbol);
                Indent = Template.Open(Builder);
            }
    
            internal void Dispose()
            {
                Template.Close(Builder);
            }
        }
    }

    internal sealed class TypeOpenTemplate
    {
        internal readonly global::Microsoft.CodeAnalysis.INamedTypeSymbol Type;
        internal readonly TypeOpenTemplate? Child;
        internal readonly int ChildDepth;

        internal TypeOpenTemplate(global::Microsoft.CodeAnalysis.INamedTypeSymbol type, TypeOpenTemplate? child, int childDepth)
        {
            Type = type;
            Child = child;
            ChildDepth = childDepth;
        }

        internal static TypeOpenTemplate Create(global::Microsoft.CodeAnalysis.INamedTypeSymbol symbol)
        {
            var template = new TypeOpenTemplate(symbol, null, 0);
            while (symbol.ContainingType is {} container)
            {
                template = new(container, template, template.ChildDepth + 1);
                symbol = container;
            }

            return template;
        }

        internal void TransformAppend(global::System.Text.StringBuilder builder, int indent)
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

    internal readonly struct TypeTemplate
    {
        private readonly TypeOpenTemplate Template;

        internal TypeTemplate(global::Microsoft.CodeAnalysis.INamedTypeSymbol symbol)
        {
            Template = TypeOpenTemplate.Create(symbol);
        }

        internal int Open(global::System.Text.StringBuilder builder, int indent)
        {
            Template.TransformAppend(builder, indent);
            return (Template.ChildDepth << 2) + indent + 4;
        }

        internal void Close(global::System.Text.StringBuilder builder, int indent)
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

        internal ref struct Scope
        {
            internal readonly global::System.Text.StringBuilder Builder;
            internal readonly TypeTemplate Template;
            internal readonly int Indent;

            internal Scope(global::System.Text.StringBuilder builder, global::Microsoft.CodeAnalysis.INamedTypeSymbol symbol, int indent)
            {
                Builder = builder;
                Indent = indent;
                Template = new(symbol);
                Template.Open(Builder, Indent);
            }

            internal void Dispose()
            {
                Template.Close(Builder, Indent);
            }
        }
    }
}