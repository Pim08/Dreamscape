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
        public DbSet<Item> Items { get; set; }
        public DbSet<Trade> Trades { get; set; }

        public DbSet<InventoryItem> InventoryItems { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(
                "server=localhost;database=Dreamscape;user=root;password=;",
                ServerVersion.Parse("8.0.30")
            );
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // NOTE: These passwords will be automatically converted to BCrypt hashes
            // when users first log in (see LoginPage.AttemptLogin method)
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, username = "ShadowSlayer", password_hash = "Test123!", Emailadress = "shadow@example.com", Role = User.ROLE_USER },
                new User { Id = 2, username = "MysticMage", password_hash = "Mage2024", Emailadress = "mystic@example.com", Role = User.ROLE_USER },
                new User { Id = 3, username = "DragonKnight", password_hash = "Dragon!99", Emailadress = "dragon@example.com", Role = User.ROLE_USER },
                new User { Id = 4, username = "AdminMaster", password_hash = "Admin007", Emailadress = "admin@example.com", Role = User.ROLE_OWNER },
                new User { Id = 5, username = "ThunderRogue", password_hash = "Thund3r!!", Emailadress = "thunder@example.com", Role = User.ROLE_USER }
            );

            modelBuilder.Entity<Item>().HasData(
                new Item { Id = 101, Naam = "Zwaard des Vuur", Beschrijving = "Een mythisch zwaard met een vlammende gloed.", Type = "Wapen", Zeldzaamheid = "Legendarisch", Kracht = 90, Snelheid = 60, Duurzaamheid = 80, MagischeEigenschap = "+30% vuurschade" },
                new Item { Id = 102, Naam = "Ijs Amulet", Beschrijving = "Een amulet dat beschermt tegen kou.", Type = "Accessoire", Zeldzaamheid = "Episch", Kracht = 20, Snelheid = 10, Duurzaamheid = 70, MagischeEigenschap = "+25% weerstand tegen ijs" },
                new Item { Id = 103, Naam = "Schaduw Mantel", Beschrijving = "Mantel die je bewegingen verbergt.", Type = "Armor", Zeldzaamheid = "Zeldzaam", Kracht = 40, Snelheid = 85, Duurzaamheid = 50, MagischeEigenschap = "+15% ontwijk kans" },
                new Item { Id = 104, Naam = "Hamer der Titanen", Beschrijving = "Massieve hamer met aardekracht.", Type = "Wapen", Zeldzaamheid = "Legendarisch", Kracht = 95, Snelheid = 40, Duurzaamheid = 90, MagischeEigenschap = "Kan vijanden verdoven" },
                new Item { Id = 105, Naam = "Lichtboog", Beschrijving = "Boog die energie pijlen schiet.", Type = "Wapen", Zeldzaamheid = "Episch", Kracht = 85, Snelheid = 75, Duurzaamheid = 60, MagischeEigenschap = "+10% kritieke kans" },
                new Item { Id = 106, Naam = "Helende Ring", Beschrijving = "Ring die gezondheid herstelt.", Type = "Accessoire", Zeldzaamheid = "Zeldzaam", Kracht = 10, Snelheid = 5, Duurzaamheid = 100, MagischeEigenschap = "+5 HP per seconde" },
                new Item { Id = 107, Naam = "Demonen Harnas", Beschrijving = "Duister harnas met enorme kracht.", Type = "Armor", Zeldzaamheid = "Legendarisch", Kracht = 75, Snelheid = 50, Duurzaamheid = 95, MagischeEigenschap = "Absorbeert 20% schade" }
            );
            modelBuilder.Entity<InventoryItem>().HasData(

    new InventoryItem { Id = 1, UserId = 1, ItemId = 101, Quantity = 1 },
    new InventoryItem { Id = 2, UserId = 1, ItemId = 106, Quantity = 2 },

    new InventoryItem { Id = 3, UserId = 2, ItemId = 102, Quantity = 1 },

    new InventoryItem { Id = 4, UserId = 3, ItemId = 104, Quantity = 1 },

    new InventoryItem { Id = 5, UserId = 5, ItemId = 105, Quantity = 1 }

);
        }

    }
}