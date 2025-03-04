namespace AiUo.AspNet;

public interface IRequestSignFilterProvider
{
    List<RequestSignFilterElement> Build();
}