using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace Sanchime.TypeGen.SourceGeneration.Extensions;

internal static class IncrementalValuesProviderExtensions
{
    public static IncrementalValuesProvider<AttributeSyntax> CreateAttributeSyntaxProvider<T>(this SyntaxValueProvider syntaxProvider)
        where T : Attribute
    {
        var attributeType = typeof(T);
        return syntaxProvider.CreateAttributeSyntaxProvider(attributeType);
    }

    public static IncrementalValuesProvider<AttributeSyntax> CreateAttributeSyntaxProvider(this SyntaxValueProvider syntaxProvider, Type attributeType)
    {
        var attributeNameRegex = new Regex($"^({attributeType.Namespace})?{attributeType.GetShortAttributeName()}(Attribute)??<[^>]*>$");

        return syntaxProvider.CreateSyntaxProvider(
            predicate: (node, _) =>
            {
                if (node is not AttributeSyntax attr)
                {
                    return false;
                }
                return attributeNameRegex.IsMatch(attr.Name.ToString());
            },
            transform: (ctx, ct) =>
            {
                try
                {
                    var attributeSyntax = (AttributeSyntax)ctx.Node;
                    var attributeSymbolInfo = ctx.SemanticModel.GetSymbolInfo(attributeSyntax, ct);

                    var symbol = attributeSymbolInfo.Symbol ?? attributeSymbolInfo.CandidateSymbols.FirstOrDefault();

                    if (symbol is not IMethodSymbol attributeSymbol)
                        return null;

                    var attributeName = attributeSymbol.ContainingType.MetadataName;

                    if (attributeName != attributeType.Name)
                        return null;

                    return attributeSyntax;
                }
                catch
                {
                    // TODO: diagnostics?
                    return null;
                }

            }).WhereNotNull();
    }

    public static IncrementalValuesProvider<T> WhereNotNull<T>(this IncrementalValuesProvider<T?> provider)
    {
        return provider.Where(x => x is not null)!;
    }
}