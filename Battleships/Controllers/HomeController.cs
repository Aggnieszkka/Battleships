using Battleships.Models;
using Battleships.Tools;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Battleships.Controllers
{
    public class HomeController : Controller
    {
        private DatabaseContext _database = new DatabaseContext();

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
        public string StartGame(List<int> shipArray)
        {
            var game = _database.Games.Add(new Game());

            List<int> aiShips = LocateShipTool.GetRandomShips();

            List<Tile> tiles = new List<Tile>();

            //add player ship tiles to database
            foreach (var index in shipArray)
            {
                tiles.Add(new Tile
                {
                    Number = index,
                    Type = TileType.ship,
                    GameId = game.Id,
                    Owner = ShotAt.playerTile
                });
            }

            //add AI ship tiles to database
            foreach (var index in aiShips)
            {
                tiles.Add(new Tile
                {
                    Number = index,
                    Type = TileType.ship,
                    GameId = game.Id,
                    Owner = ShotAt.aiTile
                });
            }
            _database.Tiles.AddRange(tiles);
            _database.SaveChanges();
            return Newtonsoft.Json.JsonConvert.SerializeObject(game.Id);
        }
        public string ShootRandomly(int gameNumber)
        {
            var game = _database.Games.FirstOrDefault(g => g.Id == gameNumber);
            List<GameEventDTO> gameEventDTOList = new List<GameEventDTO>();

            if (game != null)
            {
                game.Sequence++;
                var availableTiles = game.AITiles.Where(t => t.Value.Type == TileType.water || t.Value.Type == TileType.ship).Select(t => t.Key).ToList();
                if (availableTiles.Count > 0)
                {
                    Random random = new Random(Guid.NewGuid().GetHashCode());
                    var randomIndex = random.Next(0, availableTiles.Count - 1);
                    ShootTool.TryShot(_database, game, availableTiles[randomIndex], ShotAt.aiTile, gameEventDTOList);

                    _database.Entry(game).State = EntityState.Modified;
                    _database.SaveChanges();
                }
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(gameEventDTOList);
        }
        public string Shoot(int gameNumber, int index)
        {
            var game = _database.Games.FirstOrDefault(g => g.Id == gameNumber);
            List<GameEventDTO> gameEventDTOList = new List<GameEventDTO>();
            if (game != null && (game.AITiles[index].Type == TileType.water || game.AITiles[index].Type == TileType.ship))
            {
                game.Sequence++;
                ShootTool.TryShot(_database, game, index, ShotAt.aiTile, gameEventDTOList);

                _database.Entry(game).State = EntityState.Modified;
                _database.SaveChanges();
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(gameEventDTOList);
        }
        public string AIShot(int gameNumber)
        {
            List<GameEventDTO> gameEventDTOList = new List<GameEventDTO>();
            var game = _database.Games.FirstOrDefault(g => g.Id == gameNumber);
            if (game != null)
            {
                var tiles = game.PlayerTiles.Select(t => t.Key).ToList();

                Random random = new Random(Guid.NewGuid().GetHashCode());
                List<int> availableTiles = new List<int>();

                do
                {
                    availableTiles = game.PlayerTiles
                                    .Where(t => t.Value.Type == TileType.water || t.Value.Type == TileType.ship)
                                    .Select(t => t.Key)
                                    .ToList();

                    if (availableTiles.Count > 0)
                    {
                        //If there are shot player tiles make only opposite tiles avaible
                        if (game.PlayerTiles.Any(t => t.Value.Type == TileType.shot))
                        {
                            foreach (var playerTile in game.PlayerTiles.Where(p => p.Value.Type == TileType.shot))
                            {
                                var index = playerTile.Key;
                                availableTiles = new List<int>();

                                var avaibleTilesDictionary = LocateShipTool.AreOppositeTilesAvailable(tiles, index);
                                foreach (var availableTile in avaibleTilesDictionary)
                                {
                                    if (availableTile.Value && (game.PlayerTiles[availableTile.Key].Type == TileType.ship || game.PlayerTiles[availableTile.Key].Type == TileType.water))
                                    {
                                        availableTiles.Add(availableTile.Key);
                                    }
                                }
                                if (availableTiles.Count > 0)
                                {
                                    break;
                                }
                            }
                        }
                        var randomIndex = random.Next(0, availableTiles.Count - 1);
                        ShootTool.TryShot(_database, game, availableTiles[randomIndex], ShotAt.playerTile, gameEventDTOList);
                    }
                } while (gameEventDTOList[gameEventDTOList.Count - 1].Tile != TileType.missed && game.PlayerTiles.Any(t => t.Value.Type == TileType.ship));

                _database.SaveChanges();
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(gameEventDTOList);
        }

        public string CheckVictoryOrDefeat(int gameNumber, ShotAt shotAt)
        {
            var game = _database.Games.FirstOrDefault(g => g.Id == gameNumber);
            if (game == null)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(false);
            }

            var tiles = shotAt == ShotAt.aiTile ? game.AITiles : game.PlayerTiles;

            if (tiles.Any(t => t.Value.Type == TileType.ship))
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(false);
            }
            game.FinishDate = DateTime.Now;

            _database.Entry(game).State = EntityState.Modified;
            _database.SaveChanges();

            return Newtonsoft.Json.JsonConvert.SerializeObject(true);
        }
        public string GetRankingRows()
        {
            var games = _database.Games.ToList();
            List<Game> wonAndFinishedGames = games.Where(g => !g.AITiles.Any(t => t.Value.Type == TileType.ship)).ToList();
            List<RankingRowDTO> sortedRows = wonAndFinishedGames.OrderBy(g => g.Sequence)
                                                                .ThenBy(g => g.FinishDate - g.Startdate)
                                                                .ThenBy(g => g.Startdate)
                                                                .Take(8)
                                                                .Select(g => new RankingRowDTO(g))
                                                                .ToList();
            return Newtonsoft.Json.JsonConvert.SerializeObject(sortedRows);
        }
        public string QualifyForRanking(int gameNumber)
        {
            var games = _database.Games.ToList();
            var game = games.FirstOrDefault(g => g.Id == gameNumber);

            List<Game> wonAndFinishedGames = games.Where(g => !g.AITiles.Any(t => t.Value.Type == TileType.ship)).ToList();
            List<Game> sortedGames = wonAndFinishedGames.OrderBy(g => g.Sequence)
                                                                .ThenBy(g => g.FinishDate - g.Startdate)
                                                                .ThenBy(g => g.Startdate)
                                                                .Take(8)
                                                                .ToList();
            bool qualified = true;

            if (sortedGames.Count < 8)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(qualified);
            }
            foreach (var rankingGame in sortedGames)
            {
                if (rankingGame.Sequence > game.Sequence)
                {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(qualified);
                }
                else if (rankingGame.Sequence == game.Sequence)
                {
                    if ((rankingGame.FinishDate - rankingGame.Startdate) < (game.FinishDate - game.Startdate))
                    {
                        return Newtonsoft.Json.JsonConvert.SerializeObject(qualified);
                    }
                }
            }

            qualified = false;
            return Newtonsoft.Json.JsonConvert.SerializeObject(qualified);
        }
        public void SetNickname(int gameNumber, string nickname)
        {
            if (!string.IsNullOrEmpty(nickname))
            {
                var game = _database.Games.FirstOrDefault(g => g.Id == gameNumber);
                game.PlayerNick = nickname;

                _database.Entry(game).State = EntityState.Modified;
                _database.SaveChanges();
            }
        }

        public List<int> availableTiles { get; set; }
    }
}
