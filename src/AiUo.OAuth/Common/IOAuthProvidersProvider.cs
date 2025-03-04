using System.Collections.Generic;
using System.Threading.Tasks;

namespace AiUo.OAuth;

public interface IOAuthProvidersProvider
{
    Task<Dictionary<string, OAuthProviderElement>> GetProvidersAsync();
}