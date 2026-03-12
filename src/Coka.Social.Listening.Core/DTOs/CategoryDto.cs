namespace Coka.Social.Listening.Core.DTOs;

/// <summary>
/// Category child item with project count
/// </summary>
public class CategoryChildDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int ProjectCount { get; set; }
}

/// <summary>
/// Category group (parent) containing children and total project count
/// </summary>
public class CategoryGroupDto
{
    public string GroupKey { get; set; } = string.Empty;
    public string GroupLabel { get; set; } = string.Empty;
    public int TotalProjectCount { get; set; }
    public List<CategoryChildDto> Children { get; set; } = new();
}
