using Dapper;
using Erronka.Data;
using Erronka.Models;
using System.Collections.Generic;

namespace Erronka.Services
{
    public class ProductService
    {
        public IEnumerable<Product> GetAll()
        {
            using var conn = Database.GetConnection();
            return conn.Query<Product>("SELECT * FROM Products ORDER BY Name");
        }

        public Product GetById(int id)
        {
            using var conn = Database.GetConnection();
            return conn.QuerySingleOrDefault<Product>("SELECT * FROM Products WHERE Id = @id", new { id });
        }

        public int Create(Product p)
        {
            using var conn = Database.GetConnection();
            return (int)conn.ExecuteScalar<long>("INSERT INTO Products (Code, Name, Price) VALUES(@Code,@Name,@Price); SELECT last_insert_rowid();", p);
        }

        public void Update(Product p)
        {
            using var conn = Database.GetConnection();
            conn.Execute("UPDATE Products SET Code=@Code, Name=@Name, Price=@Price WHERE Id=@Id", p);
        }

        public void Delete(int id)
        {
            using var conn = Database.GetConnection();
            conn.Execute("DELETE FROM Products WHERE Id=@id", new { id });
        }
    }
}
