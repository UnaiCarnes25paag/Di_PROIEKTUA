using Dapper;
using Erronka.Data;
using Erronka.Models;
using System.Collections.Generic;
using System.Linq;

namespace Erronka.Services
{
    public class StockService
    {
        public IEnumerable<Stock> GetAll()
        {
            using var conn = Database.GetConnection();
            var sql = @"
SELECT s.Id, s.ProductId, s.Quantity, s.Location,
       p.Id, p.Code, p.Name, p.Price
FROM Stock s
LEFT JOIN Products p ON s.ProductId = p.Id
ORDER BY p.Name";

            var list = conn.Query<Stock, Product, Stock>(
                sql,
                (stock, product) =>
                {
                    stock.Product = product;
                    return stock;
                },
                splitOn: "Id"
            ).ToList();

            return list;
        }

        public void AddStock(Stock s)
        {
            using var conn = Database.GetConnection();
            conn.Execute("INSERT INTO Stock (ProductId, Quantity, Location) VALUES(@ProductId,@Quantity,@Location)", s);
        }

        public void UpdateStock(Stock s)
        {
            using var conn = Database.GetConnection();
            conn.Execute("UPDATE Stock SET ProductId=@ProductId, Quantity=@Quantity, Location=@Location WHERE Id=@Id", s);
        }

        public void DeleteStock(int id)
        {
            using var conn = Database.GetConnection();
            conn.Execute("DELETE FROM Stock WHERE Id=@id", new { id });
        }
    }
}
