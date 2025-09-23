namespace Backend.Models.Domains;

public class AuditableEntity
{
    public DateTime CreatedOn { get; set; }

    public long? CreatedByUserId { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public long? UpdatedByUserId { get; set; }

    public User? CreatedByUser { get; set; }
    public User? UpdatedByUser { get; set; }
}