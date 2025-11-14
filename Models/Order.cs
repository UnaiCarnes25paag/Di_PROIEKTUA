using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Erronka.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int? TableId { get; set; }
        public int? UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public double Total { get; set; }
        public bool Paid { get; set; }

        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}