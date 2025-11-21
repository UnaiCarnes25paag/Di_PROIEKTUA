using Erronka.Models;

public class Reservation
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime Date { get; set; }
    public string TimeSlot { get; set; }

    public Table Table { get; set; }
    public User User { get; set; }
}
