using AutoMapper;
using Humanizer;
using System.Globalization;
using Utilities.Models;
using api = Utilities.Protos;
using MatchType = Utilities.Models.MatchType;

namespace Utilities.MappingProfiles
{
    internal class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<api.DateMatch, DateMatch?>()
                .ConvertUsing((apiDateMatch, dateMatch) =>
                {
                    if (apiDateMatch == null)
                    {
                        return null;
                    }

                    if (apiDateMatch.MatchTypeCase == api.DateMatch.MatchTypeOneofCase.None)
                    {
                        throw new NoMatchFoundException("Unable to map DateMatch with MatchTypeOneOfCase.None");
                    }

                    if (apiDateMatch.MatchTypeCase == api.DateMatch.MatchTypeOneofCase.Range
                    && apiDateMatch.Range.RangeTypeCase == api.DateMatch.Types.DateRange.RangeTypeOneofCase.None)
                    {
                        throw new NoMatchFoundException("Unable to map DateMatch with RangeTypeOneOfCase.None");
                    }

                    if (apiDateMatch.MatchTypeCase == api.DateMatch.MatchTypeOneofCase.Exact)
                    {
                        return new DateMatch
                        {
                            Type = MatchType.Exact,
                            Date = DateTime.SpecifyKind(
                            DateTime.ParseExact(apiDateMatch.Exact, Constants.DateMatchFormat, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal),
                            DateTimeKind.Utc),
                            DateRange = default
                        };
                    }

                    var dateFormat = apiDateMatch.Range.Absolute?.IncludeTime != null && apiDateMatch.Range.Absolute?.IncludeTime == true
                    ? Constants.DateTimeMatchFormat
                    : Constants.DateMatchFormat;

                    return new DateMatch
                    {
                        Type = MatchType.Range,
                        Date = null,
                        DateRange = new DateRange
                        {
                            Type = apiDateMatch.Range.RangeTypeCase == api.DateMatch.Types.DateRange.RangeTypeOneofCase.Absolute
                            ? RangeType.Absolute
                            : RangeType.Relative,
                            AbsoluteDateRange =
                            new AbsoluteDateRange
                            {
                                IncludeTime = apiDateMatch.Range.Absolute?.IncludeTime,
                                OnOrAfter = string.IsNullOrWhiteSpace(apiDateMatch.Range.Absolute?.OnOrAfter)
                                    ? null
                                    : DateTime.SpecifyKind(
                                        DateTime.ParseExact(apiDateMatch.Range.Absolute.OnOrAfter, dateFormat, CultureInfo.InvariantCulture),
                                        DateTimeKind.Utc),
                                Before = string.IsNullOrWhiteSpace(apiDateMatch.Range.Absolute?.Before)
                                    ? null
                                    : DateTime.SpecifyKind(
                                        DateTime.ParseExact(apiDateMatch.Range.Absolute.Before, dateFormat, CultureInfo.InvariantCulture),
                                        DateTimeKind.Utc)
                            },
                            RelativeDateRange =
                                new RelativeDateRange
                                {
                                    StartDate = apiDateMatch.Range.Relative?.StartDate,
                                    EndDate = apiDateMatch.Range.Relative?.EndDate,
                                    TimeZone = apiDateMatch.Range.Relative?.Timezone
                                }
                        }
                    };
                });
        }
    }
}
