using ShadowsocksUriGenerator.Utils;
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
            var parseResult = InteractionHelper.TryParseDataLimitString(dataLimitString, out var parsedDataLimit);

            Assert.Equal(expectedResult, parseResult);
            Assert.Equal(expectedDataLimit, parsedDataLimit);
        }

        [Theory]
        [InlineData(0UL, false, false, "0")]
        [InlineData(0UL, false, true, "0 B")]
        [InlineData(0UL, true, false, "0")]
        [InlineData(0UL, true, true, "0 B")]
        [InlineData(1024UL, false, false, "1.024 K")]
        [InlineData(2048UL, false, true, "2.048 KB")]
        [InlineData(2560UL, true, false, "2.5 Ki")]
        [InlineData(4096UL, true, true, "4 KiB")]
        [InlineData(4194304UL, false, false, "4.194 M")]
        [InlineData(6291456UL, false, true, "6.291 MB")]
        [InlineData(8388608UL, true, false, "8 Mi")]
        [InlineData(536870912UL, true, true, "512 MiB")]
        [InlineData(1073741824UL, false, false, "1.074 G")]
        [InlineData(137438953472UL, false, true, "137.4 GB")]
        [InlineData(137975824384UL, true, false, "128.5 Gi")]
        [InlineData(1098437885952UL, true, true, "1023 GiB")]
        [InlineData(1099511627776UL, false, false, "1.1 T")]
        [InlineData(140737488355328UL, false, true, "140.7 TB")]
        [InlineData(281474976710656UL, true, false, "256 Ti")]
        [InlineData(1124800395214848UL, true, true, "1023 TiB")]
        [InlineData(1125899906842624UL, false, false, "1.126 P")]
        [InlineData(144115188075855872UL, false, true, "144.1 PB")]
        [InlineData(288230376151711744UL, true, false, "256 Pi")]
        [InlineData(1151795604700004352UL, true, true, "1023 PiB")]
        [InlineData(1152921504606846976UL, false, false, "1.153 E")]
        [InlineData(2305843009213693952UL, false, true, "2.306 EB")]
        [InlineData(4611686018427387904UL, true, false, "4 Ei")]
        [InlineData(6917529027641081856UL, true, true, "6 EiB")]
        public void HumanReadableDataString_FromUlong_ToString(ulong dataInBytes, bool middle_i, bool trailingB, string expectedDataString)
        {
            var dataString = InteractionHelper.HumanReadableDataString(dataInBytes, middle_i, trailingB);

            Assert.Equal(expectedDataString, dataString);
        }
    }
}
