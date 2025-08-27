using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoApp
{
    public class Memo
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public int OrderNum { get; set; }
    }
    internal class MemoRepository
    {
        private readonly string _databasePath;

        public MemoRepository(string databasePath)
        {
            _databasePath = databasePath;
            CreateDatabase();
        }

        private void CreateDatabase()
        {
            using var connection = new SqliteConnection(_databasePath);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText =
            @"
                CREATE TABLE IF NOT EXISTS Memos (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT,
                    Memo TEXT,
                    OrderNum INTEGER NOT NULL
                );
            ";
            command.ExecuteNonQuery();
        }

        public List<Memo> GetAll()
        {
            var memos = new List<Memo>();

            using var connection = new SqliteConnection(_databasePath);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Title, Memo, OrderNum FROM Memos ORDER BY OrderNum ASC";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                memos.Add(new Memo
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Content = reader.GetString(2),
                    OrderNum = reader.GetInt32(3)
                });
            }
            return memos;
        }

        public void Save(string title, string content)
        {
            using var connection = new SqliteConnection(_databasePath);
            connection.Open();

            var getMaxCommand = connection.CreateCommand();
            getMaxCommand.CommandText = "SELECT IFNULL(MAX(OrderNum), -1) FROM Memos";
            var maxOrder = Convert.ToInt32(getMaxCommand.ExecuteScalar());

            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText =
                "INSERT INTO Memos (Title, Memo, OrderNum) VALUES ($title, $memo, $order)";
            insertCommand.Parameters.AddWithValue("$title", title);
            insertCommand.Parameters.AddWithValue("$memo", content);
            insertCommand.Parameters.AddWithValue("$order", maxOrder + 1);
            insertCommand.ExecuteNonQuery();
        }

        public void Update(int id, string title, string content)
        {
            using var connection = new SqliteConnection(_databasePath);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "UPDATE Memos SET Title=@Title, Memo=@Memo WHERE Id=@Id";
            command.Parameters.AddWithValue("@Title", title);
            command.Parameters.AddWithValue("@Memo", content);
            command.Parameters.AddWithValue("@Id", id);
            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = new SqliteConnection(_databasePath);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Memos WHERE Id=@Id";
            command.Parameters.AddWithValue("@Id", id);
            command.ExecuteNonQuery();
        }
        public void UpdateOrder(int id, int orderNum)
        {
            using var connection = new SqliteConnection(_databasePath);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "UPDATE Memos SET OrderNum = $orderNum WHERE Id = $id";
            command.Parameters.AddWithValue("$orderNum", orderNum);
            command.Parameters.AddWithValue("$id", id);
            command.ExecuteNonQuery();
        }
    }
}
