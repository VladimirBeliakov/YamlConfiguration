namespace Processor.TypeDefinitions
{
    public enum FailurePoint : byte
    {
        IllFormedStream = 1,
        UnresolvedTag = 2,
        UnrecognizedTag = 3,
        UnavailableTag = 4,
    }
}