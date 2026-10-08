using ShadowsocksUriGenerator.Utils;

namespace ShadowsocksUriGenerator.Tests
{
    public class UtilTests
    {
        [Test]
        [Arguments("0", true, 0UL)]
        [Arguments("1024", true, 1024UL)]
        [Arguments("2K", true, 2048UL)]
        [Arguments("4M", true, 4194304UL)]
        [Arguments("8G", true, 8589934592UL)]
        [Arguments("16T", true, 17592186044416UL)]
        [Arguments("32P", true, 36028797018963968UL)]
        [Arguments("8E", true, 9223372036854775808UL)]
        [Arguments("", false, 0UL)]
        [Arguments("M", false, 0UL)]
        [Arguments("64B", false, 0UL)]
        [Arguments("128g", false, 0UL)]
        [Arguments("BYTE", false, 0UL)]
        [Arguments("32,768", false, 0UL)]
        [Arguments("65535MEM", false, 0UL)]
        public async Task Parse_DataLimitString_ReturnsBoolUlong(string dataLimitString, bool expectedResult, ulong expectedDataLimit)
        {
            var parseResult = InteractionHelper.TryParseDataLimitString(dataLimitString, out var parsedDataLimit);

            await Assert.That(parseResult).IsEqualTo(expectedResult);
            await Assert.That(parsedDataLimit).IsEqualTo(expectedDataLimit);
        }

        [Test]
        [Arguments(0UL, false, false, "0")]
        [Arguments(0UL, false, true, "0 B")]
        [Arguments(0UL, true, false, "0")]
        [Arguments(0UL, true, true, "0 B")]
        [Arguments(1024UL, false, false, "1.024 K")]
        [Arguments(2048UL, false, true, "2.048 KB")]
        [Arguments(2560UL, true, false, "2.5 Ki")]
        [Arguments(4096UL, true, true, "4 KiB")]
        [Arguments(4194304UL, false, false, "4.194 M")]
        [Arguments(6291456UL, false, true, "6.291 MB")]
        [Arguments(8388608UL, true, false, "8 Mi")]
        [Arguments(536870912UL, true, true, "512 MiB")]
        [Arguments(1073741824UL, false, false, "1.074 G")]
        [Arguments(137438953472UL, false, true, "137.4 GB")]
        [Arguments(137975824384UL, true, false, "128.5 Gi")]
        [Arguments(1098437885952UL, true, true, "1023 GiB")]
        [Arguments(1099511627776UL, false, false, "1.1 T")]
        [Arguments(140737488355328UL, false, true, "140.7 TB")]
        [Arguments(281474976710656UL, true, false, "256 Ti")]
        [Arguments(1124800395214848UL, true, true, "1023 TiB")]
        [Arguments(1125899906842624UL, false, false, "1.126 P")]
        [Arguments(144115188075855872UL, false, true, "144.1 PB")]
        [Arguments(288230376151711744UL, true, false, "256 Pi")]
        [Arguments(1151795604700004352UL, true, true, "1023 PiB")]
        [Arguments(1152921504606846976UL, false, false, "1.153 E")]
        [Arguments(2305843009213693952UL, false, true, "2.306 EB")]
        [Arguments(4611686018427387904UL, true, false, "4 Ei")]
        [Arguments(6917529027641081856UL, true, true, "6 EiB")]
        public async Task HumanReadableDataString_FromUlong_ToString(ulong dataInBytes, bool middle_i, bool trailingB, string expectedDataString)
        {
            var dataString = InteractionHelper.HumanReadableDataString(dataInBytes, middle_i, trailingB);

            await Assert.That(dataString).IsEqualTo(expectedDataString);
        }
    }
}