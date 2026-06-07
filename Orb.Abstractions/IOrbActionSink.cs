namespace Orb.Abstractions;

public interface IOrbActionSink
{
    void Execute(OrbAction action);
}
