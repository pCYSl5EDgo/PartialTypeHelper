using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace PartialTypeHelper.Test;

public class UnitTest1
{
    private readonly ITestOutputHelper testOutputHelper;

    public UnitTest1(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void TemplateTest()
    {
        using var tempWorkarea = TemporaryProjectWorkarea.Create();
        var contents = @"
using System;
using System.Collections.Generic;

namespace System.Runtime.CompilerServices
{
    internal sealed class IsExternalInit : Attribute{}
    public partial record class A(int Q, string P)
    {
        public partial struct B<T0>
        {
            public readonly T0 Value;
            public B(T0 value) => Value = value;

            public partial record struct C<TX, TY> {}
        }
    }
}
            ";
        tempWorkarea.AddFileToTargetProject("Temp.cs", contents);
        var compilation = tempWorkarea.GetOutputCompilation();
        var types = compilation.GetNamedTypeSymbolsFromGenerated();
        var builder = new StringBuilder();
        foreach (var type in types)
        {
            builder.Clear();
            {
                using var ns = new NamespaceTemplate.Util(builder, type.ContainingNamespace);
                using var t = new TypeTemplate.Util(builder, type, ns.Indent);
            }

            var x = builder.ToString();
            ;
        }
    }
}