using Battleships.Models;
using Battleships.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Battleships.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Pirate Wars";
            return View();
        }

        public string GetRandomShips()
        {
            var drawnIndexList = LocateShipTool.GetRandomShips();
            return Newtonsoft.Json.JsonConvert.SerializeObject(drawnIndexList);
        }

        public static List<Game> Games = new List<Game>();
        public string StartGame(List<int> shipArray)
        {
            Game game = new Game();
            game.GameNumber = Games.Count > 0 ? Games.Max(g => g.GameNumber) + 1 : 1;
            List<int> aiShips = LocateShipTool.GetRandomShips();
            for (int i = 0; i < 100; i++)
            {
                game.PlayerTiles.Add(i, shipArray.Contains(i) ? Tile.ship : Tile.water);
                game.AITiles.Add(i, aiShips.Contains(i) ? Tile.ship : Tile.water);
            }
            Games.Add(game);
            return Newtonsoft.Json.JsonConvert.SerializeObject(game.GameNumber);
        }
        public string ShootRandomly(int gameNumber)
        {
            var game = Games.FirstOrDefault(g => g.GameNumber == gameNumber);
            List<GameEventDTO> gameEventDTOList = new List<GameEventDTO>();

            game.Sequence++;
            var avaibleTiles = game.AITiles.Where(t => t.Value == Tile.water || t.Value == Tile.ship).Select(t => t.Key).ToList();
            if (avaibleTiles.Count > 0)
            {
                Random random = new Random(Guid.NewGuid().GetHashCode());
                var randomIndex = random.Next(0, avaibleTiles.Count - 1);
                ShootTool.TryShot(game, avaibleTiles[randomIndex], ShotAt.aiTile, gameEventDTOList);
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(gameEventDTOList);
        }
        public string Shoot(int gameNumber, int index)
        {
            var game = Games.FirstOrDefault(g => g.GameNumber == gameNumber);
            List<GameEventDTO> gameEventDTOList = new List<GameEventDTO>();
            game.Sequence++;
            ShootTool.TryShot(game, index, ShotAt.aiTile, gameEventDTOList);

            return Newtonsoft.Json.JsonConvert.SerializeObject(gameEventDTOList);
        }
        public string AIShot(int gameNumber)
        {
            List<GameEventDTO> gameEventDTOList = new List<GameEventDTO>();
            var game = Games.FirstOrDefault(g => g.GameNumber == gameNumber);
            var tiles = game.PlayerTiles.Select(t => t.Key).ToList();

            Random random = new Random(Guid.NewGuid().GetHashCode());
            List<int> avaibleTiles = new List<int>();

            do
            {
                avaibleTiles = game.PlayerTiles.Where(t => t.Value == Tile.water || t.Value == Tile.ship).Select(t => t.Key).ToList();

                if (avaibleTiles.Count > 0)
                {
                    //If there are player tiles shot make only opposite tiles avaible
                    if (game.PlayerTiles.ContainsValue(Tile.shot))
                    {
                        foreach (var playerTile in game.PlayerTiles.Where(p => p.Value == Tile.shot))
                        {
                            var index = playerTile.Key;
                            avaibleTiles = new List<int>();

                            var avaibleTilesDictionary = LocateShipTool.AreOppositeTilesAvaible(tiles, index);
                            foreach (var avaibleTile in avaibleTilesDictionary)
                            {
                                if (avaibleTile.Value && (game.PlayerTiles[avaibleTile.Key] == Tile.ship || game.PlayerTiles[avaibleTile.Key] == Tile.water))
                                {
                                    avaibleTiles.Add(avaibleTile.Key);
                                }
                            }
                            if (avaibleTiles.Count > 0)
                            {
                                break;
                            }
                        }
                    }
                    var randomIndex = random.Next(0, avaibleTiles.Count - 1);
                    ShootTool.TryShot(game, avaibleTiles[randomIndex], ShotAt.playerTile, gameEventDTOList);
                }
            } while (gameEventDTOList[gameEventDTOList.Count - 1].Tile != Tile.missed && game.PlayerTiles.ContainsValue(Tile.ship));

            return Newtonsoft.Json.JsonConvert.SerializeObject(gameEventDTOList);
        }

        public string CheckVictoryOrDefeat(int gameNumber, ShotAt shotAt)
        {
            var game = Games.FirstOrDefault(g => g.GameNumber == gameNumber);
            var tiles = shotAt == ShotAt.aiTile ? game.AITiles : game.PlayerTiles;

            if (tiles.ContainsValue(Tile.ship))
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(false);
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(true);
        }
    }
}
