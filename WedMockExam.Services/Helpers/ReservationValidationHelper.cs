using System;

namespace WedMockExam.Services.Helpers
{
    public static class ReservationValidationHelper
    {
        public static bool IsPastDate(DateTime date)
        {
            return date.Date < DateTime.UtcNow.Date;
        }

        public static bool IsWorkingDay(DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }

        public static DateTime GetNextWorkingDay()
        {
            var nextDay = DateTime.UtcNow.AddDays(1);
            while (!IsWorkingDay(nextDay))
            {
                nextDay = nextDay.AddDays(1);
            }
            return nextDay;
        }

        public static bool IsWithinTwoWeekLimit(DateTime date)
        {
            var twoWeeksFromNow = DateTime.UtcNow.AddDays(14);
            return date.Date <= twoWeeksFromNow.Date;
        }

        public static bool IsDateValidForReservation(DateTime date)
        {
            if (IsPastDate(date))
            {
                return false;
            }

            if (!IsWorkingDay(date))
            {
                return false;
            }

            return IsWithinTwoWeekLimit(date);
        }
    }
} 