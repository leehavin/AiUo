using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiUo.BIZ.DataSplit;
using AiUo.BIZ.DataSplit.DAL;
using AiUo.Text;

namespace AiUo.BIZ.DataSplit.JOB.DataMove
{
    internal class ColumnValueHelper
    {
        private S_split_tablePO _option;
        public ColumnValueHelper(S_split_tablePO option) 
        {
            _option = option;
        }
        public ColumnValue GetKeepEndValue(DateTime execTime)
        {
            var ret = new ColumnValue();
            switch ((MoveKeepMode)_option.MoveKeepMode)
            {
                case MoveKeepMode.None:
                    ret.Date = execTime.Date;
                    break;
                case MoveKeepMode.Day:
                    ret.Date = execTime.AddDays(-_option.MoveKeepValue).Date;
                    break;
                case MoveKeepMode.Week:
                    var weekDate = execTime.AddDays(-_option.MoveKeepValue * 7).Date;
                    ret.Date = DateTimeUtil.BeginDayOfWeek(weekDate);
                    break;
                case MoveKeepMode.Month:
                    var monthDate = execTime.AddMonths(-_option.MoveKeepValue).Date;
                    ret.Date = new DateTime(monthDate.Year, monthDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    break;
                case MoveKeepMode.Quarter:
                    var quarterDate = execTime.AddMonths(-_option.MoveKeepValue * 3);
                    var quarterMonth = Math.DivRem(quarterDate.Month - 1, 3, out int _) * 3 + 1;
                    ret.Date = new DateTime(quarterDate.Year, quarterMonth, 1, 0, 0, 0, DateTimeKind.Utc);
                    break;
                case MoveKeepMode.Year:
                    ret.Date = new DateTime(execTime.Year - _option.MoveKeepValue, 1, 1);
                    break;
                default:
                    throw new Exception("未知的MoveKeepMode");
            }
            ret.Value = ColumnDateToValue(ret.Date);
            return ret;
        }
        public ColumnValue ParseColumnValue(object value)
        {
            var ret = new ColumnValue();
            if (value == null || value == DBNull.Value)
                return ret;
            switch ((ColumnType)_option.ColumnType)
            {
                case ColumnType.DateTime:
                    var dt = value.ConvertTo<DateTime>();
                    ret.Date = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, DateTimeKind.Utc);
                    ret.Value = dt.ToString("yyyy-MM-dd");
                    break;
                case ColumnType.ObjectId:
                    ret.Value = Convert.ToString(value);
                    var dt1 = ObjectId.ParseTimestamp(ret.Value);
                    ret.Date = new DateTime(dt1.Year, dt1.Month, dt1.Day, 0, 0, 0, DateTimeKind.Utc);
                    break;
                case ColumnType.NumDay:
                    ret.Value = Convert.ToString(value);
                    var dt2 = ret.Value.ToDateTime("yyyyMMdd");
                    ret.Date = new DateTime(dt2.Year, dt2.Month, dt2.Day, 0, 0, 0, DateTimeKind.Utc);
                    break;
                case ColumnType.NumWeek:
                    ret.Value = Convert.ToString(value);
                    var year = ret.Value.Substring(0, 4).ToInt32();
                    var week = ret.Value.Substring(4).ToInt32();
                    var dt3 = DateTimeUtil.BeginDayOfWeek(year, week);
                    ret.Date = new DateTime(dt3.Year, dt3.Month, dt3.Day, 0, 0, 0, DateTimeKind.Utc);
                    break;
                case ColumnType.NumMonth:
                    ret.Value = Convert.ToString(value);
                    var dt4 = ret.Value.ToDateTime("yyyyMM");
                    ret.Date = new DateTime(dt4.Year, dt4.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    break;
                case ColumnType.NumQuarter:
                    ret.Value = Convert.ToString(value);
                    var y = ret.Value.Substring(0, 4).ToInt32();
                    var q = ret.Value.Substring(4).ToInt32();
                    var m = q * 3 - 2;
                    ret.Date = new DateTime(y, m, 1, 0, 0, 0, DateTimeKind.Utc);
                    break;
                case ColumnType.NumYear:
                    ret.Value = Convert.ToString(value);
                    ret.Date = new DateTime(ret.Value.ConvertTo<int>(), 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    break;
                default:
                    throw new Exception($"未知的值。value:{value} columnType:{_option.ColumnType} columnName:{_option.ColumnName}");
            }

            return ret;
        }
        public DateTime ColumnValueToDate(string value)
        {
            switch ((ColumnType)_option.ColumnType)
            {
                case ColumnType.DateTime:
                    var dt = value.ToDateTime();
                    return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, DateTimeKind.Utc);
                case ColumnType.ObjectId:
                    var dt1 = ObjectId.ParseTimestamp(value);
                    return new DateTime(dt1.Year, dt1.Month, dt1.Day, 0, 0, 0, DateTimeKind.Utc);
                case ColumnType.NumDay:
                    var dt2 = value.ToDateTime("yyyyMMdd");
                    return new DateTime(dt2.Year, dt2.Month, dt2.Day, 0, 0, 0, DateTimeKind.Utc);
                case ColumnType.NumWeek:
                    var year = value.Substring(0, 4).ToInt32();
                    var week = value.Substring(4).ToInt32();
                    var dt3 = DateTimeUtil.BeginDayOfWeek(year, week);
                    return new DateTime(dt3.Year, dt3.Month, dt3.Day, 0, 0, 0, DateTimeKind.Utc);
                case ColumnType.NumMonth:
                    var dt4 =value.ToDateTime("yyyyMM");
                    return new DateTime(dt4.Year, dt4.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                case ColumnType.NumQuarter:
                    var y = value.Substring(0, 4).ToInt32();
                    var q = value.Substring(4).ToInt32();
                    var m = q * 3 - 2;
                    return new DateTime(y, m, 1, 0, 0, 0, DateTimeKind.Utc);
                case ColumnType.NumYear:
                    return new DateTime(value.ConvertTo<int>(), 1, 1, 0, 0, 0, DateTimeKind.Utc);
                default:
                    throw new Exception($"未知的值。value:{value} columnType:{_option.ColumnType} columnName:{_option.ColumnName}");
            }
        }
        public string ColumnDateToValue(DateTime date)
        {
            switch ((ColumnType)_option.ColumnType)
            {
                case ColumnType.DateTime:
                    return date.ToString("yyyy-MM-dd");
                case ColumnType.ObjectId:
                    return ObjectId.TimestampId(date);
                case ColumnType.NumDay:
                    return date.ToString("yyyyMMdd");
                case ColumnType.NumWeek:
                    return DateTimeUtil.ToYearWeekString(date);
                case ColumnType.NumMonth:
                    return date.ToString("yyyyMM");
                case ColumnType.NumQuarter:
                    return DateTimeUtil.ToYearQuarter(date);
                case ColumnType.NumYear:
                    return date.Year.ToString();
                default:
                    throw new Exception("未知的ColumnType");
            }
        }
        public string GetColumnWhere(string begin, string end)
        {
            var list = new List<string>();
            switch ((ColumnType)_option.ColumnType)
            {
                case ColumnType.DateTime:
                    if (!string.IsNullOrEmpty(begin))
                        list.Add($"`{_option.ColumnName}`>='{begin}'");
                    list.Add($"`{_option.ColumnName}`<'{end}'");
                    break;
                case ColumnType.ObjectId:
                    if (!string.IsNullOrEmpty(begin))
                        list.Add($"`{_option.ColumnName}`>='{begin}'");
                    list.Add($"`{_option.ColumnName}`<'{end}' AND LENGTH(`{_option.ColumnName}`)=24");
                    break;
                case ColumnType.NumDay:
                    if (!string.IsNullOrEmpty(begin))
                        list.Add($"`{_option.ColumnName}`>={begin}");
                    list.Add($"`{_option.ColumnName}`<{end} AND LENGTH(`{_option.ColumnName}`)=8");
                    break;
                case ColumnType.NumWeek:
                    if (!string.IsNullOrEmpty(begin))
                        list.Add($"`{_option.ColumnName}`>={begin}");
                    list.Add($"`{_option.ColumnName}`<{end} AND LENGTH(`{_option.ColumnName}`)=6");
                    break;
                case ColumnType.NumMonth:
                    if (!string.IsNullOrEmpty(begin))
                        list.Add($"`{_option.ColumnName}`>={begin}");
                    list.Add($"`{_option.ColumnName}`<{end} AND LENGTH(`{_option.ColumnName}`)=6");
                    break;
                case ColumnType.NumQuarter:
                    if (!string.IsNullOrEmpty(begin))
                        list.Add($"`{_option.ColumnName}`>={begin}");
                    list.Add($"`{_option.ColumnName}`<{end} AND LENGTH(`{_option.ColumnName}`)=5");
                    break;
                case ColumnType.NumYear:
                    if (!string.IsNullOrEmpty(begin))
                        list.Add($"`{_option.ColumnName}`>={begin}");
                    list.Add($"`{_option.ColumnName}`<{end} AND LENGTH(`{_option.ColumnName}`)=4");
                    break;
            }
            if (!string.IsNullOrEmpty(_option.MoveWhere))
            {
                var where = _option.MoveWhere.ToUpper().Trim().TrimStart("AND ");
                list.Add(where);
            }
            return string.Join(" AND ", list);
        }
    }
    internal class ColumnValue
    {
        public DateTime Date { get; set; }
        public string Value { get; set; }
    }
}
