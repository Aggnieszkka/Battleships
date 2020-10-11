using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Battleships.Tools
{
    public static class LocateShipTool
    {
        public static List<int> GetRandomShips()
        {
            List<int> avaibleIndexList = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                avaibleIndexList.Add(i);
            }
            List<int> drawnIndexList = new List<int>();

            //build 3 ships of 2 tiles
            while (drawnIndexList.Count < 6)
            {
                DrawShips(avaibleIndexList, drawnIndexList, 2);
            }
            //build 2 ships of 3 tiles
            while (drawnIndexList.Count < 12)
            {
                DrawShips(avaibleIndexList, drawnIndexList, 3);
            }
            //build 1 ship of 4 tiles
            while (drawnIndexList.Count < 16)
            {
                DrawShips(avaibleIndexList, drawnIndexList, 4);
            }
            //build 4 ships of 1 tile
            while (drawnIndexList.Count < 20)
            {
                DrawShips(avaibleIndexList, drawnIndexList, 1);
            }
            return drawnIndexList;
        }

        public static void DrawShips(List<int> avaibleIndexList, List<int> drawnIndexList, int shipSize)
        {
            List<int> demoDrawnIndexList = new List<int>(); //for removing items from avaibleIndexList
            Random random = new Random(Guid.NewGuid().GetHashCode());

            int drawnIndex = random.Next(0, avaibleIndexList.Count - 1);
            int expandSideNumber = random.Next(0, 3);

            switch (expandSideNumber)
            {
                //expand left
                case 0:
                    if (MayExpandHorizontally(avaibleIndexList, drawnIndex, -1, shipSize, 9))
                    {
                        demoDrawnIndexList = AddTiles(avaibleIndexList, drawnIndex, -1, shipSize);
                    }
                    break;
                //expand right
                case 1:
                    if (MayExpandHorizontally(avaibleIndexList, drawnIndex, 1, shipSize, 0))
                    {
                        demoDrawnIndexList = AddTiles(avaibleIndexList, drawnIndex, 1, shipSize);
                    }
                    break;
                //expand up
                case 2:
                    if (MayExpandVertically(avaibleIndexList, drawnIndex, -10, shipSize))
                    {
                        demoDrawnIndexList = AddTiles(avaibleIndexList, drawnIndex, -10, shipSize);
                    }
                    break;
                //expand down
                case 3:
                    if (MayExpandVertically(avaibleIndexList, drawnIndex, 10, shipSize))
                    {
                        demoDrawnIndexList = AddTiles(avaibleIndexList, drawnIndex, 10, shipSize);
                    }
                    break;
            }
            //delete used indexes from avaibleIndexList and avaible indexes around them
            if (demoDrawnIndexList.Count > 0)
            {
                HashSet<int> toRemove = new HashSet<int>();
                foreach (var value in demoDrawnIndexList)
                {
                    Dictionary<int, bool> tilesAvaibility = AreTilesAvaible(avaibleIndexList, avaibleIndexList.IndexOf(value));

                    toRemove.Add(avaibleIndexList[avaibleIndexList.IndexOf(value)]);

                    foreach (var item in tilesAvaibility)
                    {
                        if (item.Value)
                        {
                            toRemove.Add(avaibleIndexList[item.Key]);
                        }
                    }
                    drawnIndexList.Add(value);
                }
                foreach (var value in toRemove.OrderByDescending(x => x))
                {
                    avaibleIndexList.RemoveAt(avaibleIndexList.IndexOf(value));
                }
                demoDrawnIndexList = new List<int>();
            }
        }

        public static bool MayExpandHorizontally(List<int> avaibleIndexList, int drawnIndex, int direction, int numberOfTiles, int moduloOutcome)
        {
            if (!MayExpandVertically(avaibleIndexList, drawnIndex, direction, numberOfTiles))
            {
                return false;
            }
            for (int i = 1; i < numberOfTiles; i++) //check if spots are next to each other when expanding horizontally
            {
                if ((avaibleIndexList[drawnIndex] + (direction * i)) % 10 == moduloOutcome)
                {
                    return false;
                }
            }
            return true;
        }
        public static bool MayExpandVertically(List<int> avaibleIndexList, int drawnIndex, int direction, int numberOfTiles)
        {
            for (int i = 1; i < numberOfTiles; i++) //check if next spots are avaible
            {
                int checkValue = avaibleIndexList[drawnIndex] + (direction * i);
                if (
                    !IsValidIndex(checkValue)
                    || !avaibleIndexList.Contains(checkValue)
                    )
                {
                    return false;
                }
            }
            return true;
        }
        public static bool IsValidIndex(int index)
        {
            if (index < 0 || index > 99)
            {
                return false;
            }
            return true;
        }
        public static List<int> AddTiles(List<int> avaibleIndexList, int drawnIndex, int direction, int numberOfTiles)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < numberOfTiles; i++)
            {
                list.Add(avaibleIndexList[drawnIndex] + (direction * i));
            }
            return list;
        }

        public static Dictionary<int, bool> AreTilesAvaible(List<int> avaibleIndexList, int index)
        {
            var tilesAvaibility = AreOppositeTilesAvaible (avaibleIndexList, index);
            var diagonalTilesAvaibility = AreDiagonalTilesAvaible (avaibleIndexList, index);

            foreach (var item in diagonalTilesAvaibility)
            {
                tilesAvaibility.Add(item.Key, item.Value);
            }

            return tilesAvaibility;
        }

        public static Dictionary<int, bool> AreOppositeTilesAvaible(List<int> avaibleIndexList, int index)
        {
            bool left = MayExpandHorizontally(avaibleIndexList, index, -1, 2, 9);
            bool right = MayExpandHorizontally(avaibleIndexList, index, 1, 2, 0);
            bool up = MayExpandVertically(avaibleIndexList, index, -10, 2);
            bool down = MayExpandVertically(avaibleIndexList, index, 10, 2);

            Dictionary<int, bool> tilesAvaibility = new Dictionary<int, bool>();

            var value = avaibleIndexList[index];

            if (avaibleIndexList.Contains(value + 10))
                tilesAvaibility.Add(avaibleIndexList.IndexOf(value + 10), down);
            if (avaibleIndexList.Contains(value + 1))
                tilesAvaibility.Add(avaibleIndexList.IndexOf(value + 1), right);

            if (avaibleIndexList.Contains(value - 1))
                tilesAvaibility.Add(avaibleIndexList.IndexOf(value - 1), left);
            if (avaibleIndexList.Contains(value - 10))
                tilesAvaibility.Add(avaibleIndexList.IndexOf(value - 10), up);

            return tilesAvaibility;
        }

        public static Dictionary<int, bool> AreDiagonalTilesAvaible(List<int> avaibleIndexList, int index)
        {
            bool leftUp = MayExpandHorizontally(avaibleIndexList, index, -11, 2, 9);
            bool rightUp = MayExpandHorizontally(avaibleIndexList, index, -9, 2, 0);
            bool leftDown = MayExpandHorizontally(avaibleIndexList, index, 9, 2, 9);
            bool rightDown = MayExpandHorizontally(avaibleIndexList, index, 11, 2, 0);

            Dictionary<int, bool> tilesAvaibility = new Dictionary<int, bool>();

            var value = avaibleIndexList[index];

            if (avaibleIndexList.Contains(value + 11))
                tilesAvaibility.Add(avaibleIndexList.IndexOf(value + 11), rightDown);
            if (avaibleIndexList.Contains(value + 9))
                tilesAvaibility.Add(avaibleIndexList.IndexOf(value + 9), leftDown);

            if (avaibleIndexList.Contains(value - 9))
                tilesAvaibility.Add(avaibleIndexList.IndexOf(value - 9), rightUp);
            if (avaibleIndexList.Contains(value - 11))
                tilesAvaibility.Add(avaibleIndexList.IndexOf(value - 11), leftUp);

            return tilesAvaibility;
        }
    }
}