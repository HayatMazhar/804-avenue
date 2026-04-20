namespace Avenue804.Web.Domain;

public enum ProjectStage
{
    Consultation,
    Design,
    Approval,
    Procurement,
    Execution,
    Finishing,
    Snagging,
    Handover
}

public class ProjectProgress
{
    public int Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string? ProjectTitle { get; set; }
    public string? Location { get; set; }
    public ProjectStage CurrentStage { get; set; } = ProjectStage.Consultation;
    public string? StageNotes { get; set; }
    public int ProgressPercent { get; set; }       // 0–100
    public DateTimeOffset? EstimatedCompletion { get; set; }
    public string? AccessToken { get; set; }       // unique link token for client
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
