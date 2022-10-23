using GudSafe.Data.Entities;

namespace GudSafe.Data.Models.EntityModels;

public class GudCollectionModel : BaseModel
{
    public User Creator { get; set; }
    public List<GudFileModel> Files { get; set; }
}