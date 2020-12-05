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
    }
}
