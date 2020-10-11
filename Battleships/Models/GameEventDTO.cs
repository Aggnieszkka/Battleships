using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Battleships.Models
{
    public class GameEventDTO
    {
        public long GameNumber { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ShotAt ShotAt { get; set; }
        public int Index { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Tile Tile { get; set; }
        public int Sequence { get; set; }
    }
    public enum ShotAt { aiTile, playerTile}
}