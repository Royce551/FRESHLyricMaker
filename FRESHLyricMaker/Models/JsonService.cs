using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FRESHLyricMaker.Models
{
    public class JsonService
    {
        public static async Task<T> ReadAsync<T>(string path) where T : new()
        {
            if (!File.Exists(path))
            {
                var x = new T();
                await WriteAsync(path, x);
            }

            using var jsonString = new FileStream(path, FileMode.OpenOrCreate);
            var thing = await JsonSerializer.DeserializeAsync<T>(jsonString);
            if (thing is not null) return thing;
            else throw new Exception(""); // TODO: exception message
        }

        public static async Task WriteAsync(string path, object thing)
        {
            var jsonString = JsonSerializer.Serialize(thing);
            await File.WriteAllTextAsync(path, jsonString);
        }
    }
}
