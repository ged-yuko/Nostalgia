using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Nostalgia
{
    static class Extensions
    {
        public static Type[] GetTypesOrNone(this Assembly asm)
        {
            try
            {
                return asm.GetTypes();
            }
            catch (Exception ex)
            {
                return new Type[0];
            }
        }

        public static int GetWeekNumber(this DateTime t)
        {
            var weeksFirstDay = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            var weekRule = CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule;
            var weekNum = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(t, weekRule, weeksFirstDay);
            return weekNum;
        }

        public static void InvokeAsync(this DispatcherObject obj, Action act, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            obj.Dispatcher.InvokeAsync(act, priority);
        }

        public static int InternalBinarySearch<T>(this IList<T> array, T value, Comparison<T> comparer)
        {
            return InternalBinarySearch(array, 0, array.Count, value, comparer);
        }


        public static int InternalBinarySearch<T>(this IList<T> array, int index, int length, T value, Comparison<T> comparer)
        {
            int num = index;
            int num2 = index + length - 1;
            while (num <= num2)
            {
                int num3 = num + (num2 - num >> 1);
                int num4 = comparer(array[num3], value);
                if (num4 == 0)
                {
                    return num3;
                }
                if (num4 < 0)
                {
                    num = num3 + 1;
                }
                else
                {
                    num2 = num3 - 1;
                }
            }
            return ~num;
        }

        public static IComparer<T> CreateComparer<T>(Comparison<T> comparison)
        {
            return Comparer<T>.Create(comparison);
        }
    }
}
