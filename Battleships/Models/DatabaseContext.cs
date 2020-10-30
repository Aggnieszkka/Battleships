using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Battleships.Models
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext()
            : base ("DefaultConnection")
        {
            
        }
        public DbSet<Game> Games { get; set; }
        public DbSet<Tile> Tiles { get; set; }

    }
}