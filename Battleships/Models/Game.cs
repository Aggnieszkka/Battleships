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
                Dictionary<int, Tile> playerTiles = Tiles.Where(t => t.Owner == ShotAt.playerTile).ToDictionary(t => t.Number, t => t);

                for (int i = 0; i < 100; i++)
                {
                    if (!playerTiles.ContainsKey(i))
                    {
                        playerTiles.Add(i, new Tile
                        {
                            Number = i,
                            GameId = Id,
                            Owner = ShotAt.playerTile,
                            Type = TileType.water
                        });
                    }
                }
                return playerTiles.OrderBy(t => t.Key).ToDictionary(t => t.Key, t => t.Value);
            }
            set
            {
                List<Tile> aiTiles = Tiles != null ? Tiles.Where(t => t.Owner == ShotAt.aiTile).ToList() : new List<Tile>();
                Tiles = new List<Tile>();
                Tiles.AddRange(value.Values);
                Tiles.AddRange(aiTiles);
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
                Dictionary<int, Tile> aiTiles = Tiles.Where(t => t.Owner == ShotAt.aiTile).ToDictionary(t => t.Number, t => t);

                for (int i = 0; i < 100; i++)
                {
                    if (!aiTiles.ContainsKey(i))
                    {
                        aiTiles.Add(i, new Tile
                        {
                            Number = i,
                            GameId = Id,
                            Owner = ShotAt.aiTile,
                            Type = TileType.water
                        });
                    }
                }
                return aiTiles.OrderBy(t => t.Key).ToDictionary(t => t.Key, t => t.Value);
            }
            set
            {
                List<Tile> playerTiles = Tiles != null ? Tiles.Where(t => t.Owner == ShotAt.playerTile).ToList() : new List<Tile>();
                Tiles = new List<Tile>();
                Tiles.AddRange(value.Values);
                Tiles.AddRange(playerTiles);
            }
        }

        public int Sequence { get; set; }
        public DateTime Startdate { get; set; }
        public DateTime FinishDate { get; set; }
        public string PlayerNick { get; set; }
    }
    public enum TileType { water, ship, shot, drowned, missed }
}