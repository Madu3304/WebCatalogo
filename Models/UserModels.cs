using System.Security.Claims;

namespace WebCatalogo.Models
{
    public class UserModels
    {

        public string? UserName { get; set; }
        public string? Password { get; set; }
        public ClaimsIdentity? UserNome { get; internal set; }
    }
}
