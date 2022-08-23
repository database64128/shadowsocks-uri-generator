using System;
using System.Buffers;
using System.Text;

namespace ShadowsocksUriGenerator.Utils;

public static class Base64Url
{
    public static string Encode(string data) => Encode(Encoding.UTF8.GetBytes(data));

    public static string Encode(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static bool TryEncode(string data, out string base64url)
    {
        var byteCount = Encoding.UTF8.GetByteCount(data);
        var byteArray = ArrayPool<byte>.Shared.Rent(byteCount);
        Span<byte> bytes = byteArray;
        var bytesWritten = Encoding.UTF8.GetBytes(data, bytes);
        var ret = TryEncode(bytes[..bytesWritten], out base64url);
        ArrayPool<byte>.Shared.Return(byteArray);
        return ret;
    }

    public static bool TryEncode(ReadOnlySpan<byte> bytes, out string base64url)
    {
        var charsLength = (bytes.Length + 2) / 3 * 4;
        var charsArray = ArrayPool<char>.Shared.Rent(charsLength);
        Span<char> chars = charsArray;

        if (!Convert.TryToBase64Chars(bytes, chars, out var charsWritten))
        {
            base64url = "";
            ArrayPool<char>.Shared.Return(charsArray);
            return false;
        }

        chars = chars[..charsWritten].TrimEnd('=');

        for (var i = 0; i < charsWritten; i++)
        {
            switch (chars[i])
            {
                case '+':
                    chars[i] = '-';
                    break;
                case '/':
                    chars[i] = '_';
                    break;
            }
        }

        base64url = chars.ToString();
        ArrayPool<char>.Shared.Return(charsArray);
        return true;
    }

    public static string DecodeToString(string base64url) => Encoding.UTF8.GetString(DecodeToBytes(base64url));

    public static byte[] DecodeToBytes(string base64url)
    {
        var base64string = base64url.Replace('_', '/').Replace('-', '+');
        base64string = base64string.PadRight(base64string.Length + (4 - base64string.Length % 4) % 4, '=');
        return Convert.FromBase64String(base64string);
    }
}
