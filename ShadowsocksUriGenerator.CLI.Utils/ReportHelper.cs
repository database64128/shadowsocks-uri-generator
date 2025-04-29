using System.Text;

namespace ShadowsocksUriGenerator.CLI.Utils
{
    public static class ReportHelper
    {
        public static (string dataUsageByGroup, string dataUsageByUser) GenerateDataUsageCSV(
            IEnumerable<(string group, ulong bytesUsed, ulong bytesRemaining)> recordsByGroup,
            IEnumerable<(string username, ulong bytesUsed, ulong bytesRemaining)> recordsByUser)
        {
            var groupSB = new StringBuilder();
            groupSB.Append("Group,Data Used,Data Remaining\r\n");
            foreach (var (group, bytesUsed, bytesRemaining) in recordsByGroup)
            {
                groupSB.Append(group);
                if (bytesUsed > 0UL)
                    groupSB.Append($",{bytesUsed}");
                else
                    groupSB.Append(',');
                if (bytesRemaining > 0UL)
                    groupSB.Append($",{bytesRemaining}\r\n");
                else
                    groupSB.Append(",\r\n");
            }

            var userSB = new StringBuilder();
            userSB.Append("User,Data Used,Data Remaining\r\n");
            foreach (var (username, bytesUsed, bytesRemaining) in recordsByUser)
            {
                userSB.Append(username);
                if (bytesUsed > 0UL)
                    userSB.Append($",{bytesUsed}");
                else
                    userSB.Append(',');
                if (bytesRemaining > 0UL)
                    userSB.Append($",{bytesRemaining}\r\n");
                else
                    userSB.Append(",\r\n");
            }

            return (groupSB.ToString(), userSB.ToString());
        }
    }
}
