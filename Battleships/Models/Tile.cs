using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Battleships.Models
{
    public class Tile
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public int Number { get; set; }
        public TileType Type { get; set; }
        public ShotAt Owner { get; set; }
    }

    public enum ShotAt { aiTile, playerTile }
}