using GudSafe.Data.Entities;

namespace GudSafe.Data.Models;

public class GudCollectionModel : BaseModel
{
    public User Creator { get; set; }
    public List<GudFileModel> Files { get; set; }
}