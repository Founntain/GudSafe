namespace GudSafe.Data.Models;

public class UserModel : BaseModel
{
    public string Email { get; set; }

    public List<GudFileModel> Files { get; set; }
}