using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class UserAddress
{
    public int Id { get; set; }
    public string AddressType { get; set; } = null!;
    public string AddressLine_1 { get; set; } = null!;
    public string? AddressLine_2 { get; set; }

    public string PostalCode { get; set; } = null!;
    public string City { get; set; } = null!;

    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = null!;
    public UserAccount User { get; set; } = null!;
}

