namespace GudSafe.Data.Models.EntityModels;

public class BaseModel
{
    public ulong Id { get; set; }
    public DateTimeOffset CreationTime { get; set; }
    public string Name { get; set; }
}