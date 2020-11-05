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
        public GameEventDTO()
        {
            IsVisible = true;
        }
        public int GameNumber { get; set; }
        public int Index { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public TileType Tile { get; set; }
        public int Sequence { get; set; }
        public bool IsVisible { get; set; }
        public string Time { get; set; }
    }
}