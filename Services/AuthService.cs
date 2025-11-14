using Dapper;
using Erronka.Models;
using Erronka.Data;
using System.Linq;

namespace Erronka.Services
{
    public class AuthService
    {
        public User? Login(string username, string password)
        {
            using var conn = Database.GetConnection();

            var user = conn.QuerySingleOrDefault<User>(
                "SELECT * FROM Users WHERE Username=@username", new { username });

            if (user == null)
                return null;

            // Validar contraseña con hash + salt
            bool valid = PasswordHelper.VerifyPassword(password, user.PasswordHash, user.PasswordSalt);
            return valid ? user : null;
        }

        public bool CreateUser(User u, string plainPassword)
        {
            using var conn = Database.GetConnection();

            var existing = conn.QuerySingleOrDefault<User>(
                "SELECT * FROM Users WHERE Username=@Username", new { u.Username });
            if (existing != null)
                return false;

            // Generar hash y salt
            PasswordHelper.CreatePasswordHash(plainPassword, out byte[] hash, out byte[] salt);

            conn.Execute(
                "INSERT INTO Users (Username, PasswordHash, PasswordSalt, Role, FullName) VALUES(@Username,@PasswordHash,@PasswordSalt,@Role,@FullName)",
                new { u.Username, PasswordHash = hash, PasswordSalt = salt, u.Role, u.FullName });

            return true;
        }

        public void UpdateUser(User u)
        {
            using var conn = Database.GetConnection();
            conn.Execute("UPDATE Users SET Username=@Username, Role=@Role, FullName=@FullName WHERE Id=@Id", u);
        }

        public void DeleteUser(int id)
        {
            using var conn = Database.GetConnection();
            conn.Execute("DELETE FROM Users WHERE Id=@id", new { id });
        }
    }
}
