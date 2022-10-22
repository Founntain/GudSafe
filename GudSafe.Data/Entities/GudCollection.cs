namespace GudSafe.Data.Entities;

public class GudCollection : BaseEntity
{
    public virtual User Creator { get; set; }
    public virtual HashSet<GudFile> Files { get; set; } = new();
}