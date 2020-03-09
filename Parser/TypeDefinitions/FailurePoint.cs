namespace Parser.TypeDefinitions
{
    public enum FailurePoint : byte
    {
        IllFormedStream = 0,
        UnresolvedTag = 1,
        UnrecognizedTag = 2,
        UnavailableTag = 3,
    }
}