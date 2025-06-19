using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChronoLoop.Components.Pages
{
    public static class Storage
    {
        private static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, "ChronoSave.json");

        public static async Task<List<Home>> LoadAsync()
        {
            System.Diagnostics.Debug.WriteLine($"Tracker file path: {FilePath}");

            if (!File.Exists(FilePath))
                return new List<Home>();

            using var stream = File.OpenRead(FilePath);
            return await JsonSerializer.DeserializeAsync<List<Home>>(stream) ?? new List<Home>();
        }

        public static async Task SaveAsync(List<Home> trackers)
        {
            Console.WriteLine($"Saving to: {FilePath}");

            using var stream = File.Create(FilePath);
            await JsonSerializer.SerializeAsync(stream, trackers);
        }
    }
}
