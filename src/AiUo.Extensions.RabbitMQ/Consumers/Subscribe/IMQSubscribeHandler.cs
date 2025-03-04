namespace AiUo.Extensions.RabbitMQ;

public interface IMQSubscribeHandler<TMessage>
    where TMessage : class, new()
{
    Task OnMessage(TMessage message, CancellationToken cancellationToken);
}