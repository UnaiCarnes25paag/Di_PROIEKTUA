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
SELECT 
    r.Id,
    r.TableId,
    r.CreatedByUserId,
    r.Date,
    r.TimeSlot,

    t.Id AS Table_Id,
    t.Number,
    t.Seats,

    u.Id AS User_Id,
    u.Username,
    u.FullName,
    u.Role
FROM Reservations r
LEFT JOIN Tables t ON r.TableId = t.Id
LEFT JOIN Users u ON r.CreatedByUserId = u.Id
ORDER BY r.Date, r.TimeSlot
";

            return conn.Query<Reservation, Table, User, Reservation>(
                sql,
                (res, table, user) =>
                {
                    res.Table = table;
                    res.User = user;
                    return res;
                },
                splitOn: "Table_Id,User_Id"
            ).ToList();
        }


        public bool IsTableFree(int tableId, DateTime date, string timeSlot)
        {
            using var conn = Database.GetConnection();
            int count = conn.ExecuteScalar<int>(
                @"SELECT COUNT(*) 
                  FROM Reservations 
                  WHERE TableId = @tableId AND Date = @date AND TimeSlot = @timeSlot",
                new { tableId, date, timeSlot });

            return count == 0;
        }


        public int Create(Reservation r)
        {
            using var conn = Database.GetConnection();

            // Comprobar disponibilidad ANTES de insertar:
            int count = conn.ExecuteScalar<int>(
                @"SELECT COUNT(*) FROM Reservations 
          WHERE TableId = @TableId AND Date = @Date AND TimeSlot = @TimeSlot",
                new { r.TableId, r.Date, r.TimeSlot });

            if (count > 0)
                throw new Exception("Mahaia dagoeneko okupatuta dago egun eta ordu-tarte horretan.");

            var sql = @"
        INSERT INTO Reservations (TableId, Date, TimeSlot, CreatedByUserId) 
        VALUES (@TableId, @Date, @TimeSlot, @CreatedByUserId);
        SELECT last_insert_rowid();
    ";

            return (int)conn.ExecuteScalar<long>(sql, r);
        }



        public void Update(Reservation r)
        {
            using var conn = Database.GetConnection();

            int count = conn.ExecuteScalar<int>(
                @"SELECT COUNT(*) FROM Reservations 
          WHERE TableId = @TableId AND Date = @Date AND TimeSlot = @TimeSlot
            AND Id != @Id",
                new { r.TableId, r.Date, r.TimeSlot, r.Id });

            if (count > 0)
                throw new Exception("Ezinezkoa da eguneratzea: mahaia dagoeneko okupatuta dago.");

            var sql = @"
    UPDATE Reservations 
    SET TableId = @TableId,
        Date = @Date,
        TimeSlot = @TimeSlot
    WHERE Id = @Id";

            conn.Execute(sql, r);
        }

        public void Delete(int id)
        {
            using var conn = Database.GetConnection();
            conn.Execute("DELETE FROM Reservations WHERE Id = @id", new { id });
        }


        public IEnumerable<Reservation> GetByUserId(int userId)
        {
            using var conn = Database.GetConnection();

            var sql = @"
SELECT * 
FROM Reservations 
WHERE CreatedByUserId = @userId 
ORDER BY Date, TimeSlot";

            return conn.Query<Reservation>(sql, new { userId });
        }

        public IEnumerable<Reservation> GetByUserIdWithTable(int userId)
        {
            using var conn = Database.GetConnection();

            var sql = @"
SELECT 
    r.Id, r.TableId, r.CreatedByUserId, r.Date, r.TimeSlot,
    t.Id AS Table_Id, t.Number, t.Seats,
    u.Id AS User_Id, u.Username, u.FullName, u.Role
FROM Reservations r
LEFT JOIN Tables t ON r.TableId = t.Id
LEFT JOIN Users u ON r.CreatedByUserId = u.Id
WHERE r.CreatedByUserId = @userId
ORDER BY r.Date, r.TimeSlot";

            return conn.Query<Reservation, Table, User, Reservation>(
                sql,
                (res, table, user) =>
                {
                    res.Table = table;
                    res.User = user;
                    return res;
                },
                new { userId },
                splitOn: "Table_Id,User_Id"
            );
        }

        public IEnumerable<Table> GetAvailableTables(DateTime date, string timeSlot)
        {
            using var conn = Database.GetConnection();

            var sql = @"
        SELECT * 
        FROM Tables 
        WHERE Id NOT IN (
            SELECT TableId FROM Reservations
            WHERE Date = @date AND TimeSlot = @timeSlot
        )";

            return conn.Query<Table>(sql, new { date, timeSlot });
        }

    }
}
