using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
            Sequence = 0;
            Startdate = DateTime.Now;
            FinishDate = DateTime.MaxValue;
            PlayerNick = "Anonymous";
        }
        public int Id { get; set; }
        [NotMapped]
        public Dictionary<int, Tile> PlayerTiles 
        {
            get
            {
                if (Tiles == null)
                    Tiles = new List<Tile>();
                return Tiles.Where(t=>t.Owner == ShotAt.playerTile).ToDictionary(v => v.Number, v => v);
            }
            set
            {
                Tiles = new List<Tile>();
                foreach (var tile in value)
                {
                    Tiles.Add(tile.Value);
                }
            }
        }
        public virtual List<Tile> Tiles { get; set; }
        [NotMapped]
        public Dictionary<int, Tile> AITiles
        {
            get
            {
                if (Tiles == null)
                    Tiles = new List<Tile>();
                return Tiles.Where(t=>t.Owner == ShotAt.aiTile).ToDictionary(v => v.Number, v => v);
            }
            set
            {
                Tiles = new List<Tile>();
                foreach (var tile in value)
                {
                    Tiles.Add(tile.Value);
                }
            }
        }
        
        public int Sequence { get; set; }
        public DateTime Startdate { get; set; }
        public DateTime FinishDate { get; set; }
        public string PlayerNick { get; set; }
    }
    public enum TileType { water, ship, shot, drowned, missed }
}