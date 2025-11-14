using Erronka.Models;
using System;
using System.IO;
using System.Text;

namespace Erronka.Services
{
    /// Genera el ticket de una orden (impreso o en pantalla).
    public class TicketService
    {
        private readonly OrderService _orderService;

        public TicketService()
        {
            _orderService = new OrderService();
        }

        public string GenerateTicket(int orderId)
        {
            var order = _orderService.GetOrder(orderId);
            if (order == null)
                return "Error: no se encontró la orden.";

            var sb = new StringBuilder();
            sb.AppendLine("===== ERRONKA ELKARTEA =====");
            sb.AppendLine($"Ticket Nº: {order.Id}");
            sb.AppendLine($"Fecha: {order.CreatedAt:dd/MM/yyyy HH:mm}");
            sb.AppendLine("-----------------------------");

            double total = 0;
            foreach (var item in order.Items)
            {
                // Use Product on OrderItem; guard against null Product
                var product = item.Product;
                string productName = product?.Name ?? "Desconocido";
                double unitPrice = product?.Price ?? 0.0;

                double subtotal = item.Quantity * unitPrice;
                sb.AppendLine($"{productName} x{item.Quantity}  {subtotal:0.00}€");
                total += subtotal;
            }

            sb.AppendLine("-----------------------------");
            sb.AppendLine($"TOTAL: {total:0.00}€");
            sb.AppendLine(order.Paid ? "PAGADO " : "PENDIENTE ");
            sb.AppendLine("=============================");

            // Guardar como archivo .txt en Tickets/
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tickets");
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, $"Ticket_{order.Id}.txt");
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);

            return path;
        }
    }
}