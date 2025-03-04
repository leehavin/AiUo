using AiUo.Configuration;

namespace AiUo.AspNet;

public interface IAccessIpFiltersProvider
{
    List<AccessIpFilterElement> Build();
}