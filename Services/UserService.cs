using Dapper;
using Erronka.Data;
using Erronka.Models;
using System.Collections.Generic;

namespace Erronka.Services
{
    public class UserService
    {
        public IEnumerable<User> GetAllUsers()
        {
            using var conn = DatabaseService.GetConnection();
            return conn.Query<User>("SELECT * FROM Users ORDER BY Username");
        }

        public User? GetUserById(int id)
        {
            using var conn = DatabaseService.GetConnection();
            return conn.QuerySingleOrDefault<User>("SELECT * FROM Users WHERE Id = @id", new { id });
        }

        public void AddUser(User user)
        {
            using var conn = DatabaseService.GetConnection();
            conn.Execute(
                "INSERT INTO Users (Username, PasswordHash, PasswordSalt, Role, FullName) VALUES(@Username, @PasswordHash, @PasswordSalt, @Role, @FullName)",
                user
            );
        }

        public void UpdateUser(User user)
        {
            using var conn = DatabaseService.GetConnection();
            conn.Execute(
                "UPDATE Users SET Username = @Username, PasswordHash = @PasswordHash, PasswordSalt = @PasswordSalt, Role = @Role, FullName = @FullName WHERE Id = @Id",
                user
            );
        }

        public void DeleteUser(int id)
        {
            using var conn = DatabaseService.GetConnection();
            conn.Execute("DELETE FROM Users WHERE Id = @id", new { id });
        }
    }
}
