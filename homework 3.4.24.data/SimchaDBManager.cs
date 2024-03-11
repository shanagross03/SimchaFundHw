using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace homework_3._4._24.data
{
    public class SimchaDBManager
    {
        private string _connectionString = @"Data Source=.\sqlexpress; Initial Catalog=HomeWork; Integrated Security=true;";

        public List<Contributor> GetContributors(int id = 0)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            var getById = id != 0 ? "WHERE c.Id = @id" : "";
            command.CommandText = $@"SELECT c.*, SUM(con.Amount) AS 'Contributed',
                                    (SELECT SUM(d.Amount) FROM Deposits d WHERE d.ContributorId = c.Id) AS 'Deposit' FROM Contributor c
                                    LEFT JOIN Contributions con 
                                    ON c.id= con.ContributorId
                                    {getById}
                                    GROUP BY c.FirstName, c.LastName, c.AlwaysInclude, c.Id, c.CellNumber, c.CreatedDate
                                    ORDER BY c.Id";
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            var reader = command.ExecuteReader();
            var contributors = new List<Contributor>();
            while (reader.Read())
            {
                contributors.Add(new()
                {
                    Id = (int)reader["Id"],
                    FirstName = (string)reader["FirstName"],
                    LastName = (string)reader["LastName"],
                    AlwaysInclude = (bool)reader["AlwaysInclude"],
                    CellNumber = (string)reader["CellNumber"],
                    CreatedDate = (DateTime)reader["CreatedDate"],
                    Balance = reader.GetOrNull<decimal>("Deposit") - reader.GetOrNull<decimal>("Contributed")
                });
            }
            return contributors;
        }

        public List<Simcha> GetSimchas(int id = 0)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            var getById = id != 0 ? "WHERE Id = @id" : "";
            command.CommandText = $"SELECT * FROM Simcha {getById}";
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            var reader = command.ExecuteReader();
            var simchas = new List<Simcha>();
            while (reader.Read())
            {
                simchas.Add(new()
                {
                    Id = (int)reader["Id"],
                    Name = (string)reader["Name"],
                    Date = (DateTime)reader["Date"],
                    TotalContributed = GetTotalPerSimcha((int)reader["Id"]),
                    ContributorCount = GetTotalContributorsPerSimcha((int)reader["Id"])
                });
            }
            return simchas;
        }

        public List<Contributor> AddContributedAmountForSimcha(int simchaId, List<Contributor> contributors)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = @$"SELECT c.Id, SUM(co.Amount) AS 'Amount' FROM Contributor c
                                LEFT JOIN Contributions co
                                ON c.Id = co.ContributorId 
                                WHERE co.SimchaId = @id
                                GROUP BY c.Id";
            command.Parameters.AddWithValue("@id", simchaId);
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                contributors.FirstOrDefault(c => c.Id == (int)reader["Id"]).AmountContributedToSimcha = (decimal)reader["Amount"];
            }

            return contributors;
        }

        public List<Transaction> GetTransactionHistory(int id)
        {
            var transactions = GetSimchaTransactions(id);
            transactions.AddRange(GetDepositTransactions(id));
            return transactions.OrderBy(t => t.Date).ToList();
        }

        public List<Transaction> GetSimchaTransactions(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT s.Name, s.Date, c.Amount FROM Simcha s
                                    JOIN Contributions c
                                    ON s.Id = c.SimchaId
                                    WHERE c.ContributorId = @id";
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            var reader = command.ExecuteReader();
            List<Transaction> transactions = new();
            while (reader.Read())
            {
                transactions.Add(new()
                {
                    Name = "Contribution for " + (string)reader["Name"],
                    Date = (DateTime)reader["Date"],
                    Total = (decimal)reader["Amount"]
                });
            }
            return transactions;
        }

        public List<Transaction> GetDepositTransactions(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.Parameters.AddWithValue("@id", id);
            command.CommandText = @"SELECT d.Amount, d.Date FROM Deposits d 
                                   WHERE d.ContributorId = @id";
            connection.Open();
            List<Transaction> transactions = new();
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                transactions.Add(new()
                {
                    Name = "Deposit",
                    Date = (DateTime)reader["Date"],
                    Total = (decimal)reader["Amount"]
                });
            }
            return transactions;
        }

        public int GetTotalContributorsPerSimcha(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT COUNT(*) FROM Contributions c
                                    JOIN Simcha s
                                    ON s.id = c.SimchaId
                                    WHERE s.Id = @id";
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            return (int)command.ExecuteScalar();

        }

        public int GetContributorCount()
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Contributor";
            connection.Open();
            return (int)command.ExecuteScalar();
        }

        public decimal GetTotalPerSimcha(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT SUM(Amount) AS 'Total' FROM Contributions WHERE SimchaId = @id";
            command.Parameters.AddWithValue("@id", id);
            connection.Open();

            object amount = command.ExecuteScalar();

            return amount == DBNull.Value ? 0 : (decimal)amount;
        }

        public void DeleteContributionsForSimcha(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE Contributions WHERE SimchaId = @id";
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            command.ExecuteNonQuery();
        }

        public void UpdateContributor(Contributor c)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = @"UPDATE Contributor SET FirstName = @firstName, LastName = @lastName, CellNumber = @cellNumber,
                 AlwaysInclude = @alwaysInclude WHERE Id = @id";

            command.Parameters.AddWithValue("@firstName", c.FirstName);
            command.Parameters.AddWithValue("@lastName", c.LastName);
            command.Parameters.AddWithValue("@cellNumber", c.CellNumber);
            command.Parameters.AddWithValue("@alwaysInclude", c.AlwaysInclude);
            command.Parameters.AddWithValue("@id", c.Id);
            connection.Open();
            command.ExecuteNonQuery();
        }

        public void AddContributionsForSimcha(List<Contribution> contributions)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = @$"INSERT INTO Contributions (ContributorId, Amount, SimchaId)
                                     VALUES (@contributorId, @amount, @simchaId)";
            connection.Open();

            foreach (Contribution c in contributions)
            {
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@contributorId", c.ContributorId);
                command.Parameters.AddWithValue("@amount", c.Amount);
                command.Parameters.AddWithValue("@simchaId", c.SimchaId);
                command.ExecuteNonQuery();
            }
        }

        public void AddContributor(Contributor c)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = @$"INSERT INTO Contributor(FirstName, LastName, CellNumber, CreatedDate, AlwaysInclude)
                                    VALUES (@firstName, @lastName, @cellNumber, @createdDate, @alwaysInclude) SELECT SCOPE_IDENTITY()";
            command.Parameters.AddWithValue("@firstName", c.FirstName);
            command.Parameters.AddWithValue("@lastName", c.LastName);
            command.Parameters.AddWithValue("@cellNumber", c.CellNumber);
            command.Parameters.AddWithValue("@alwaysInclude", c.AlwaysInclude);
            command.Parameters.AddWithValue("@createdDate", c.CreatedDate);
            connection.Open();
            c.Id = (int)(decimal)command.ExecuteScalar();
            AddDeposit(new Deposit { ContributorId = c.Id, Amount = c.Balance, Date = c.CreatedDate });
        }

        public void AddDeposit(Deposit d)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = @$"INSERT INTO Deposits(Date, Amount, ContributorId) VALUES(@date, @amount, @contributorId)";
            command.Parameters.AddWithValue("@date", d.Date);
            command.Parameters.AddWithValue("@amount", d.Amount);
            command.Parameters.AddWithValue("@contributorId", d.ContributorId);
            connection.Open();
            command.ExecuteNonQuery();

        }

        public void AddSimcha(Simcha s)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Simcha (Name, Date) VALUES (@name, @date)";
            command.Parameters.AddWithValue("@name", s.Name);
            command.Parameters.AddWithValue("@date", s.Date);
            connection.Open();
            command.ExecuteNonQuery();
        }
    }
    public static class Extensions
    {
        public static T GetOrNull<T>(this SqlDataReader reader, string columnName)
        {
            var value = reader[columnName];
            if (value == DBNull.Value)
            {
                return default(T);
            }

            return (T)value;
        }
    }
}
