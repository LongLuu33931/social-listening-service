namespace Coka.Social.Listening.Core.DTOs;

public class ApproveMentionDto
{
    public bool IsApproved { get; set; } = true;
}

public class BulkApproveMentionDto
{
    public bool IsApproved { get; set; } = true;
    public List<Guid>? ArticleIds { get; set; }
    public List<int>? TiktokIds { get; set; }
    public List<int>? YoutubeIds { get; set; }
    public List<int>? FacebookIds { get; set; }
}

public class BulkApproveResultDto
{
    public int ArticlesUpdated { get; set; }
    public int TiktokUpdated { get; set; }
    public int YoutubeUpdated { get; set; }
    public int FacebookUpdated { get; set; }
}
