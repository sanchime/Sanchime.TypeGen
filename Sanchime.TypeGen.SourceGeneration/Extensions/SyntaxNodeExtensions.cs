using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics.CodeAnalysis;

namespace Sanchime.TypeGen.SourceGeneration.Extensions;

internal static class SyntaxNodeExtensions
{
    public static bool TryCompileNamedTypeSymbol(this SyntaxNode syntax, Compilation compilation, CancellationToken token, [NotNullWhen(true)] out INamedTypeSymbol? result)
    {
        result = null;
        var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);

        var decleredSymbol = semanticModel.GetDeclaredSymbol(syntax, token);
        if (decleredSymbol is not INamedTypeSymbol namedTypeSymbol)
            return false;

        result = namedTypeSymbol;
        return true;
    }

    public static ISymbol? GetSymbol(this SyntaxNode syntaxNode, Compilation compilation, CancellationToken token)
    {
        // TODO: include candidate symbols?
        return compilation.GetSemanticModel(syntaxNode.SyntaxTree).GetSymbolInfo(syntaxNode, token).Symbol;
    }

    public static BaseNamespaceDeclarationSyntax? GetNamespace(this SyntaxNode node, CancellationToken token = default)
    {
        var currNode = node;
        while (currNode is not BaseNamespaceDeclarationSyntax)
        {
            token.ThrowIfCancellationRequested();

            currNode = currNode.Parent;
            if (currNode is null)
                return null;
        }
        var namespaceDeclaration = (BaseNamespaceDeclarationSyntax)currNode;
        return namespaceDeclaration;
    }

    public static string GetFullName(this TypeDeclarationSyntax typeNode, CancellationToken token = default)
    {
        var @namespace = typeNode.GetNamespace(token)?.Name.ToString() ?? string.Empty;
        var typeName = typeNode.Identifier.ToString();
        return string.IsNullOrEmpty(@namespace) ? typeName : $"{@namespace}.{typeName}";
    }

    public static bool TryFindParent<T>(this SyntaxNode node, [NotNullWhen(true)] out T? found, CancellationToken token = default)
        where T : SyntaxNode
    {
        var current = node;
        while (current is not null)
        {
            token.ThrowIfCancellationRequested();

            if (current is T resolved)
            {
                found = resolved;
                return true;
            }

            current = current.Parent;
        }

        found = default;
        return false;
    }
}
