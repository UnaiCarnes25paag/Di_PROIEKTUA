using Dapper;
using Erronka.Data;
using Erronka.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Erronka.Services
{
    public class OrderService
    {
        // === MÉTODO NORMAL (sin cambios) ===
        public int CreateOrder(Order o)
        {
            using var conn = Database.GetConnection();
            using var tx = conn.BeginTransaction();

            int orderId = CreateOrderTransactional(o, conn, tx);

            tx.Commit();
            return orderId;
        }

        // === NUEVO MÉTODO: CREACIÓN EN TRANSACCIÓN EXTERNA ===
        public int CreateOrderTransactional(Order o, IDbConnection conn, IDbTransaction tx)
        {
            // Calcular total
            double total = 0;
            foreach (var item in o.Items)
            {
                double productPrice = conn.ExecuteScalar<double>(
                    "SELECT Price FROM Products WHERE Id = @id",
                    new { id = item.ProductId },
                    tx
                );
                total += productPrice * item.Quantity;
            }

            // Insertar orden
            var orderId = (int)conn.ExecuteScalar<long>(
                @"INSERT INTO Orders (TableId, UserId, Total, Paid, CreatedAt)
                  VALUES (@TableId, @UserId, @Total, @Paid, @CreatedAt);
                  SELECT last_insert_rowid();",
                new
                {
                    o.TableId,
                    o.UserId,
                    Total = total,
                    Paid = o.Paid ? 1 : 0,
                    CreatedAt = o.CreatedAt
                },
                tx
            );

            // Insertar items
            foreach (var item in o.Items)
            {
                conn.Execute(
                    "INSERT INTO OrderItems (OrderId, ProductId, Quantity) VALUES(@OrderId, @ProductId, @Quantity)",
                    new
                    {
                        OrderId = orderId,
                        item.ProductId,
                        item.Quantity
                    },
                    tx
                );
            }

            return orderId;
        }

        public Order GetOrder(int id)
        {
            using var conn = Database.GetConnection();

            var o = conn.QuerySingleOrDefault<Order>(
                "SELECT * FROM Orders WHERE Id=@id",
                new { id }
            );

            if (o == null) return null;

            var items = conn.Query<OrderItem, Product, OrderItem>(
                @"SELECT oi.*, p.* 
                  FROM OrderItems oi
                  LEFT JOIN Products p ON oi.ProductId = p.Id
                  WHERE oi.OrderId = @id",
                (orderItem, product) =>
                {
                    orderItem.Product = product;
                    return orderItem;
                },
                new { id },
                splitOn: "Id"
            ).ToList();

            o.Items = items;
            return o;
        }

        public IEnumerable<Order> GetAll()
        {
            using var conn = Database.GetConnection();
            return conn.Query<Order>("SELECT * FROM Orders ORDER BY CreatedAt DESC");
        }

        public void SetPaid(int orderId)
        {
            using var conn = Database.GetConnection();
            conn.Execute("UPDATE Orders SET Paid=1 WHERE Id=@id", new { id = orderId });
        }
    }
}
