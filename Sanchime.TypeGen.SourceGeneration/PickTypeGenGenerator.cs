using Microsoft.CodeAnalysis;
using Sanchime.TypeGen.SourceGeneration.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Sanchime.TypeGen.SourceGeneration;

[Generator]
internal partial class PickTypeGenGenerator : TypeGenGenerator
{
    internal override Type AttributeType => typeof(PickAttribute<>);

    internal override string CreateSource(INamedTypeSymbol typeSymbol, IEnumerable<ISymbol> members)
    {
        throw new NotImplementedException();
    }

    internal override IEnumerable<ISymbol> GetMembers(ITypeSymbol type, IEnumerable<string> members)
    {
        return type.GetIncludeProperties(members);
    }

    internal override string Named(INamedTypeSymbol typeSymbol, ITypeSymbol baseTypeSymbol, IEnumerable<string> members)
    {
        return $"{typeSymbol.ToDisplayString()}_Pick{baseTypeSymbol.Name}_{String.Join("_", members)}.g.cs";
    }
}


