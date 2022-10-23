using GudSafe.Data.Entities;

namespace GudSafe.Data.ViewModels;

public class GalleryViewModel
{
    public string? Username { get; set; }
    public List<GudFile>? Files { get; set; }
}