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
            List<int> availableIndexList = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                availableIndexList.Add(i);
            }
            List<int> drawnIndexList = new List<int>();

            //build 3 ships of 2 tiles
            while (drawnIndexList.Count < 6)
            {
                DrawShips(availableIndexList, drawnIndexList, 2);
            }
            //build 2 ships of 3 tiles
            while (drawnIndexList.Count < 12)
            {
                DrawShips(availableIndexList, drawnIndexList, 3);
            }
            //build 1 ship of 4 tiles
            while (drawnIndexList.Count < 16)
            {
                DrawShips(availableIndexList, drawnIndexList, 4);
            }
            //build 4 ships of 1 tile
            while (drawnIndexList.Count < 20)
            {
                DrawShips(availableIndexList, drawnIndexList, 1);
            }
            return drawnIndexList;
        }

        public static void DrawShips(List<int> availableIndexList, List<int> drawnIndexList, int shipSize)
        {
            List<int> demoDrawnIndexList = new List<int>(); //for removing items from avaibleIndexList
            Random random = new Random(Guid.NewGuid().GetHashCode());

            int drawnIndex = random.Next(0, availableIndexList.Count - 1);
            int expandSideNumber = random.Next(0, 3);

            switch (expandSideNumber)
            {
                //expand left
                case 0:
                    if (MayExpandHorizontally(availableIndexList, drawnIndex, -1, shipSize, 9))
                    {
                        demoDrawnIndexList = AddTiles(availableIndexList, drawnIndex, -1, shipSize);
                    }
                    break;
                //expand right
                case 1:
                    if (MayExpandHorizontally(availableIndexList, drawnIndex, 1, shipSize, 0))
                    {
                        demoDrawnIndexList = AddTiles(availableIndexList, drawnIndex, 1, shipSize);
                    }
                    break;
                //expand up
                case 2:
                    if (MayExpandVertically(availableIndexList, drawnIndex, -10, shipSize))
                    {
                        demoDrawnIndexList = AddTiles(availableIndexList, drawnIndex, -10, shipSize);
                    }
                    break;
                //expand down
                case 3:
                    if (MayExpandVertically(availableIndexList, drawnIndex, 10, shipSize))
                    {
                        demoDrawnIndexList = AddTiles(availableIndexList, drawnIndex, 10, shipSize);
                    }
                    break;
            }
            //delete used indexes from avaibleIndexList and avaible indexes around them
            if (demoDrawnIndexList.Count > 0)
            {
                HashSet<int> toRemove = new HashSet<int>();
                foreach (var value in demoDrawnIndexList)
                {
                    Dictionary<int, bool> tilesAvailability = AreTilesAvailable(availableIndexList, availableIndexList.IndexOf(value));

                    toRemove.Add(availableIndexList[availableIndexList.IndexOf(value)]);

                    foreach (var item in tilesAvailability)
                    {
                        if (item.Value)
                        {
                            toRemove.Add(availableIndexList[item.Key]);
                        }
                    }
                    drawnIndexList.Add(value);
                }
                foreach (var value in toRemove.OrderByDescending(x => x))
                {
                    availableIndexList.RemoveAt(availableIndexList.IndexOf(value));
                }
                demoDrawnIndexList = new List<int>();
            }
        }

        public static bool MayExpandHorizontally(List<int> availableIndexList, int drawnIndex, int direction, int numberOfTiles, int moduloOutcome)
        {
            if (!MayExpandVertically(availableIndexList, drawnIndex, direction, numberOfTiles))
            {
                return false;
            }
            for (int i = 1; i < numberOfTiles; i++) //check if spots are next to each other when expanding horizontally
            {
                if ((availableIndexList[drawnIndex] + (direction * i)) % 10 == moduloOutcome)
                {
                    return false;
                }
            }
            return true;
        }
        public static bool MayExpandVertically(List<int> availableIndexList, int drawnIndex, int direction, int numberOfTiles)
        {
            for (int i = 1; i < numberOfTiles; i++) //check if next spots are avaible
            {
                int checkValue = availableIndexList[drawnIndex] + (direction * i);
                if (
                    !IsValidIndex(checkValue)
                    || !availableIndexList.Contains(checkValue)
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
        public static List<int> AddTiles(List<int> availableIndexList, int drawnIndex, int direction, int numberOfTiles)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < numberOfTiles; i++)
            {
                list.Add(availableIndexList[drawnIndex] + (direction * i));
            }
            return list;
        }

        public static Dictionary<int, bool> AreTilesAvailable(List<int> availableIndexList, int index)
        {
            var tilesAvailability = AreOppositeTilesAvailable (availableIndexList, index);
            var diagonalTilesAvailability = AreDiagonalTilesAvailable (availableIndexList, index);

            foreach (var item in diagonalTilesAvailability)
            {
                tilesAvailability.Add(item.Key, item.Value);
            }

            return tilesAvailability;
        }

        public static Dictionary<int, bool> AreOppositeTilesAvailable(List<int> availableIndexList, int index)
        {
            bool left = MayExpandHorizontally(availableIndexList, index, -1, 2, 9);
            bool right = MayExpandHorizontally(availableIndexList, index, 1, 2, 0);
            bool up = MayExpandVertically(availableIndexList, index, -10, 2);
            bool down = MayExpandVertically(availableIndexList, index, 10, 2);

            Dictionary<int, bool> tilesAvailability = new Dictionary<int, bool>();

            var value = availableIndexList[index];

            if (availableIndexList.Contains(value + 10))
                tilesAvailability.Add(availableIndexList.IndexOf(value + 10), down);
            if (availableIndexList.Contains(value + 1))
                tilesAvailability.Add(availableIndexList.IndexOf(value + 1), right);

            if (availableIndexList.Contains(value - 1))
                tilesAvailability.Add(availableIndexList.IndexOf(value - 1), left);
            if (availableIndexList.Contains(value - 10))
                tilesAvailability.Add(availableIndexList.IndexOf(value - 10), up);

            return tilesAvailability;
        }

        public static Dictionary<int, bool> AreDiagonalTilesAvailable(List<int> availableIndexList, int index)
        {
            bool leftUp = MayExpandHorizontally(availableIndexList, index, -11, 2, 9);
            bool rightUp = MayExpandHorizontally(availableIndexList, index, -9, 2, 0);
            bool leftDown = MayExpandHorizontally(availableIndexList, index, 9, 2, 9);
            bool rightDown = MayExpandHorizontally(availableIndexList, index, 11, 2, 0);

            Dictionary<int, bool> tilesAvailability = new Dictionary<int, bool>();

            var value = availableIndexList[index];

            if (availableIndexList.Contains(value + 11))
                tilesAvailability.Add(availableIndexList.IndexOf(value + 11), rightDown);
            if (availableIndexList.Contains(value + 9))
                tilesAvailability.Add(availableIndexList.IndexOf(value + 9), leftDown);

            if (availableIndexList.Contains(value - 9))
                tilesAvailability.Add(availableIndexList.IndexOf(value - 9), rightUp);
            if (availableIndexList.Contains(value - 11))
                tilesAvailability.Add(availableIndexList.IndexOf(value - 11), leftUp);

            return tilesAvailability;
        }
    }
}