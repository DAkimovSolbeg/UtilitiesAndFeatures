namespace Utilities.Models
{
    public class StringMatch
    {
        public StringMatchType Type { get; set; }
        public string Value { get; set; } = string.Empty;
    }

    public enum StringMatchType
    {
        Exact,
        StartsWith,
        Contains
    }
}
