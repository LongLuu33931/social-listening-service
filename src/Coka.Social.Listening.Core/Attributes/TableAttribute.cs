namespace Coka.Social.Listening.Core.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TableAttribute : Attribute
{
    public string Name { get; }

    public TableAttribute(string name)
    {
        Name = name;
    }
}

[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public class ColumnAttribute : Attribute
{
    public string Name { get; }

    public ColumnAttribute(string name)
    {
        Name = name;
    }
}

[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public class PrimaryKeyAttribute : Attribute
{
}
