using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using static Sanchime.TypeGen.SourceGeneration.Extensions.MemberDeclarationFormats;

namespace Sanchime.TypeGen.SourceGeneration.Extensions;

public static class MemberDeclarationFormats
{
    public static class Tokens
    {
        public const string Accessibility = "{accessibility}";
        public const string Scope = "{scope}";
        public const string FieldAccess = "{fieldAccess}";
        public const string Type = "{type}";
        public const string Name = "{name}";
        public const string Accessors = "{accessors}";
    }

    public const string Source = $"{Tokens.Accessibility}{Tokens.Scope}{Tokens.FieldAccess} {Tokens.Type} {Tokens.Name}{Tokens.Accessors}";

    public const string GetSetProp = $"{Tokens.Accessibility} {Tokens.Type} {Tokens.Name} {{ get; set; }}";
    public const string GetProp = $"{Tokens.Accessibility} {Tokens.Type} {Tokens.Name} {{ get; }}";
    public const string SetProp = $"{Tokens.Accessibility} {Tokens.Type} {Tokens.Name} {{ set; }}";
    public const string PublicGetSetProp = $"public {Tokens.Type} {Tokens.Name} {{ get; set; }}";

    public const string Field = $"{Tokens.Accessibility}{Tokens.FieldAccess} {Tokens.Type} {Tokens.Name};";
    public const string PublicField = $"public{Tokens.FieldAccess} {Tokens.Type} {Tokens.Name};";
}

internal static class SourceOutputExtensions
{
    private static string ApplyFormat(string format, string accessibility, string scope, string fieldAccess, string type, string name, string accessors)
    {
        return format
            .Replace(Tokens.Accessibility, accessibility)
            .Replace(Tokens.Scope, scope)
            .Replace(Tokens.FieldAccess, fieldAccess)
            .Replace(Tokens.Type, type)
            .Replace(Tokens.Name, name)
            .Replace(Tokens.Accessors, accessors);
    }

    public static void WriteType(
        this SourceProductionContext context,
        TypeDeclarationSyntax typeDeclarationSyntax,
        IEnumerable<ISymbol?> members,
        string memberDeclarationFormat,
        string outputFileName,
        CancellationToken token = default)
    {

        var sourceBuilder = new SourceBuilder();

        sourceBuilder.AddLine("#nullable enable");
        sourceBuilder.AddLine("#pragma warning disable CS8618");

        var @namespace = typeDeclarationSyntax.GetNamespace(token);

        if (@namespace is not null)
        {
            var fileScoped = @namespace is FileScopedNamespaceDeclarationSyntax;
            sourceBuilder.AddNamespace(@namespace.Name.ToString(), fileScoped);
        }

        sourceBuilder.AddTypeDeclaration(typeDeclarationSyntax.Modifiers, typeDeclarationSyntax.Keyword, typeDeclarationSyntax.Identifier);

        foreach (var member in members)
        {
            if (member is null)
                continue;

            var accessibility = member.DeclaredAccessibility.ToString().ToLower();
            var scope = member.IsStatic ? " static" : string.Empty;

            if (member is IPropertySymbol prop)
            {
                var type = prop.Type.ToDisplayString();
                var name = prop.Name;
                var accessors = " " + prop.GetAccessors();

                sourceBuilder.AddLine(ApplyFormat(memberDeclarationFormat, accessibility, scope, string.Empty, type, name, accessors));
                continue;
            }

            if (member is IFieldSymbol field)
            {
                var type = field.Type.ToDisplayString();
                var name = field.Name;
                var fieldAccess = field.IsReadOnly ? " readonly" : string.Empty;

                sourceBuilder.AddLine(ApplyFormat(memberDeclarationFormat, accessibility, scope, fieldAccess, type, name, ";"));
            }
        }

        var source = sourceBuilder.Build();

        context.AddSource(outputFileName, SourceText.From(source, Encoding.Unicode));
    }
}
