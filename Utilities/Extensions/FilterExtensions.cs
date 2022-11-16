using System.Linq.Expressions;
using Utilities.Models;
using Utilities.Interfaces;
using api = Utilities.Protos;
using Utilities.Helpers;

namespace Utilities.Extensions
{
    public static class FilterExtensions
    {
        public static IQueryable<T> AllCommonFilters<T>(this IQueryable<T> query, BaseFilter filter)
            where T : IEntity
        {
            return query.FilterById(filter);
        }

        public static IQueryable<T> FilterById<T>(this IQueryable<T> query, BaseFilter filter)
            where T : IEntity
        {
            if (filter.Ids.Any())
            {
                query = query.Where(x => filter.Ids.Contains(x.Id));
            }

            return query;
        }

        public static IQueryable<T> StringMatch<T>(
            this IQueryable<T> query,
            Expression<Func<T, string>> column,
            api.StringMatch? stringMatch
        )
        {
            if (stringMatch == null)
            {
                return query;
            }

            Expression<Func<string, bool>> whereClause;

            switch (stringMatch.MatchTypeCase)
            {
                case api.StringMatch.MatchTypeOneofCase.Exact:
                    {
                        whereClause =
                            fieldValue => fieldValue.ToLower() == stringMatch.Exact.ToLower();

                        break;
                    }
                case api.StringMatch.MatchTypeOneofCase.StartsWith:
                    {
                        whereClause =
                            fieldValue => fieldValue.ToLower().StartsWith(stringMatch.StartsWith.ToLower());

                        break;
                    }
                case api.StringMatch.MatchTypeOneofCase.None:
                    {
                        throw new ArgumentException("'None case' is not a valid search option. This can happen when you send a search type with an undefined value.");
                    }
                default:
                    {
                        throw new NotImplementedException("Unknown case: " + Enum.GetName(typeof(api.StringMatch.MatchTypeOneofCase), stringMatch.MatchTypeCase));
                    }
            }

            if (whereClause != null)
            {
                var combined = ParameterReplacer.Replace<Func<string, bool>, Func<T, bool>>(
                    whereClause,
                    whereClause.Parameters.Single(),
                    column
                );

                query = query.Where(combined);
            }

            return query;
        }

        public static IQueryable<T> StringMatch<T>(
            this IQueryable<T> query,
            Expression<Func<T, string>> column,
            StringMatch stringMatch
        )
            where T : IEntity
        {
            if (stringMatch == null)
            {
                return query;
            }

            Expression<Func<string, bool>> whereClause;

            switch (stringMatch.Type)
            {
                case StringMatchType.Exact:
                    {
                        whereClause =
                            fieldValue => fieldValue.ToLower() == stringMatch.Value.ToLower();
                        break;
                    }
                case StringMatchType.StartsWith:
                    {
                        whereClause =
                            fieldValue => fieldValue.ToLower().StartsWith(stringMatch.Value.ToLower());
                        break;
                    }
                case StringMatchType.Contains:
                    {
                        whereClause =
                            fieldValue => fieldValue.ToLower().Contains(stringMatch.Value.ToLower());
                        break;
                    }
                default:
                    {
                        throw new NotImplementedException("Unknown case: " + Enum.GetName(typeof(StringMatchType), stringMatch.Type));
                    }
            }

            if (whereClause != null)
            {
                var combined = ParameterReplacer.Replace<Func<string, bool>, Func<T, bool>>(
                    whereClause,
                    whereClause.Parameters.Single(),
                    column
                );

                query = query.Where(combined);
            }

            return query;
        }

        public static IQueryable<T> BooleanMatch<T>(
            this IQueryable<T> query,
            Expression<Func<T, bool>> column,
            bool? filter
        )
            where T : IEntity
        {
            if (!filter.HasValue)
            {
                return query;
            }

            Expression<Func<bool, bool>> whereClause = fieldValue => fieldValue == filter.Value;

            var combined = ParameterReplacer.Replace<Func<bool, bool>, Func<T, bool>>(
                whereClause,
                whereClause.Parameters.Single(),
                column
            );

            return query.Where(combined);
        }

        public static IQueryable<T> ExactStringMatch<T>(
            this IQueryable<T> query,
            Expression<Func<T, string>> column,
            string filter
        )
            where T : IEntity
        {
            if (string.IsNullOrEmpty(filter))
            {
                return query;
            }

            Expression<Func<string, bool>> whereClause = fieldValue => fieldValue.ToLower() == filter.ToLower();

            var combined = ParameterReplacer.Replace<Func<string, bool>, Func<T, bool>>(
                whereClause,
                whereClause.Parameters.Single(),
                column
            );

            return query.Where(combined);
        }

