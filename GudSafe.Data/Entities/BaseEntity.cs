namespace GudSafe.Data.Entities
{
    public class BaseEntity
    {
        public ulong ID { get; set; }
        public Guid UniqueId { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public DateTimeOffset CreationTime { get; set; } = DateTimeOffset.UtcNow;
    }
}