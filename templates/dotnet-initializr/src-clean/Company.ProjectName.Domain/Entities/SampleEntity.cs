namespace Company.ProjectName.Domain.Entities;

public class SampleEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
