namespace GudSafe.Data.Models.EntityModels;

public class UserModel : BaseModel
{
    public string Email { get; set; }

    public List<GudFileModel> Files { get; set; }

    public List<GudCollectionModel> Collections { get; set; }
}