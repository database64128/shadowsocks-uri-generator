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
        public static readonly JsonSerializerOptions commonJsonSerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
        };

        public static readonly JsonSerializerOptions snakeCaseJsonSerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy(),
            WriteIndented = true,
        };

        public static readonly JsonSerializerOptions commonJsonDeserializerOptions = new()
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true,
        };

        public static readonly string configDirectory;

        static Utilities()
        {
#if PACKAGED
            // ~/.config on Linux
            // ~/AppData/Roaming on Windows
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            configDirectory = $"{appDataPath}/shadowsocks-uri-generator";
#else
            // Use executable directory
            // Executable directory for single-file deployments in .NET 5: https://docs.microsoft.com/en-us/dotnet/core/deploying/single-file
            configDirectory = AppContext.BaseDirectory;
#endif
        }

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
        /// Converts a data representation in bytes
        /// to a human readable data string.
        /// </summary>
        /// <param name="dataInBytes">
        /// The number of bytes.
        /// </param>
        /// <param name="middle_i">
        /// Whether to use 'GiB', 'TiB' instead of 'GB', 'TB'.
        /// Defaults to false, or 'GB', 'TB'.
        /// No matter true or false, 1024 is always used as the base.
        /// </param>
        /// <param name="trailingB">
        /// Whether the returned string has a trailing 'B' representing bytes.
        /// Defaults to true, or 'GB', 'TB'.
        /// Set to false for 'G', 'T'.
        /// </param>
        /// <returns>
        /// A human readable string representation of the amount of data.
        /// </returns>
        public static string HumanReadableDataString(ulong dataInBytes, bool middle_i = false, bool trailingB = true)
        {
            var stringTail = $@"{(middle_i ? "i" : "")}{(trailingB ? "B" : "")}";

            return dataInBytes switch
            {
                < 1024UL => $@"{dataInBytes}{(trailingB ? "B" : "")}",
                < 1024UL * 1024UL => $@"{dataInBytes / 1024.0:G4}K{stringTail}",
                < 1024UL * 1024UL * 1024UL => $@"{dataInBytes / 1024.0 / 1024.0:G4}M{stringTail}",
                < 1024UL * 1024UL * 1024UL * 1024UL => $@"{dataInBytes / 1024.0 / 1024.0 / 1024.0:G4}G{stringTail}",
                < 1024UL * 1024UL * 1024UL * 1024UL * 1024UL => $@"{dataInBytes / 1024.0 / 1024.0 / 1024.0 / 1024.0:G4}T{stringTail}",
                < 1024UL * 1024UL * 1024UL * 1024UL * 1024UL * 1024UL => $@"{dataInBytes / 1024.0 / 1024.0 / 1024.0 / 1024.0 / 1024.0:G4}P{stringTail}",
                _ => $@"{dataInBytes / 1024.0 / 1024.0 / 1024.0 / 1024.0 / 1024.0 / 1024.0:G4}E{stringTail}",
            };
        }

        /// <summary>
        /// Gets the fully qualified absolute path
        /// that the original path points to.
        /// </summary>
        /// <param name="path">A relative or absolute path.</param>
        /// <returns>A fully qualified path.</returns>
        public static string GetAbsolutePath(string path)
            => Path.IsPathFullyQualified(path) ? path : $"{configDirectory}/{path}";

        /// <summary>
        /// Loads data from a JSON file.
        /// </summary>
        /// <typeparam name="T">Data object type.</typeparam>
        /// <param name="filename">JSON file name.</param>
        /// <param name="jsonSerializerOptions">Deserialization options.</param>
        /// <returns>A data object loaded from the JSON file.</returns>
        public static async Task<T> LoadJsonAsync<T>(string filename, JsonSerializerOptions? jsonSerializerOptions = null) where T : class, new()
        {
            // extend relative path
            filename = GetAbsolutePath(filename);

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
            FileStream? jsonFile = null;
            try
            {
                // extend relative path
                filename = GetAbsolutePath(filename);
                // create directory
                var directoryPath = Path.GetDirectoryName(filename) ?? throw new ArgumentException("Invalid path", nameof(filename));
                Directory.CreateDirectory(directoryPath);
                // save JSON
                jsonFile = new FileStream(filename, FileMode.Create);
                await JsonSerializer.SerializeAsync(jsonFile, jsonData, jsonSerializerOptions);
            }
            catch (ArgumentException)
            {
                Console.WriteLine($"Error: invalid path: {filename}.");
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

        public static void PrintTableBorder(params int[] columnWidths)
        {
            foreach (var columnWidth in columnWidths)
            {
                Console.Write('+');
                for (var i = 0; i < columnWidth; i++)
                    Console.Write('-');
            }
            Console.Write("+\n");
        }
    }
}
