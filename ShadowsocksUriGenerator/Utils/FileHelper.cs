using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.Utils
{
    public static class FileHelper
    {
        public static readonly JsonSerializerOptions ConfigJsonSerializerOptions = new()
        {
            AllowTrailingCommas = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            IgnoreReadOnlyProperties = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true,
        };

        public static readonly JsonSerializerOptions DataJsonSerializerOptions = new()
        {
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            IgnoreReadOnlyProperties = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true,
        };

        public static readonly JsonSerializerOptions APISnakeCaseJsonSerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            IgnoreReadOnlyProperties = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = true,
        };

        /// <summary>
        /// Loads data from a JSON file.
        /// </summary>
        /// <typeparam name="T">Data object type.</typeparam>
        /// <param name="filename">JSON file name.</param>
        /// <param name="jsonSerializerOptions">Deserialization options.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the read operation.</param>
        /// <returns>
        /// A ValueTuple containing a data object loaded from the JSON file and an error message.
        /// The error message is null if no errors occurred.
        /// </returns>
        public static async Task<(T, string? errMsg)> LoadJsonAsync<T>(string filename, JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = default) where T : class, new()
        {
            if (!File.Exists(filename))
                return (new(), null);

            T? jsonData = null;
            string? errMsg = null;
            FileStream? jsonFile = null;

            try
            {
                jsonFile = new(filename, FileMode.Open);
                jsonData = await JsonSerializer.DeserializeAsync<T>(jsonFile, jsonSerializerOptions, cancellationToken);
            }
            catch (Exception ex)
            {
                errMsg = $"Error: failed to load {filename}: {ex.Message}";
            }
            finally
            {
                if (jsonFile is not null)
                    await jsonFile.DisposeAsync();
            }

            jsonData ??= new();
            return (jsonData, errMsg);
        }

        /// <summary>
        /// Saves data to a JSON file.
        /// </summary>
        /// <typeparam name="T">Data object type.</typeparam>
        /// <param name="filename">JSON file name.</param>
        /// <param name="jsonData">The data object to save.</param>
        /// <param name="jsonSerializerOptions">Serialization options.</param>
        /// <param name="alwaysOverwrite">Always overwrite the original file.</param>
        /// <param name="noBackup">Do not create `filename.old` as backup.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the write operation.</param>
        /// <returns>An error message. Null if no errors occurred.</returns>
        public static async Task<string?> SaveJsonAsync<T>(
            string filename,
            T jsonData,
            JsonSerializerOptions? jsonSerializerOptions = null,
            bool alwaysOverwrite = false,
            bool noBackup = false,
            CancellationToken cancellationToken = default)
        {
            string? errMsg = null;
            FileStream? jsonFile = null;

            try
            {
                // create directory
                var directoryPath = Path.GetDirectoryName(filename);
                if (!string.IsNullOrEmpty(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                // save JSON
                if (alwaysOverwrite || !File.Exists(filename)) // alwaysOverwrite or file doesn't exist. Just write to it.
                {
                    jsonFile = new(filename, FileMode.Create);
                    await JsonSerializer.SerializeAsync(jsonFile, jsonData, jsonSerializerOptions, cancellationToken);
                }
                else // File exists. Write to `filename.new` and then replace with the new file and creates backup `filename.old`.
                {
                    jsonFile = new($"{filename}.new", FileMode.Create);
                    await JsonSerializer.SerializeAsync(jsonFile, jsonData, jsonSerializerOptions, cancellationToken);
                    jsonFile.Close();
                    File.Replace($"{filename}.new", filename, noBackup ? null : $"{filename}.old");
                }
            }
            catch (Exception ex)
            {
                errMsg = $"Error: failed to save {filename}: {ex.Message}";
            }
            finally
            {
                if (jsonFile is not null)
                    await jsonFile.DisposeAsync();
            }

            return errMsg;
        }
    }
}
