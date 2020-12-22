using Xunit;

namespace ShadowsocksUriGenerator.Tests
{
    public class UtilTests
    {
        [Theory]
        [InlineData("0", true, 0UL)]
        [InlineData("1024", true, 1024UL)]
        [InlineData("2K", true, 2048UL)]
        [InlineData("4M", true, 4194304UL)]
        [InlineData("8G", true, 8589934592UL)]
        [InlineData("16T", true, 17592186044416UL)]
        [InlineData("32P", true, 36028797018963968UL)]
        [InlineData("8E", true, 9223372036854775808UL)]
        [InlineData("", false, 0UL)]
        [InlineData("M", false, 0UL)]
        [InlineData("64B", false, 0UL)]
        [InlineData("128g", false, 0UL)]
        [InlineData("BYTE", false, 0UL)]
        [InlineData("32,768", false, 0UL)]
        [InlineData("65535MEM", false, 0UL)]
        public void Parse_DataLimitString_ReturnsBoolUlong(string dataLimitString, bool expectedResult, ulong expectedDataLimit)
        {
            var parseResult = Utilities.TryParseDataLimitString(dataLimitString, out var parsedDataLimit);

            Assert.Equal(expectedResult, parseResult);
            Assert.Equal(expectedDataLimit, parsedDataLimit);
        }

        [Theory]
        [InlineData(0UL, false, false, "0")]
        [InlineData(0UL, false, true, "0B")]
        [InlineData(0UL, true, false, "0")]
        [InlineData(0UL, true, true, "0B")]
        [InlineData(1024UL, false, false, "1K")]
        [InlineData(2048UL, false, true, "2KB")]
        [InlineData(2560UL, true, false, "2.5Ki")]
        [InlineData(4096UL, true, true, "4KiB")]
        [InlineData(4194304UL, false, false, "4M")]
        [InlineData(6291456UL, false, true, "6MB")]
        [InlineData(8388608UL, true, false, "8Mi")]
        [InlineData(536870912UL, true, true, "512MiB")]
        [InlineData(1073741824UL, false, false, "1G")]
        [InlineData(137438953472UL, false, true, "128GB")]
        [InlineData(137975824384UL, true, false, "128.5Gi")]
        [InlineData(1098437885952UL, true, true, "1023GiB")]
        [InlineData(1099511627776UL, false, false, "1T")]
        [InlineData(140737488355328UL, false, true, "128TB")]
        [InlineData(281474976710656UL, true, false, "256Ti")]
        [InlineData(1124800395214848UL, true, true, "1023TiB")]
        [InlineData(1125899906842624UL, false, false, "1P")]
        [InlineData(144115188075855872UL, false, true, "128PB")]
        [InlineData(288230376151711744UL, true, false, "256Pi")]
        [InlineData(1151795604700004352UL, true, true, "1023PiB")]
        [InlineData(1152921504606846976UL, false, false, "1E")]
        [InlineData(2305843009213693952UL, false, true, "2EB")]
        [InlineData(4611686018427387904UL, true, false, "4Ei")]
        [InlineData(6917529027641081856UL, true, true, "6EiB")]
        public void HumanReadableDataString_FromUlong_ToString(ulong dataInBytes, bool middle_i, bool trailingB, string expectedDataString)
        {
            var dataString = Utilities.HumanReadableDataString(dataInBytes, middle_i, trailingB);

            Assert.Equal(expectedDataString, dataString);
        }
    }
}
