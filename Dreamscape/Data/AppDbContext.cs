using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Dreamscape.Data
{


    class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(
                "server=localhost;database=Dreamscape;user=root;password=;",
                ServerVersion.Parse("8.0.30")
            );
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasData(

                new User
                {
                    Id = 1,
                    username = "owner",
                    password_hash = "owner123", // plain-text
                    Emailadress = "owner@dreamscape.nl",
                    Role = User.ROLE_OWNER
                },

                new User
                {
                    Id = 2,
                    username = "pim",
                    password_hash = "welkom123", // plain-text
                    Emailadress = "pim@dreamscape.nl",
                    Role = User.ROLE_OWNER
                },

                new User
                {
                    Id = 3,
                    username = "player1",
                    password_hash = "player123", // plain-text
                    Emailadress = "player1@mail.com",
                    Role = User.ROLE_USER
                },

                new User
                {
                    Id = 4,
                    username = "player2",
                    password_hash = "player456", // plain-text
                    Emailadress = "player2@mail.com",
                    Role = User.ROLE_USER
                }
            );
        }
    }
}
