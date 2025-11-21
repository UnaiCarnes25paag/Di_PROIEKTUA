using Dapper;
using Erronka.Services;
using Microsoft.Data.Sqlite;
using System;
using System.IO;
using Erronka.Models;

namespace Erronka.Data
{
    public static class Database
    {
        public static string DbFile => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tpv.db");
        public static string ConnectionString => $"Data Source={DbFile};Mode=ReadWriteCreate;";

        public static void InitializeDatabase()
        {
            // Ensure folder exists
            var folder = Path.GetDirectoryName(DbFile);
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            // Open connection (file will be created if missing because of Mode=ReadWriteCreate)
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            // Create schema if missing
            conn.Execute(@"
CREATE TABLE IF NOT EXISTS Users (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Username TEXT UNIQUE NOT NULL,
  PasswordHash BLOB NOT NULL,
  PasswordSalt BLOB NOT NULL,
  Role TEXT NOT NULL,
  FullName TEXT,
  CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS Products (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Code TEXT UNIQUE,
  Name TEXT NOT NULL,
  Price REAL NOT NULL
);

CREATE TABLE IF NOT EXISTS Stock (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  ProductId INTEGER NOT NULL,
  Quantity INTEGER NOT NULL,
  Location TEXT,
  FOREIGN KEY(ProductId) REFERENCES Products(Id)
);

CREATE TABLE IF NOT EXISTS Tables (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Number INTEGER UNIQUE NOT NULL,
  Seats INTEGER
);

CREATE TABLE IF NOT EXISTS Reservations (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  TableId INTEGER NOT NULL,
  CustomerName TEXT,
  Date TEXT NOT NULL,
  TimeSlot TEXT NOT NULL,
  CreatedByUserId INTEGER,
  FOREIGN KEY(TableId) REFERENCES Tables(Id),
  FOREIGN KEY(CreatedByUserId) REFERENCES Users(Id)
);

CREATE TABLE IF NOT EXISTS Orders (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  TableId INTEGER,
  UserId INTEGER,
  CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
  Total REAL DEFAULT 0,
  Paid INTEGER DEFAULT 0,
  FOREIGN KEY(TableId) REFERENCES Tables(Id),
  FOREIGN KEY(UserId) REFERENCES Users(Id)
);

CREATE TABLE IF NOT EXISTS OrderItems (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  OrderId INTEGER NOT NULL,
  ProductId INTEGER,
  Quantity INTEGER,
  UnitPrice REAL,
  FOREIGN KEY(OrderId) REFERENCES Orders(Id),
  FOREIGN KEY(ProductId) REFERENCES Products(Id)
);
");

            // If Users table is empty, seed default data (products, stock, tables, users)
            int usersCount = conn.ExecuteScalar<int>("SELECT COUNT(1) FROM Users");
            if (usersCount == 0)
            {
                // Seed basic data: sample products
                conn.Execute("INSERT OR IGNORE INTO Products (Code, Name, Price) VALUES (@c,@n,@p)", new[] {
                // --- EXISTENTEAK ---
                new { c="P001", n="Pintxo", p=2.5 },
                new { c="P002", n="Txakolin (kopa)", p=3.5 },
                new { c="P003", n="Menu bazkaria", p=12.0 },

                // --- PINTXOAK / TAPAK ---
                new { c="P004", n="Tortilla pintxoa", p=2.2 },
                new { c="P005", n="Kroketak (unitatea)", p=1.5 },
                new { c="P006", n="Urdaiazpiko iberikozko pintxoa", p=3.0 },
                new { c="P007", n="Gilda", p=1.8 },
                new { c="P008", n="Txistor pintxoa", p=2.5 },
                new { c="P009", n="Errusiar entsalada (tapa)", p=3.0 },

                // --- FRESKOAK ---
                new { c="P010", n="Coca-Cola", p=2.2 },
                new { c="P011", n="Coca-Cola Zero", p=2.2 },
                new { c="P012", n="Fanta Laranja", p=2.2 },
                new { c="P013", n="Fanta Limon", p=2.2 },
                new { c="P014", n="Nestea", p=2.3 },
                new { c="P015", n="Ur botila", p=1.5 },
                new { c="P016", n="Tonika", p=2.2 },

                // --- GARAGARDOAK ---
                new { c="P017", n="Garagardo txikia (kaña)", p=2.0 },
                new { c="P018", n="Garagardo handia (jarra)", p=3.8 },
                new { c="P019", n="Garagardo botila", p=2.5 },
                new { c="P020", n="Radler", p=2.2 },

                // --- ARDOAK ---
                new { c="P021", n="Ardo beltza (kopa)", p=2.8 },
                new { c="P022", n="Ardo zuria (kopa)", p=2.8 },
                new { c="P023", n="Ardo arrosa (kopa)", p=2.8 },
                new { c="P024", n="Ardo beltz botila", p=12.0 },
                new { c="P025", n="Ardo zuri botila", p=11.0 },

                // --- KAFEAK ---
                new { c="P026", n="Kafe hutsa", p=1.4 },
                new { c="P027", n="Kafe esnearekin", p=1.6 },
                new { c="P028", n="Kafe ebakia", p=1.5 },
                new { c="P029", n="Kaputxinoa", p=2.0 },
                new { c="P030", n="Infusioa", p=1.6 },

                // --- HASIERAKO PLATERAK ---
                new { c="P031", n="Entsalada mistoa", p=6.5 },
                new { c="P032", n="Tomate eta ventreska entsalada", p=8.0 },
                new { c="P033", n="Eguneko zopa", p=5.0 },
                new { c="P034", n="Patata brabak", p=5.5 },
                new { c="P035", n="Kalamarrak frijituak", p=7.5 },

                // --- HARAGIAK ---
                new { c="P036", n="Hanburgesa osoa", p=9.0 },
                new { c="P037", n="Oilasko errea (azioa)", p=8.5 },
                new { c="P038", n="Txuleta xerra", p=10.5 },
                new { c="P039", n="Entrekota", p=15.0 },
                new { c="P040", n="Saiheskiak BBQ moduan", p=12.0 },

                // --- ARRANAK ---
                new { c="P041", n="Merluza plantxan", p=11.0 },
                new { c="P042", n="Bakailaoa pil-pilean", p=13.0 },
                new { c="P043", n="Txipiroiak beren tintan", p=12.5 },

                // --- PLATER KONBINATUAK ---
                new { c="P044", n="Plater konbinatua 1 (arrautza + patatak + kroketak)", p=8.5 },
                new { c="P045", n="Plater konbinatua 2 (xerra + patatak + entsalada)", p=10.0 },
                new { c="P046", n="Plater konbinatua 3 (oilaskoa + arroza + entsalada)", p=9.5 },

                // --- POSTREAK ---
                new { c="P047", n="Flan etxekoa", p=3.0 },
                new { c="P048", n="Esne-arroza", p=3.0 },
                new { c="P049", n="Gazta tarta", p=3.5 },
                new { c="P050", n="Izozkia (2 bola)", p=3.0 },

                // --- MENUA ---  
                new { c="P051", n="Haurrentzako menua", p=7.0 }
            });

                // Ensure stock rows exist (use product ids 1..3)
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (1, 100, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (2, 50, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (3, 25, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (4, 40, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (5, 60, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (6, 30, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (7, 80, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (8, 50, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (9, 45, 'Biltegia')");

                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (10, 100, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (11, 100, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (12, 90, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (13, 90, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (14, 70, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (15, 120, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (16, 50, 'Biltegia')");

                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (17, 80, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (18, 60, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (19, 70, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (20, 50, 'Biltegia')");

                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (21, 40, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (22, 40, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (23, 40, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (24, 20, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (25, 20, 'Biltegia')");

                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (26, 200, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (27, 200, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (28, 150, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (29, 120, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (30, 100, 'Biltegia')");

                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (31, 30, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (32, 25, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (33, 20, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (34, 40, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (35, 35, 'Biltegia')");

                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (36, 30, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (37, 25, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (38, 20, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (39, 15, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (40, 20, 'Biltegia')");

                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (41, 25, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (42, 20, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (43, 20, 'Biltegia')");

                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (44, 25, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (45, 25, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (46, 25, 'Biltegia')");

                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (47, 20, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (48, 20, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (49, 20, 'Biltegia')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (50, 20, 'Biltegia')");

                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (51, 10, 'Biltegia')");

                // Create some tables if none exist
                int tablesCount = conn.ExecuteScalar<int>("SELECT COUNT(1) FROM Tables");
                if (tablesCount == 0)
                {
                    for (int i = 1; i <= 8; i++)
                    {
                        conn.Execute("INSERT OR IGNORE INTO Tables (Number, Seats) VALUES (@n, @s)", new { n = i, s = 4 });
                    }
                }

                // Create default admin user and default normal user
                // passwords are plaintext here only for seed; we hash them
                var adminPassword = "admin";
                var (adminHash, adminSalt) = Erronka.Services.PasswordHelper.HashPassword(adminPassword);
                conn.Execute("INSERT INTO Users (Username, PasswordHash, PasswordSalt, Role, FullName) VALUES (@u,@h,@s,@r,@f)",
                    new { u = "admin", h = adminHash, s = adminSalt, r = "Admin", f = "Administratzailea" });

                var userPassword = "user1";
                var (userHash, userSalt) = Erronka.Services.PasswordHelper.HashPassword(userPassword);
                conn.Execute("INSERT INTO Users (Username, PasswordHash, PasswordSalt, Role, FullName) VALUES (@u,@h,@s,@r,@f)",
                    new { u = "user1", h = userHash, s = userSalt, r = "User", f = "Erabiltzaile Normala" });
            }
        }

        public static SqliteConnection GetConnection()
        {
            var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            return conn;
        }
    }
}