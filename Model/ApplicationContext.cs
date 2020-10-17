using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinRTXamlToolkit.IO.Serialization;

namespace vNextBot.Model
{
    public class ApplicationContext : DbContext
    {
        private static string connectionString;
        private static string connectionLine;
        public static void setConnectionString(string host, int port, string dbName, string login, string password)
        {
            connectionString = "Server=" + host + ";Port=" + port + ";Database=" + dbName + ";Username=" + login + ";Password=" + password;
            connectionLine = "Server=" + host + " Port=" + port + " Database=" + dbName + " Username=" + login;
        }

        public static string getConnectionLine()
        {
            return connectionLine;
        }

        public ApplicationContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(connectionString);
        }
        public DbSet<Setting> Setting { get; set; }
        public DbSet<SettingTypes> SettingTypes { get; set; }
        public DbSet<Action> Actions { get; set; }
        public DbSet<KnowledgeBase> KnowledgeBases { get; set; }
        public DbSet<User> Users { get; set; }

        public object UserReset(int id)
        {
            using (var command = Database.GetDbConnection().CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "dbo.cf_user_reset";
                command.Parameters.Add(new Npgsql.NpgsqlParameter("_f_user", NpgsqlTypes.NpgsqlDbType.Integer)
                { Value = id });
                if (command.Connection.State == ConnectionState.Closed)
                    command.Connection.Open();
                return command.ExecuteScalar();
            }
        }
    }
}
