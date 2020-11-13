using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace shadowsocks_uri_generator
{
    public static class Utilities
    {
        public static readonly JsonSerializerOptions commonJsonSerializerOptions = new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
        };

        public static readonly JsonSerializerOptions snakeCaseJsonSerializerOptions = new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy(),
            WriteIndented = true,
        };

        public static readonly JsonSerializerOptions commonJsonDeserializerOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        /// <summary>
        /// Load data from a JSON file.
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
        /// Save data to a JSON file.
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
