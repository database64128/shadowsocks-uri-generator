﻿using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace ShadowsocksUriGenerator.Utils
{
    public static class FileHelper
    {
        /// <summary>
        /// Loads data from a JSON file.
        /// </summary>
        /// <typeparam name="T">Data object type.</typeparam>
        /// <param name="filename">JSON file name.</param>
        /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the read operation.</param>
        /// <returns>
        /// A ValueTuple containing a data object loaded from the JSON file and an error message.
        /// The error message is null if no errors occurred.
        /// </returns>
        public static async Task<(T, string? errMsg)> LoadJsonAsync<T>(string filename, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default) where T : class, new()
        {
            if (!File.Exists(filename))
                return (new(), null);

            T? jsonData = null;
            string? errMsg = null;
            FileStream? jsonFile = null;

            try
            {
                jsonFile = new(filename, FileMode.Open);
                jsonData = await JsonSerializer.DeserializeAsync<T>(jsonFile, jsonTypeInfo, cancellationToken);
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
        /// <param name="jsonTypeInfo">Metadata about the type to convert.</param>
        /// <param name="alwaysOverwrite">Always overwrite the original file.</param>
        /// <param name="noBackup">Do not create `filename.old` as backup.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the write operation.</param>
        /// <returns>An error message. Null if no errors occurred.</returns>
        public static async Task<string?> SaveJsonAsync<T>(
            string filename,
            T jsonData,
            JsonTypeInfo<T> jsonTypeInfo,
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
                    await JsonSerializer.SerializeAsync(jsonFile, jsonData, jsonTypeInfo, cancellationToken);
                }
                else // File exists. Write to `filename.new` and then replace with the new file and creates backup `filename.old`.
                {
                    jsonFile = new($"{filename}.new", FileMode.Create);
                    await JsonSerializer.SerializeAsync(jsonFile, jsonData, jsonTypeInfo, cancellationToken);
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
