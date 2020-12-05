using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator
{
    public static class Utilities
    {
        public static readonly JsonSerializerOptions commonJsonSerializerOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
        };

        public static readonly JsonSerializerOptions snakeCaseJsonSerializerOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy(),
            WriteIndented = true,
        };

        public static readonly JsonSerializerOptions commonJsonDeserializerOptions = new JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true,
        };

        /// <summary>
        /// Tries to parse a data limit string.
        /// </summary>
        /// <param name="dataLimit">The data limit string to parse.</param>
        /// <param name="dataLimitInBytes">The parsed data limit in bytes.</param>
        /// <returns>True on successful parsing. False on failure.</returns>
        public static bool TryParseDataLimitString(string dataLimit, out ulong dataLimitInBytes)
        {
            dataLimitInBytes = 0UL;
            if (string.IsNullOrEmpty(dataLimit))
                return false;
            ulong multiplier = 1UL;
            switch (dataLimit[^1])
            {
                case 'K':
                    multiplier = 1024UL;
                    break;
                case 'M':
                    multiplier = 1024UL * 1024UL;
                    break;
                case 'G':
                    multiplier = 1024UL * 1024UL * 1024UL;
                    break;
                case 'T':
                    multiplier = 1024UL * 1024UL * 1024UL * 1024UL;
                    break;
                case 'P':
                    multiplier = 1024UL * 1024UL * 1024UL * 1024UL * 1024UL;
                    break;
                case 'E':
                    multiplier = 1024UL * 1024UL * 1024UL * 1024UL * 1024UL * 1024UL;
                    break;
            }
            if (multiplier == 1UL)
                return ulong.TryParse(dataLimit, out dataLimitInBytes);
            else if (ulong.TryParse(dataLimit[0..^1], out var dataLimitBeforeMultiplication))
            {
                dataLimitInBytes = dataLimitBeforeMultiplication * multiplier;
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Loads data from a JSON file.
        /// </summary>
        /// <typeparam name="T">Data object type.</typeparam>
        /// <param name="filename">JSON file name.</param>
        /// <param name="jsonSerializerOptions">Deserialization options.</param>
        /// <returns>A data object loaded from the JSON file.</returns>
        public static async Task<T> LoadJsonAsync<T>(string filename, JsonSerializerOptions? jsonSerializerOptions = null) where T : class, new()
        {
            if (!File.Exists(filename))
                return new T();

            T? jsonData = null;
            FileStream? jsonFile = null;
            try
            {
                jsonFile = new FileStream(filename, FileMode.Open);
                jsonData = await JsonSerializer.DeserializeAsync<T>(jsonFile, jsonSerializerOptions);
            }
            catch
            {
                Console.WriteLine($"Error: failed to load {filename}.");
                Environment.Exit(1);
            }
            finally
            {
                if (jsonFile != null)
                    await jsonFile.DisposeAsync();
            }
            if (jsonData != null)
                return jsonData;
            else
                return new T();
        }

        /// <summary>
        /// Saves data to a JSON file.
        /// </summary>
        /// <typeparam name="T">Data object type.</typeparam>
        /// <param name="filename">JSON file name.</param>
        /// <param name="jsonData">The data object to save.</param>
        /// <param name="jsonSerializerOptions">Serialization options.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public static async Task SaveJsonAsync<T>(string filename, T jsonData, JsonSerializerOptions? jsonSerializerOptions = null)
        {
            FileStream jsonFile = null!;
            try
            {
                jsonFile = new FileStream(filename, FileMode.Create);
                await JsonSerializer.SerializeAsync(jsonFile, jsonData, jsonSerializerOptions);
            }
            catch
            {
                Console.WriteLine($"Error: failed to save {filename}.");
            }
            finally
            {
                if (jsonFile != null)
                    await jsonFile.DisposeAsync();
            }
        }
    }
}
