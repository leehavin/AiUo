namespace AiUo.AspNet;

public interface IClientSignFiltersProvider
{
    List<ClientSignFilterElement> Build();
}