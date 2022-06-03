namespace API.Entities;

public class AppUser : IdentityUser<int>
{
    public string Pseudo { get; set; }
    public byte[] PasswordSalt { get; set; }
    public string Gender { get; set; }
    public string Introduction { get; set; }
    public string LookingFor { get; set; }
    public string Interests { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastActive { get; set; }
    public ICollection<Photo> Photos { get; set; }
    public ICollection<UserLike> LikedByUsers { get; set; }
    public ICollection<UserLike> LikedUser { get;set; }
    public ICollection<Message> MessagesSent { get;set; }
    public ICollection<Message> MessagesReceived { get;set; }
    public ICollection<AppUserRole> UserRoles { get; set; }

    // public int GetAge() {
    //     return DateOfBirth.CalculateAge();
    // }
}
