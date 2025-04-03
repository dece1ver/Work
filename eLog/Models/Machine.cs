using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace eLog.Models
{
    public class Machine
    {
        private static readonly HashSet<char> InvalidChars = new(
            Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars())
        );

        public Machine(string name)
        {
            Name = name;
        }

        public string Name { get; }

        [JsonIgnore]
        public string SafeName => new(Name.Select(ch => InvalidChars.Contains(ch) ? '-' : ch).ToArray());
    }
}