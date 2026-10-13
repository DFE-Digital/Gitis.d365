using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DFE.GITIS.CRM.Plugins.Tests
{
    public class DataServicesTests
    {
        [Fact]
        public void GetNextWorkingDayForFriday()
        {
            var result = DataServices.GetNextWorkingDay(DateTime.Parse("20/08/2021"));
            Assert.Equal(DayOfWeek.Monday, result.DayOfWeek);
        }

        [Fact]
        public void GetNextWorkingDayForMonday()
        {
            var result = DataServices.GetNextWorkingDay(DateTime.Parse("23/08/2021"));
            Assert.Equal(DayOfWeek.Tuesday, result.DayOfWeek);
        }
    }
}
