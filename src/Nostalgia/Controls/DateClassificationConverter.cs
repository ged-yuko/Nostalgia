using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace Nostalgia.Controls
{
    public enum DateClassificator
    {
        Pinned = -1,
        Today = 0,
        Yesterday,
        ThisWeek,
        LastWeek,
        ThisMonth,
        LastMonth,
        Older
    }

    public class DateClassificationCategory : IComparable<DateClassificationCategory>, IComparable
    {
        static readonly object _lock = new object();
        static readonly Dictionary<DateClassificator, DateClassificationCategory> _cats = new Dictionary<DateClassificator, DateClassificationCategory>();

        public static DateClassificationCategory Get(DateClassificator key)
        {
            lock (_lock)
            {
                if (!_cats.TryGetValue(key, out var val))
                {
                    _cats.Add(key, val = new DateClassificationCategory(key));
                }

                return val;
            }
        }

        public static DateClassificationCategory Get(DateTime t)
        {
            var now = DateTime.Now.Date;
            var curr = t.Date;
            DateClassificator result;

            if (now == curr)
                result = DateClassificator.Today;
            else if (now == curr.AddDays(1))
                result = DateClassificator.Yesterday;
            else if (now.GetWeekNumber() == curr.GetWeekNumber())
                result = DateClassificator.ThisWeek;
            else if (now.GetWeekNumber() == curr.AddDays(7).GetWeekNumber())
                result = DateClassificator.LastWeek;
            else if (now.Month == curr.Month)
                result = DateClassificator.ThisMonth;
            else if (now.Month == curr.AddMonths(1).Month)
                result = DateClassificator.LastMonth;
            else
                result = DateClassificator.Older;

            return DateClassificationCategory.Get(result);
        }

        public static DateClassificationCategory Get(RecentProject p)
        {
            if (p.IsFavorite)
                return DateClassificationCategory.Get(DateClassificator.Pinned);
            else
                return DateClassificationCategory.Get(p.LastAccessed.DateTime);
        }

        public DateClassificator Classificator { get; }

        public DateClassificationCategory(DateClassificator key)
        {
            this.Classificator = key;
        }

        public int CompareTo(DateClassificationCategory other)
        {
            return ((int)this.Classificator).CompareTo((int)other.Classificator);
        }

        public int CompareTo(object obj)
        {
            if (obj is DateClassificationCategory other)
                return this.CompareTo(other);
            else
                return -1;
        }
    }

    public class DateClassificationConverter : MarkupExtension, IValueConverter
    {
        public DateClassificationConverter()
        {
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case RecentProject project: return DateClassificationCategory.Get(project);
                case DateTimeOffset t: return DateClassificationCategory.Get(t.DateTime);
                default: return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
