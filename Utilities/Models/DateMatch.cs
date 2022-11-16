namespace Utilities.Models
{
    public class DateMatch
    {
        public DateRange? DateRange { get; set; }
        public DateTime? Date { get; set; }
        public MatchType Type { get; set; }
    }

    public class DateRange
    {
        public RelativeDateRange? RelativeDateRange { get; set; }
        public AbsoluteDateRange? AbsoluteDateRange { get; set; }
        public RangeType Type { get; set; }
    }

    public class RelativeDateRange
    {
        public int? StartDate { get; set; }
        public int? EndDate { get; set; }
        public string? TimeZone { get; set; }
    }

    public class AbsoluteDateRange
    {
        public DateTime? OnOrAfter { get; set; }
        public DateTime? Before { get; set; }
        public bool? IncludeTime { get; set; }
    }

    public enum RangeType
    {
        Relative,
        Absolute
    }

    public enum MatchType
    {
        Exact,
        Range
    }
}
