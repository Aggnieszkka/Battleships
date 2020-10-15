﻿using Battleships.Models;
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
            gameEventDTO.Sequence = game.Sequence;
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

                //If player tile was hit make diagonal tiles unavaible
                if (shotAt == ShotAt.playerTile)
                {
                    MarkAnavaibleDiagonalTiles(index, tiles);
                }

                AddOppositeDrownedTiles(gameEventDTOList, tiles, index, shotAt);
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

        public static void AddOppositeDrownedTiles(List<GameEventDTO> gameEventDTOList, Dictionary<int, Tile> tiles, int index, ShotAt shotAt)
        {
            var indexList = tiles.Select(t => t.Key).ToList();
            var oppositeTiles = LocateShipTool.AreOppositeTilesAvaible(indexList, index);

            tiles[index] = Tile.drowned;

            foreach (var item in oppositeTiles)
            {
                //find already shot tile next to drowned tile
                if (item.Value && tiles.ContainsKey(item.Key) && tiles[item.Key] == Tile.shot)
                {
                    //change shot tile to drowned tile and add to list of events
                    GameEventDTO oppositeGameEventDTO = new GameEventDTO();
                    oppositeGameEventDTO.IsVisible = false;
                    oppositeGameEventDTO.Index = item.Key;
                    oppositeGameEventDTO.Tile = Tile.drowned;
                    gameEventDTOList.Add(oppositeGameEventDTO);

                    //if there are any water tiles around mark them as missed for smarter AI shots
                    if (shotAt == ShotAt.playerTile)
                    {
                        var tilesToMark = LocateShipTool.AreOppositeTilesAvaible(indexList, item.Key);
                        foreach (var tile in tilesToMark)
                        {
                            if (tile.Value && tiles[tile.Key] == Tile.water)
                            {
                                tiles[tile.Key] = Tile.missed;
                            }
                        }
                    }

                    AddOppositeDrownedTiles(gameEventDTOList, tiles, oppositeGameEventDTO.Index, shotAt);
                }
            }
            //if it is one-tile ship mark water tiles around as missed
            if (!tiles.Any(t => t.Value == Tile.shot && oppositeTiles.Any(o => o.Key == t.Key && o.Value)))
            {
                foreach (var tile in oppositeTiles)
                {
                    if (tile.Value && tiles[tile.Key] == Tile.water)
                    {
                        tiles[tile.Key] = Tile.missed;
                    }
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