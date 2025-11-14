using Dapper;
using Erronka.Data;
using Erronka.Models;
using System.Collections.Generic;
using System.Linq;

namespace Erronka.Services
{
    public class ReservationService
    {
        public IEnumerable<Reservation> GetAll()
        {
            using var conn = Database.GetConnection();
            var sql = @"
SELECT r.Id, r.TableId, r.UserId, r.Date, r.Type,
       t.Id AS TableId, t.Number, t.Seats,
       u.Id AS UserId, u.Username, u.FullName, u.Role
FROM Reservations r
LEFT JOIN Tables t ON r.TableId = t.Id
LEFT JOIN Users u ON r.UserId = u.Id
ORDER BY r.Date, r.TimeSlot";
            // Multi-mapping: Reservation, Table, User -> Reservation
            var list = conn.Query<Reservation, Table, User, Reservation>(
                sql,
                (res, table, user) =>
                {
                    res.Table = table;
                    res.User = user;
                    return res;
                },
                splitOn: "TableId,UserId"
            ).ToList();

            return list;
        }

        public bool IsTableFree(int tableId, string date, string timeSlot)
        {
            using var conn = Database.GetConnection();
            int count = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM Reservations WHERE TableId=@t AND Date=@d AND TimeSlot=@ts",
                new { t = tableId, d = date, ts = timeSlot });
            return count == 0;
        }

        public int Create(Reservation r)
        {
            using var conn = Database.GetConnection();
            return (int)conn.ExecuteScalar<long>("INSERT INTO Reservations (TableId, CustomerName, Date, TimeSlot, UserId) VALUES(@TableId,@CustomerName,@Date,@TimeSlot,@UserId); SELECT last_insert_rowid();", r);
        }

        public void Update(Reservation r)
        {
            using var conn = Database.GetConnection();
            conn.Execute("UPDATE Reservations SET TableId=@TableId, CustomerName=@CustomerName, Date=@Date, TimeSlot=@TimeSlot WHERE Id=@Id", r);
        }

        public void Delete(int id)
        {
            using var conn = Database.GetConnection();
            conn.Execute("DELETE FROM Reservations WHERE Id=@id", new { id });
        }
    }
}