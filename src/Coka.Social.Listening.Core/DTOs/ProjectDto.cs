namespace Coka.Social.Listening.Core.DTOs;

public class ProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Source { get; set; }
    public string? Location { get; set; }
    public string? Investor { get; set; }
    public string? OriginUrl { get; set; }
    public string? Logo { get; set; }
    public string? CategoryKey { get; set; }
    public string? CategoryLabel { get; set; }
    public string? GroupKey { get; set; }
    public string? GroupLabel { get; set; }
    public int Status { get; set; }
    public DateTime? ConfirmationDate { get; set; }
    public Guid? ProvinceId { get; set; }
    public int TotalMentions { get; set; }
    public int ApprovedMentions { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for client project submission (status = 0, pending approval)
/// </summary>
public class SubmitProjectDto
{
    public string Name { get; set; } = string.Empty;
    public string? OriginUrl { get; set; }
    public string? CategoryKey { get; set; }
    public string? Notes { get; set; }
}

public class ProjectFilterDto
{
    public string? Search { get; set; }
    public string? CategoryKey { get; set; }
    public string? GroupKey { get; set; }
    public bool? MissingInvestor { get; set; }
    public bool? MissingConfirmationDate { get; set; }
    public bool IsAdmin { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class BulkUpdateInvestorItemDto
{
    public Guid Id { get; set; }
    public string Investor { get; set; } = string.Empty;
}

public class BulkUpdateConfirmationDateItemDto
{
    public Guid Id { get; set; }
    public DateTime ConfirmationDate { get; set; }
}

public class CreateProjectDto
{
    public string Name { get; set; } = string.Empty;
    public string? Source { get; set; }
    public string? Location { get; set; }
    public string? Investor { get; set; }
    public string? OriginUrl { get; set; }
    public string? Logo { get; set; }
    public string? CategoryKey { get; set; }
    public DateTime? ConfirmationDate { get; set; }
    public Guid? ProvinceId { get; set; }
    public List<CreateArticleDto>? Articles { get; set; }
}

public class BulkCreateProjectResultDto
{
    public List<Guid> ProjectIds { get; set; } = new();
    public int ArticlesInserted { get; set; }
    public int ArticlesSkipped { get; set; }
}

public class UpdateProjectDto
{
    public string? Name { get; set; }
    public string? Source { get; set; }
    public string? Location { get; set; }
    public string? Investor { get; set; }
    public string? OriginUrl { get; set; }
    public string? Logo { get; set; }
    public string? CategoryKey { get; set; }
    public DateTime? ConfirmationDate { get; set; }
    public Guid? ProvinceId { get; set; }
}