        public static IQueryable<T> DateMatch<T>(
            this IQueryable<T> query,
            Expression<Func<T, DateTime?>> dateColumn,
            DateMatch? dateMatch
            )

        {
            if (dateMatch == null)
            {
                return query;
            }

            Expression<Func<DateTime?, bool>> whereClause;

            switch (dateMatch.Type)
            {
                case MatchType.Exact:
                    if (dateMatch.Date.HasValue)
                    {
                        var start = dateMatch.Date.Value.Date;
                        var end = start.AddDays(1);

                        whereClause = date => date >= start && date < end;
                    }
                    else
                    {
                        throw new ArgumentException("Exact date is null");
                    }
                    break;
                case MatchType.Range:
                    {
                        var absoluteDateRange = dateMatch.DateRange!.AbsoluteDateRange;
                        var relativeDateRange = dateMatch.DateRange!.RelativeDateRange;

                        if (absoluteDateRange != null && dateMatch.DateRange.Type == RangeType.Absolute)
                        {
                            whereClause = absoluteDateRange.IncludeTime.HasValue && absoluteDateRange.IncludeTime.Value
                                ? CreateDateTimeMatchWhereClause(absoluteDateRange.OnOrAfter, absoluteDateRange.Before,
                                    RangeType.Absolute)
                                : CreateDateMatchWhereClause(absoluteDateRange.OnOrAfter, absoluteDateRange.Before,
                                    RangeType.Absolute);
                        }
                        else if (relativeDateRange != null && dateMatch.DateRange.Type == RangeType.Relative)
                        {
                            var timeZone = relativeDateRange.TimeZone;

                            var startDate = GetRelativeDateTime(relativeDateRange.StartDate, timeZone);
                            var endDate = GetRelativeDateTime(relativeDateRange.EndDate, timeZone);

                            whereClause = !string.IsNullOrWhiteSpace(timeZone)
                                ? CreateDateTimeMatchWhereClause(startDate, endDate, RangeType.Relative)
                                : CreateDateMatchWhereClause(startDate, endDate, RangeType.Relative);
                        }
                        else
                        {
                            throw new NotImplementedException("Unknown range type: " + Enum.GetName(typeof(RangeType), dateMatch.DateRange.Type));
                        }
                        break;
                    }
                default:
                    throw new NotImplementedException("Unknown match type: " + Enum.GetName(typeof(MatchType), dateMatch.Type));
            }

            if (whereClause != null)
            {
                var combined = ParameterReplacer.Replace<Func<DateTime?, bool>, Func<T, bool>>(
                    whereClause,
                    whereClause.Parameters.Single(),
                    dateColumn
                );

                query = query.Where(combined);
            }

            return query;
        }

        private static DateTime? GetRelativeDateTime(int? days, string? timeZone)
        {
            if (days == null)
            {
                return null;
            }

            var dateTime = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(timeZone))
            {
                dateTime = dateTime.ConvertDateTimeFromUtc(timeZone); // "America/New_York" Get IANA time zones
            }
            return dateTime.AddDays(days.Value);
        }

        // Maybe we should left only CreateDateTimeMatchWhereClause and search By Date and Time?
        // Otherwise it should be refactored!!
        private static Expression<Func<DateTime?, bool>> CreateDateMatchWhereClause(
            DateTime? startDate,
            DateTime? endDate,
            RangeType rangeType)
        {
            Expression<Func<DateTime?, bool>> whereClause;

            if (startDate.HasValue && endDate.HasValue)
            {
                whereClause = date => date != null && date.Value >= startDate.Value.Date && date.Value < endDate.Value.Date;
            }
            else if (startDate.HasValue)
            {
                whereClause = date => date != null && date.Value >= startDate.Value.Date;
            }
            else if (endDate.HasValue)
            {
                whereClause = date => date != null && date.Value < endDate.Value.Date;
            }
            else
            {
                throw new ArgumentException($"{rangeType} date range doesn't contain valid startDate or endDate");
            }

            return whereClause;
        }

        private static Expression<Func<DateTime?, bool>> CreateDateTimeMatchWhereClause(
            DateTime? startDate,
            DateTime? endDate,
            RangeType rangeType)
        {
            Expression<Func<DateTime?, bool>> whereClause;

            if (startDate.HasValue && endDate.HasValue)
            {
                whereClause = date => date != null && date.Value >= startDate.Value && date.Value < endDate.Value;
            }
            else if (startDate.HasValue)
            {
                whereClause = date => date != null && date.Value >= startDate.Value;
            }
            else if (endDate.HasValue)
            {
                whereClause = date => date != null && date.Value < endDate.Value;
            }
            else
            {
                throw new ArgumentException($"{rangeType} date range doesn't contain valid startDate or endDate");
            }

            return whereClause;
        }
    }
}
