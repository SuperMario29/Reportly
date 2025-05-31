using Microsoft.AspNetCore.Authorization;

public class AuthorizeRoleAttribute : AuthorizeAttribute
{
    public AuthorizeRoleAttribute(params string[] roles)
    {
        Roles = string.Join(",", roles);
        Policy = $"Role:{Roles}";
    }
}