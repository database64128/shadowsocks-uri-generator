using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace shadowsocks_uri_generator
{
    public static class Utilities
    {
        /// <summary>
        /// Load data from a JSON file.
        /// </summary>
        /// <typeparam name="T">Data object type.</typeparam>
        /// <param name="filename">JSON file name.</param>
        /// <param name="jsonSerializerOptions">Deserialization options.</param>
        /// <returns>A data object loaded from the JSON file.</returns>
        public static async Task<T> LoadJsonAsync<T>(string filename, JsonSerializerOptions? jsonSerializerOptions = null) where T : new()
        {
            T jsonData;

            if (!File.Exists(filename))
            {
                jsonData = new T();
                return jsonData;
            }

            FileStream jsonFile = null!;
            try
            {
                jsonFile = new FileStream(filename, FileMode.Open);
                jsonData = await JsonSerializer.DeserializeAsync<T>(jsonFile, jsonSerializerOptions);
            }
            catch
            {
                jsonData = new T(); // make the compiler happy, even though we don't need it.
                Console.WriteLine($"Error: failed to load {filename}.");
                Environment.Exit(1);
            }
            finally
            {
                if (jsonFile != null)
                    await jsonFile.DisposeAsync();
            }
            return jsonData;
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
