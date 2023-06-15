namespace Shared.Contracts;

public class QueryMatchesRequest
{
    public string? MatchType { get; set; }
    public int PageNumber { get; set; } = 1;

    public (bool, string) IsValid()
    {
        if (string.IsNullOrWhiteSpace(MatchType))
        {
            return (true, string.Empty);
        }

        if (_validMatchTypes.Contains(MatchType))
        {
            return PageNumber < 1 ? (false, $"{nameof(PageNumber)} must be greater than 0") : (true, string.Empty);
        }
        
        var validMatchTypesString = string.Join(", ", _validMatchTypes);

        return (false, $"Invalid {nameof(MatchType)}, possible values are: {validMatchTypesString}");

    }
    
    private readonly string[] _validMatchTypes = { "t20", "odi", "test", "county" };
}