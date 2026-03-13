namespace Coka.Social.Listening.Core.DTOs;

public class ArticleDto
{
    public Guid Id { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? SourceName { get; set; }
    public string? SourceUrl { get; set; }
    public DateTime? PubDate { get; set; }
    public string? Snippet { get; set; }
    public string? ImageUrl { get; set; }
    public string? Sentiment { get; set; }
    public bool IsRelevant { get; set; }
    public int Status { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ArticleFilterDto
{
    public string? Search { get; set; }
    public Guid? ProjectId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Sentiment { get; set; }
    public bool? IsRelevant { get; set; }
    public string? UrlContains { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class CreateArticleDto
{
    public Guid? ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? SourceName { get; set; }
    public string? SourceUrl { get; set; }
    public DateTime? PubDate { get; set; }
    public string? Snippet { get; set; }
    public string? ImageUrl { get; set; }
    public string? Sentiment { get; set; }
    public bool IsRelevant { get; set; } = true;
}

public class BulkCreateArticleResultDto
{
    public int Inserted { get; set; }
    public int Skipped { get; set; }
}

public class UpdateArticleDto
{
    public Guid? ProjectId { get; set; }
    public string? Title { get; set; }
    public string? SourceName { get; set; }
    public string? SourceUrl { get; set; }
    public DateTime? PubDate { get; set; }
    public string? Snippet { get; set; }
    public string? ImageUrl { get; set; }
    public string? Sentiment { get; set; }
    public bool? IsRelevant { get; set; }
    public string? Url { get; set; }
}

public class BulkUpdateArticleUrlItemDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
}
