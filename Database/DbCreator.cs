using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace Db
{
    public abstract class DbCreator
    {
        abstract public DbConnection newConnection(string connectionString);
        abstract public DbCommand newCommand(string sql, DbConnection connection);
        abstract public DbCommand newCommand(string sql);
        abstract public DbDataAdapter newAdapter(string sql, DbConnection connection);
        abstract public DbTransaction newTransaction(DbConnection connection);

    }
        
    public class NpgsqlCreator : DbCreator
    {
        public override DbConnection newConnection(string connectionString)
        {
            return new NpgsqlConnection(connectionString);
        }

        public override DbCommand newCommand(string sql, DbConnection connection)
        {
            return new NpgsqlCommand(sql, (NpgsqlConnection)connection);
        }

        public override DbCommand newCommand(string sql)
        {
            return new NpgsqlCommand(sql);
        }

        public override DbDataAdapter newAdapter(string sql, DbConnection connection)
        {
            return new NpgsqlDataAdapter(sql, (NpgsqlConnection)connection);
        }

        public override DbTransaction newTransaction(DbConnection connection)
        {
            return (connection as NpgsqlConnection).BeginTransaction();
        }
    }

    public class SQLiteCreator : DbCreator
    {
        public override DbConnection newConnection(string connectionString)
        {
            return new SQLiteConnection(connectionString);
        }

        public override DbCommand newCommand(string sql, DbConnection connection)
        {
            return new SQLiteCommand(sql, (SQLiteConnection)connection);
        }

        public override DbCommand newCommand(string sql)
        {
            return new SQLiteCommand(sql);
        }

        public override DbDataAdapter newAdapter(string sql, DbConnection connection)
        {
            return null;
        }

        public override DbTransaction newTransaction(DbConnection connection)
        {
            throw new NotImplementedException();
        }
    }
}
