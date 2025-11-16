using System;
using Xunit;
using SmartGreenhouse.Data;
using SmartGreenhouse.Models;

namespace SmartGreenhouse.Tests
{
    public class TestDataService
    {
        [Fact]
        public void LogRecord_HasValidDefaults()
        {
            var record = new LogRecord
            {
                Timestamp = DateTime.Now,
                Action = "TestAction"
            };

            Assert.NotNull(record.Action);
            Assert.True(record.Timestamp <= DateTime.Now);
        }
    }
}
