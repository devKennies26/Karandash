namespace Karandash.Shared.Utils.Infrastructure;

public class OutboxEvent
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }

    public string Type { get; set; }
    public string Payload { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public int RetryCount { get; set; }
}