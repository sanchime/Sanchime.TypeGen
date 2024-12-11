using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sanchime.TypeGen.SourceGeneration.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Sanchime.TypeGen.SourceGeneration.Extensions.MemberDeclarationFormats;

namespace Sanchime.TypeGen.SourceGeneration;

public abstract class TypeGenGenerator : IIncrementalGenerator
{
    internal abstract Type AttributeType { get; }

    internal abstract IEnumerable<ISymbol> GetMembers(ITypeSymbol type, IEnumerable<string> members);

    internal virtual void Generate(SourceProductionContext context, TypeDeclarationSyntax targetTypeSyntax, INamedTypeSymbol typeSymbol, ImmutableArray<AttributeData> attributeDatas, CancellationToken token)
    {
        foreach (var attribute in attributeDatas)
        {
            if (attribute.AttributeClass is not { TypeArguments: [var baseOnType, ..] })
                continue;

            if (attribute.ConstructorArguments is not [{ Kind: TypedConstantKind.Array, Type: IArrayTypeSymbol }, ..] args)
                continue;

            var members = args.SelectMany(x => x.Values)
                .Select(x => x.Value)
                .Cast<string>()
                .Where(x => !String.IsNullOrWhiteSpace(x))
                .ToImmutableHashSet();

            var baseTypeMembers = GetMembers(baseOnType, members);

            context.WriteType(
                typeDeclarationSyntax: targetTypeSyntax,
                                members: baseTypeMembers,
                memberDeclarationFormat: MemberDeclarationFormats.Source,
                        outputFileName: Named(typeSymbol, baseOnType, members),
                                token: token);
        }
    }

    internal abstract string CreateSource(INamedTypeSymbol typeSymbol, IEnumerable<ISymbol> members);


    internal abstract string Named(INamedTypeSymbol typeSymbol, ITypeSymbol baseTypeSymbol, IEnumerable<string> members);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
/*#if DEBUG
        System.Diagnostics.Debugger.Launch();
#endif*/

        var typesDict = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => node is TypeDeclarationSyntax,
                    transform: static (ctx, _) => (TypeDeclarationSyntax)ctx.Node)
                .Collect()
                .Select((types, ct) => types.ToDictionary(x => x.GetFullName(ct)));

        var generatorAttributes = context.SyntaxProvider
           .CreateAttributeSyntaxProvider(AttributeType)
           .Combine(typesDict)
           .Combine(context.CompilationProvider);


        context.RegisterSourceOutput(generatorAttributes, (spc, tuple) =>
        {
            var token = spc.CancellationToken;
            var attributeSyntax = tuple.Left.Left!;
            var types = tuple.Left.Right!;
            var compilation = tuple.Right!;

            if (!attributeSyntax.TryFindParent<TypeDeclarationSyntax>(out var targetTypeSyntax, token))
                return;

            if (!targetTypeSyntax.Modifiers.Any(m => m.ValueText == "partial"))
            {
                spc.ReportMissingPartialModifier(targetTypeSyntax);
                return;
            }

            if (!targetTypeSyntax.TryCompileNamedTypeSymbol(compilation, token, out var targetTypeSymbol))
                return;


            var attributeDatas = targetTypeSymbol.GetAttributes();

            Generate(spc, targetTypeSyntax, targetTypeSymbol, attributeDatas, token);
        });
    }

}

