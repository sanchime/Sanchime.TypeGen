using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanchime.TypeGen.SourceGeneration.Extensions;

internal static class TypeSymbolExtensions
{
    internal static IEnumerable<IPropertySymbol> GetExcludeProperties(this ITypeSymbol typeSymbol, IEnumerable<string> excludes)
    {
        return typeSymbol.GetProperties(p => excludes.Contains(p.Name));
    }

    internal static IEnumerable<IPropertySymbol> GetIncludeProperties(this ITypeSymbol typeSymbol, IEnumerable<string> includes)
    {
        return typeSymbol.GetProperties(p => !includes.Contains(p.Name));
    }

    internal static IEnumerable<IPropertySymbol> GetProperties(this ITypeSymbol typeSymbol, Predicate<IPropertySymbol> predicate)
    {
        var symbol = typeSymbol;
        do
        {
            foreach (var member in symbol.GetMembers())
            {
                if (member is not IPropertySymbol { Name: not "EqualityContract" } property)
                    continue;

                if (predicate(property))
                {
                    continue;
                }

                yield return property;
            }

            symbol = symbol.BaseType;
        } while (symbol is not null);
    }

}
