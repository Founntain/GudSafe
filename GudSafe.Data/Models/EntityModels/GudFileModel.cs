using GudSafe.Data.Entities;
using GudSafe.Data.Enums;

namespace GudSafe.Data.Models;

public class GudFileModel : BaseModel
{
    public byte[] FileData { get; set; }
    public byte[] Thumbnail { get; set; }
    public FileType FileType { get; set; }
    public string FileExtension { get; set; }

    public UserModel Creator { get; set; }
    public List<GudCollectionModel> Collections { get; set; }
}