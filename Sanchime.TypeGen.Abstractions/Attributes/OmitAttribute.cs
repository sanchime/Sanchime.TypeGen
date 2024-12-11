using System;

namespace Sanchime.TypeGen;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class OmitAttribute<TBaseOnType>(params string[] excludes) : Attribute, ITypeGenAttribute
{
    public string[] Excludes { get; } = excludes;
}
