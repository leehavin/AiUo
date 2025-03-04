namespace AiUo.AspNet;

public class EndRequestContentException : CustomException
{
    public EndRequestContentException(string message) : base(message)
    {
    }
}