using GudSafe.Data.Enums;

namespace GudSafe.Data.Entities
{
    public class GudFile : BaseEntity
    {
        public byte[] FileData { get; set; }
        public byte[] Thumbnail { get; set; }
        public FileType FileType { get; set; }
        public string FileExtension { get; set; }
        
        public virtual User Creator { get; set; }
        public virtual HashSet<GudCollection> Collections { get; set; }
    }
}