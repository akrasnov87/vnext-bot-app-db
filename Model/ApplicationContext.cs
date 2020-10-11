using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinRTXamlToolkit.IO.Serialization;

namespace RpcSecurity.Model
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
        public DbSet<Digests> Digests { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Division> Divisions { get; set; }
        public DbSet<Accesses> Accesses { get; set; }
        public DbSet<Objects> Objects { get; set; }
        public DbSet<ClientError> ClientErrors { get; set; }
    }
}
