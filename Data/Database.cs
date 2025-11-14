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
                    new { c="P001", n="Pintxo", p=2.5 },
                    new { c="P002", n="Txakolin (copa)", p=3.5 },
                    new { c="P003", n="Menu bazkaria", p=12.0 }
                });

                // Ensure stock rows exist (use product ids 1..3)
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (1, 100, 'Almacen')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (2, 50, 'Almacen')");
                conn.Execute("INSERT OR IGNORE INTO Stock (ProductId, Quantity, Location) VALUES (3, 25, 'Almacen')");

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