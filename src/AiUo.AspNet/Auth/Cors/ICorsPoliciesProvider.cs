namespace AiUo.AspNet;

public interface ICorsPoliciesProvider
{
    List<CorsPolicyElement> GetPolicies();
    void SetAutoRefresh();
}