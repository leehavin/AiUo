namespace AiUo.Extensions.CommandLineParser;

public abstract class VerbCommand<TOptions>: IVerbCommand
{
    public TOptions Options { get; set; }
    public abstract void Execute();

    public void SetOptions(object opts)
    {
        Options = (TOptions)opts;
    }
}
public interface IVerbCommand
{
    void Execute();
    void SetOptions(object opts);
}