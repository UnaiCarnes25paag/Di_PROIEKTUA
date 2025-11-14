using System;

namespace Erronka.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }

        public Table Table { get; set; }
        public User User { get; set; }
    }
}