using System;

namespace Reportly;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Salt { get; set; }
    public string Email { get; set; }
    public string Role { get; set; } // "Admin", "Support", "Client"

    public bool TwoFactorEnabled { get; set; }

    public string TwoFactorSecret { get; set; }

    public string PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
    public string CreatedBy
    {
        get; set;
    }

    public string UpdateBy { get; set; }
}
