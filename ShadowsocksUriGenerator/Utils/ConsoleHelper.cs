using System.Text;

namespace ShadowsocksUriGenerator.Utils
{
    public static class ConsoleHelper
    {
        public static void PrintTableBorder(params int[] columnWidths)
        {
            foreach (var columnWidth in columnWidths)
            {
                Console.Write('+');
                for (var i = 0; i < columnWidth; i++)
                    Console.Write('-');
            }

            Console.WriteLine("+");
        }

        public static StringBuilder AppendTableBorder(this StringBuilder stringBuilder, params int[] columnWidths)
        {
            foreach (var columnWidth in columnWidths)
            {
                stringBuilder.Append('+');
                for (var i = 0; i < columnWidth; i++)
                    stringBuilder.Append('-');
            }

            stringBuilder.AppendLine("+");

            return stringBuilder;
        }

        public static void PrintNameList(IEnumerable<string> names, bool onePerLine = false)
        {
            if (onePerLine)
            {
                foreach (var name in names)
                {
                    Console.WriteLine(name);
                }
            }
            else
            {
                var stringBuilder = new StringBuilder();

                foreach (var name in names)
                {
                    if (name.Contains(' '))
                        stringBuilder.Append($"\"{name}\" ");
                    else
                        stringBuilder.Append($"{name} ");
                }

                if (names.Any())
                {
                    stringBuilder.Remove(stringBuilder.Length - 1, 1); // remove trailing space
                    stringBuilder.AppendLine();
                }

                var output = stringBuilder.ToString();
                Console.Write(output);
            }
        }
    }
}
