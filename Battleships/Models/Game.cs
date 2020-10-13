using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Battleships.Models
{
    public class Game
    {
        public Game()
        {
            PlayerTiles = new Dictionary<int, Tile>();
            AITiles = new Dictionary<int, Tile>();
            History = new List<string>();
            Sequence = 0;
        }
        public long GameNumber { get; set; }
        public Dictionary<int, Tile> PlayerTiles { get; set; }
        public Dictionary<int, Tile> AITiles { get; set; }
        public List<string> History { get; set; }
        public int Sequence { get; set; }
    }
    public enum Tile { water, ship, shot, drowned, missed }
}