using Battleships.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Battleships.Tools
{
    public static class ShootTool
    {
        public static void TryShot(Game game, int index, ShotAt shotAt, List<GameEventDTO> gameEventDTOList)
        {
            
            if (game == null)
            {
                return;
            }

            GameEventDTO gameEventDTO = new GameEventDTO();
            gameEventDTO.Tile = shotAt == ShotAt.aiTile ? game.AITiles[index] : game.PlayerTiles[index];
            gameEventDTO.Index = index;

            var tiles = shotAt == ShotAt.aiTile ? game.AITiles : game.PlayerTiles;

            if (gameEventDTO.Tile == Tile.water)
            {
                tiles[index] = Tile.missed;
                gameEventDTO.Tile = Tile.missed;
                gameEventDTOList.Add(gameEventDTO);

            }
            else if (gameEventDTO.Tile == Tile.ship && !CheckIfDrowned(index, tiles))
            {
                tiles[index] = Tile.shot;
                gameEventDTO.Tile = Tile.shot;
                gameEventDTOList.Add(gameEventDTO);

                if (shotAt == ShotAt.playerTile)
                {
                    MarkAnavaibleDiagonalTiles(index, tiles);
                }
            }
            else if (gameEventDTO.Tile == Tile.ship && CheckIfDrowned(index, tiles))
            {
                gameEventDTO.Tile = Tile.drowned;
                gameEventDTOList.Add(gameEventDTO);

                if (shotAt == ShotAt.playerTile)
                {
                    MarkAnavaibleDiagonalTiles(index, tiles);
                }

                AddOppositeDrownedTiles(gameEventDTOList, tiles, index);
            }
        }

        public static bool CheckIfDrowned(int index, Dictionary<int, Tile> tiles)
        {
            var indexList = tiles.Select(t => t.Key).ToList();
            var oppositeTiles = LocateShipTool.AreOppositeTilesAvaible(indexList, index);

            var tilesCopy = tiles.ToDictionary(t => t.Key, t => t.Value);
            tilesCopy[index] = Tile.water;

            foreach (var item in oppositeTiles)
            {
                var trueIndex = indexList[item.Key];
                if (item.Value && tilesCopy.ContainsKey(trueIndex))
                {
                    if (tilesCopy[trueIndex] == Tile.ship)
                    {
                        return false;
                    }
                    else if (tilesCopy[trueIndex] == Tile.shot && !CheckIfDrowned(item.Key, tilesCopy))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static void AddOppositeDrownedTiles(List<GameEventDTO> gameEventDTOList, Dictionary<int, Tile> tiles, int index)
        {
            var indexList = tiles.Select(t => t.Key).ToList();
            var oppositeTiles = LocateShipTool.AreOppositeTilesAvaible(indexList, index);

            tiles[index] = Tile.drowned;

            foreach (var item in oppositeTiles)
            {
                if (item.Value && tiles.ContainsKey(item.Key) && tiles[item.Key] == Tile.shot)
                {
                    GameEventDTO oppositeGameEventDTO = new GameEventDTO();
                    oppositeGameEventDTO.Index = item.Key;
                    oppositeGameEventDTO.Tile = Tile.drowned;
                    gameEventDTOList.Add(oppositeGameEventDTO);

                    AddOppositeDrownedTiles(gameEventDTOList, tiles, oppositeGameEventDTO.Index);
                }
            }
        }

        private static void MarkAnavaibleDiagonalTiles(int index, Dictionary<int, Tile> tiles)
        {
            var diagonalAvaibility = LocateShipTool.AreDiagonalTilesAvaible(tiles.Select(t => t.Key).ToList(), index);
            foreach (var item in diagonalAvaibility)
            {
                if (item.Value)
                {
                    tiles[item.Key] = Tile.missed;
                }
            }
        }
    }
}