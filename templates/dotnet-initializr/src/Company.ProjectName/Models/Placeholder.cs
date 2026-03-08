namespace Company.ProjectName.Models;

#if (IncludeAnyOrm)
public class SampleEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
#else
// Add your models here.
#endif
