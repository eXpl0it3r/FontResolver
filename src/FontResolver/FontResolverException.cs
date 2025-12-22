namespace FontResolver;

public class FontResolverException : Exception
{
    public FontResolverException(string message)
        : base(message)
    {
    }

    public FontResolverException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
