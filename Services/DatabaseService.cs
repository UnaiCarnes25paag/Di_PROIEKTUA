using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using Erronka.Data;

namespace Erronka.Services
{
    /// <summary>
    /// Wrapper de compatibilidad: devuelve la misma conexión centralizada
    /// gestionada por Erronka.Data.Database (tpv.db).
    /// </summary>
    public static class DatabaseService
    {
        public static IDbConnection GetConnection()
        {
            return Database.GetConnection();
        }
    }
}
