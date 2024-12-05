
using WebCatalogo.Models;

namespace WebCatalogo.Services
{
    //token ficará aqui 
    public interface ITokenServices
    {
        string GerarToken(string key, string issuer, string audience, UserModels user);
    }
}
