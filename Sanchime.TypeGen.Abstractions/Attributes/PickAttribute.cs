using System;
using System.Collections.Generic;
using System.Text;

namespace Sanchime.TypeGen;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class PickAttribute<TBaseOnType>(params string[] includes) : Attribute, ITypeGenAttribute
{
    public string[] Includes { get; } = includes;
}
