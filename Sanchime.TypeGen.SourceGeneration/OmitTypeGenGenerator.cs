using Microsoft.CodeAnalysis;
using Sanchime.TypeGen.SourceGeneration.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Sanchime.TypeGen.SourceGeneration;

[Generator]
internal partial class OmitTypeGenGenerator : TypeGenGenerator
{
    internal override Type AttributeType => typeof(OmitAttribute<>);

    internal override IEnumerable<ISymbol> GetMembers(ITypeSymbol type, IEnumerable<string> members)
    {
        return type.GetExcludeProperties(members);
    }

    internal override string Named(INamedTypeSymbol typeSymbol, ITypeSymbol baseTypeSymbol, IEnumerable<string> members)
    {
        return $"{typeSymbol.ToDisplayString()}_Omit{baseTypeSymbol.Name}_{String.Join("_", members)}.g.cs";
    }


    internal override string CreateSource(INamedTypeSymbol typeSymbol, IEnumerable<ISymbol> members)
    {
        throw new NotImplementedException();
    }
}


