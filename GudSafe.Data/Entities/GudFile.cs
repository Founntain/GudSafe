namespace GudSafe.Data.Entities;

public class GudFile : BaseEntity
{
    public string FileType { get; set; }
    public string FileExtension { get; set; }

    public virtual User Creator { get; set; }
    public virtual HashSet<GudCollection> Collections { get; set; } = new();
}