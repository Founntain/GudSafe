namespace GudSafe.Data.Models
{
    public class BaseModel
    {
        public ulong Id { get; set; }
        public DateTimeOffset CreationTime { get; set; }
        public string Name { get; set; }
    }
}