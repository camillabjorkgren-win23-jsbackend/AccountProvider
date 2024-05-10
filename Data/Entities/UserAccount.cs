using Microsoft.AspNetCore.Identity;

namespace Data.Entities;
public class UserAccount : IdentityUser
{
    [ProtectedPersonalData]
    public string FirstName { get; set; } = null!;

    [ProtectedPersonalData]
    public string LastName { get; set; } = null!;

    public string? ProfileImg { get; set; } = "avatar.jpg";

    //Behöver inte fylla i vid create
    public string? Bio { get; set; }
    public DateTime? Created { get; set; }
    public DateTime? Modified { get; set; }

    public int? AddressId { get; set; }
    public UserAddress? Address { get; set; }
}
