using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statki.Database
{
    public class DatabaseManager
    {
        private const string ConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=admin;Database=Battleships";

        public void InitializeDatabase()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS users (
                    id SERIAL PRIMARY KEY,
                    email VARCHAR(255) NOT NULL UNIQUE,
                    login VARCHAR(100) NOT NULL UNIQUE,
                    password VARCHAR(255) NOT NULL,
                    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
                    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
                );";

                using (var command = new NpgsqlCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Tabela 'users' została pomyślnie utworzona (lub już istnieje).\n");
                }
            }
        }

        public bool RegisterUser(string email, string login, string hashedPassword)
        {
            string query = @"
                INSERT INTO users (email, login, password, created_at, updated_at)
                VALUES (@Email, @Login, @Password, NOW(), NOW());
            ";
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Login", login);
                        command.Parameters.AddWithValue("@Password", hashedPassword);
                        command.Parameters.AddWithValue("@RegistrationDate", DateTime.UtcNow);

                        command.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (PostgresException ex) when (ex.SqlState == "23505") // Unique violation
            {
                // Logika w przypadku duplikatu loginu lub emaila
                return false;
            }
            catch (Exception ex)
            {
                // Obsługa innych błędów
                Console.WriteLine($"Błąd podczas rejestracji: {ex.Message}");
                return false;
            }
        }

        public DBUser GetUserByLoginOrEmail(string loginOrEmail)
        {
            const string query = @"
            SELECT login, password AS PasswordHash, email 
            FROM users 
            WHERE login = @LoginOrEmail OR email = @LoginOrEmail";

            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@LoginOrEmail", loginOrEmail);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new DBUser
                                {
                                    Login = reader["login"].ToString(),
                                    PasswordHash = reader["PasswordHash"].ToString(),
                                    Email = reader["email"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas pobierania użytkownika: {ex.Message}");
            }

            return null;
        }


        public bool UpdateUser(string currentLogin, string newLogin, string newPassword, string newEmail)
        {
            const string query = @"
            UPDATE users
            SET login = @NewLogin, 
                password = @NewPassword, 
                email = @NewEmail,
                updated_at = NOW()
            WHERE login = @CurrentLogin";

            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@NewLogin", newLogin);
                        command.Parameters.AddWithValue("@NewPassword", newPassword);
                        command.Parameters.AddWithValue("@NewEmail", newEmail);
                        command.Parameters.AddWithValue("@CurrentLogin", currentLogin);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas aktualizacji użytkownika: {ex.Message}");
                return false;
            }
        }

        public class DBUser
        {
            public int Id { get; set; }
            public string Email { get; set; }
            public string Login { get; set; }
            public string PasswordHash { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    }
}
