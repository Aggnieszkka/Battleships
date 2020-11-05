using Battleships.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Battleships.Tools
{
    public static class ShootTool
    {
        public static void TryShot(DbContext database, Game game, int index, ShotAt shotAt, List<GameEventDTO> gameEventDTOList)
        {
            if (game == null)
            {
                return;
            }

            GameEventDTO gameEventDTO = new GameEventDTO();
            SetEventTime(game, gameEventDTO);
            gameEventDTO.GameNumber = game.Id;
            gameEventDTO.Sequence = game.Sequence;
            gameEventDTO.Tile = shotAt == ShotAt.aiTile ? game.AITiles[index].Type : game.PlayerTiles[index].Type;
            gameEventDTO.Index = index;

            var tiles = shotAt == ShotAt.aiTile ? game.AITiles : game.PlayerTiles;

            if (gameEventDTO.Tile == TileType.water)
            {
                tiles[index].Type = TileType.missed;
                database.Entry(tiles[index]).State = EntityState.Added;

                gameEventDTO.Tile = TileType.missed;
                gameEventDTOList.Add(gameEventDTO);

            }
            else if (gameEventDTO.Tile == TileType.ship && !CheckIfDrowned(index, tiles))
            {
                tiles[index].Type = TileType.shot;
                database.Entry(tiles[index]).State = EntityState.Modified;

                gameEventDTO.Tile = TileType.shot;
                gameEventDTOList.Add(gameEventDTO);

                //If player tile was hit make diagonal tiles unavaible for smarter AI shots
                if (shotAt == ShotAt.playerTile)
                {
                    MarkAnavailableDiagonalTiles(database, index, tiles);
                }
            }
            else if (gameEventDTO.Tile == TileType.ship && CheckIfDrowned(index, tiles))
            {
                gameEventDTO.Tile = TileType.drowned;
                gameEventDTOList.Add(gameEventDTO);

                //If player tile was hit make diagonal tiles unavaible for smarter AI shots
                if (shotAt == ShotAt.playerTile)
                {
                    MarkAnavailableDiagonalTiles(database, index, tiles);
                }

                AddOppositeDrownedTiles(database, gameEventDTOList, tiles, index, shotAt, game.Id);
            }
        }

        public static bool CheckIfDrowned(int index, Dictionary<int, Tile> tiles)
        {
            var indexList = tiles.Select(t => t.Key).ToList();
            var oppositeTiles = LocateShipTool.AreOppositeTilesAvailable(indexList, index);

            var tilesCopy = tiles.ToDictionary(t => t.Key, t => new Tile{ Type = t.Value.Type });
            tilesCopy[index].Type = TileType.water;

            foreach (var item in oppositeTiles)
            {
                var trueIndex = indexList[item.Key];
                if (item.Value && tilesCopy.ContainsKey(trueIndex))
                {
                    if (tilesCopy[trueIndex].Type == TileType.ship)
                    {
                        return false;
                    }
                    else if (tilesCopy[trueIndex].Type == TileType.shot && !CheckIfDrowned(item.Key, tilesCopy))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static void AddOppositeDrownedTiles(DbContext database, List<GameEventDTO> gameEventDTOList, Dictionary<int, Tile> tiles, int index, ShotAt shotAt, int gameNumber)
        {
            var indexList = tiles.Select(t => t.Key).ToList();
            var oppositeTiles = LocateShipTool.AreOppositeTilesAvailable(indexList, index);

            tiles[index].Type = TileType.drowned;
            database.Entry(tiles[index]).State = EntityState.Modified;

            foreach (var item in oppositeTiles)
            {
                //find already shot tile next to drowned tile
                if (item.Value && tiles.ContainsKey(item.Key) && tiles[item.Key].Type == TileType.shot)
                {
                    //change shot tile to drowned tile and add to list of events
                    GameEventDTO oppositeGameEventDTO = new GameEventDTO();
                    oppositeGameEventDTO.IsVisible = false;
                    oppositeGameEventDTO.GameNumber = gameNumber;
                    oppositeGameEventDTO.Index = item.Key;
                    oppositeGameEventDTO.Tile = TileType.drowned;
                    gameEventDTOList.Add(oppositeGameEventDTO);

                    //if there are any water tiles around mark them as missed for smarter AI shots
                    if (shotAt == ShotAt.playerTile)
                    {
                        var tilesToMark = LocateShipTool.AreOppositeTilesAvailable(indexList, item.Key);
                        foreach (var tile in tilesToMark)
                        {
                            if (tile.Value && tiles[tile.Key].Type == TileType.water)
                            {
                                tiles[tile.Key].Type = TileType.missed;
                                database.Entry(tiles[tile.Key]).State = EntityState.Added;
                            }
                        }
                    }

                    AddOppositeDrownedTiles(database, gameEventDTOList, tiles, oppositeGameEventDTO.Index, shotAt, gameNumber);
                }
            }
            //if it is one-tile ship mark water tiles around as missed for smarter AI shots
            if (!tiles.Any(t => t.Value.Type == TileType.shot && oppositeTiles.Any(o => o.Key == t.Key && o.Value)))
            {
                foreach (var tile in oppositeTiles)
                {
                    if (tile.Value && tiles[tile.Key].Type == TileType.water)
                    {
                        tiles[tile.Key].Type = TileType.missed;
                        database.Entry(tiles[tile.Key]).State = EntityState.Added;
                    }
                }
            }
        }

        public static void MarkAnavailableDiagonalTiles(DbContext database, int index, Dictionary<int, Tile> tiles)
        {
            var diagonalAvailability = LocateShipTool.AreDiagonalTilesAvailable(tiles.Select(t => t.Key).ToList(), index);
            foreach (var item in diagonalAvailability)
            {
                if (item.Value && tiles[item.Key].Type == TileType.water)
                {
                    tiles[item.Key].Type = TileType.missed;
                    database.Entry(tiles[item.Key]).State = EntityState.Added;
                }
            }
        }

        public static void SetEventTime(Game game, GameEventDTO gameEventDTO)
        {
            DateTime eventDate = DateTime.Now;
            DateTime startDate = game.Startdate;
            string time = GetTimeString(startDate, eventDate);
            gameEventDTO.Time = time;
        }

        public static string GetTimeString(DateTime startDate, DateTime endDate)
        {
            string time = "";
            string hour = (endDate - startDate).Hours.ToString("00");
            string minute = (endDate - startDate).Minutes.ToString("00");
            string second = (endDate - startDate).Seconds.ToString("00");

            if (hour == "00")
            {
                time = minute + " : " + second;
            }
            else
            {
                time = hour + " : " + minute + " : " + second;
            }
            return time;
        }
    }
}