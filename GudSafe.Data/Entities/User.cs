namespace GudSafe.Data.Entities
{
    public class User : BaseEntity
    {
        public string Password { get; set; }
        public string Salt { get; set; }
        public string Email { get; set; }
        public string ApiKey { get; set; }

        public virtual HashSet<GudFile> FilesUploaded { get; set; } = new ();
    }
}