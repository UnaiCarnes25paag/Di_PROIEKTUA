using Dapper;
using Erronka.Data;
using Erronka.Models;
using System.Collections.Generic;
using System.Linq;

namespace Erronka.Services
{
    public class OrderService
    {
        public int CreateOrder(Order o)
        {
            using var conn = Database.GetConnection();
            using var tx = conn.BeginTransaction();

            // Obtener precios desde Products y calcular el total
            double total = 0;

            foreach (var item in o.Items)
            {
                // Obtener el precio del producto desde la base de datos
                double productPrice = conn.ExecuteScalar<double>(
                    "SELECT Price FROM Products WHERE Id = @id",
                    new { id = item.ProductId },
                    tx
                );

                // Calcular subtotal y acumular al total del pedido
                total += productPrice * item.Quantity;

                // Guardar el item en OrderItems (sin UnitPrice)
                conn.Execute(
                    "INSERT INTO OrderItems (OrderId, ProductId, Quantity) VALUES(@OrderId, @ProductId, @Quantity)",
                    new
                    {
                        OrderId = 0,
                        item.ProductId,
                        item.Quantity
                    },
                    tx
                );
            }

            // 🔹 Insertar el pedido en Orders (con total calculado)
            var orderId = (int)conn.ExecuteScalar<long>(
                "INSERT INTO Orders (TableId, UserId, Total, Paid, CreatedAt) VALUES(@TableId,@UserId,@Total,@Paid,@CreatedAt); SELECT last_insert_rowid();",
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

            // 🔹 Actualizar los OrderItems para asignar el OrderId correcto
            foreach (var item in o.Items)
            {
                conn.Execute(
                    "UPDATE OrderItems SET OrderId = @OrderId WHERE OrderId = 0 AND ProductId = @ProductId",
                    new { OrderId = orderId, item.ProductId },
                    tx
                );
            }

            tx.Commit();
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

            // Use Dapper multi-mapping to populate OrderItem.Product
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