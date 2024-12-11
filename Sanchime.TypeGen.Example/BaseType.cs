namespace Sanchime.TypeGen.Example;

public class BaseType
{
    public required string Name { get; set; }

    public required string Code { get; set; }

    public int Count { get; set; }

    public string? Description { get; set; }

    public ComplexType? ComplexType { get; set; }
}


public class ComplexType
{
    public required string Name { get; set; }
}