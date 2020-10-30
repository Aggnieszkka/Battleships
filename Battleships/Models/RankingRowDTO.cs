using Battleships.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Battleships.Models
{
    public class RankingRowDTO
    {
        public RankingRowDTO(Game game)
        {
            Shots = game.Sequence;
            Time = ShootTool.GetTimeString(game.Startdate, game.FinishDate);
            Date = game.Startdate.ToShortDateString();
            Nick = game.PlayerNick;
        } 
        public string Nick { get; set; }
        public int Shots { get; set; }
        public string Time { get; set; }
        public string Date { get; set; }
    }
}