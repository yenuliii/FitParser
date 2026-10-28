namespace FitParser.Core;

public sealed class MessageCollection
{
    private readonly List<NormalizedMessage> _messages = new();

    public IReadOnlyList<NormalizedMessage> Messages => _messages;

    public int Count => _messages.Count;

    public void Add(NormalizedMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        _messages.Add(message);
    }

    public void AddRange(
        IEnumerable<NormalizedMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);

        foreach (NormalizedMessage message in messages)
        {
            Add(message);
        }
    }

    public void Clear()
    {
        _messages.Clear();
    }

}