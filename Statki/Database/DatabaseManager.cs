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

                // Tabela users
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

                // Tabela sessions
                string createSessionsTableQuery = @"
                CREATE TABLE IF NOT EXISTS sessions (
                    id SERIAL PRIMARY KEY,
                    user_id INT REFERENCES users(id),
                    session_token VARCHAR(255) NOT NULL UNIQUE,
                    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
                    expires_at TIMESTAMP NOT NULL
                );";

                using (var command = new NpgsqlCommand(createSessionsTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Tabela 'sessions' została pomyślnie utworzona (lub już istnieje).\n");
                }

                // Tabela Statistics
                string createStatisticsTableQuery = @"
                    CREATE TABLE IF NOT EXISTS statistics (
                        id SERIAL PRIMARY KEY,
                        user_id INT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
                        games_played INT NOT NULL DEFAULT 0,
                        games_won INT NOT NULL DEFAULT 0,
                        games_lost INT NOT NULL DEFAULT 0,
                        points INT NOT NULL DEFAULT 0,
                        highest_score INT NOT NULL DEFAULT 0,
                        CONSTRAINT unique_user_id UNIQUE (user_id)
                    );";

                using (var command = new NpgsqlCommand(createStatisticsTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Tabela 'statistics' została pomyślnie utworzona (lub już istnieje).");
                }

                // Tabela Ranking
                string createRankingTableQuery = @"
               CREATE TABLE IF NOT EXISTS ranking (
                    id SERIAL PRIMARY KEY,
                    stat_id INT NOT NULL REFERENCES statistics(id) ON DELETE CASCADE,
                    rank_points INT NOT NULL DEFAULT 0,
                    rank_highest_score INT NOT NULL DEFAULT 0,
                    last_update_points TIMESTAMP NOT NULL DEFAULT NOW(),
                    last_update_highest_score TIMESTAMP NOT NULL DEFAULT NOW(),
                    CONSTRAINT unique_stat_id UNIQUE (stat_id)
                );";

                using (var command = new NpgsqlCommand(createRankingTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Tabela 'ranking' została pomyślnie utworzona (lub już istnieje).");
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
            SELECT id, login, password AS PasswordHash, email, created_at, updated_at
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
                                Console.WriteLine($"Elo tu masz id usera {reader["id"]}, {reader["email"]}, {reader["login"]}");
                                return new DBUser
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Login = reader["login"].ToString(),
                                    PasswordHash = reader["PasswordHash"].ToString(),
                                    Email = reader["email"].ToString(),
                                    CreatedAt = Convert.ToDateTime(reader["created_at"]),
                                    UpdatedAt = Convert.ToDateTime(reader["updated_at"])
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

        public void SaveSessionToken(int userId, string sessionToken, DateTime expiresAt)
        {
            string checkQuery = @"
                SELECT session_token 
                FROM sessions 
                WHERE user_id = @UserId AND expires_at > NOW()";

            string insertQuery = @"
                INSERT INTO sessions (user_id, session_token, created_at, expires_at)
                VALUES (@UserId, @SessionToken, NOW(), @ExpiresAt);";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                using (var checkCommand = new NpgsqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@UserId", userId);

                    var existingToken = checkCommand.ExecuteScalar();
                    if (existingToken != null)
                    {
                        sessionToken = existingToken.ToString();
                        Console.WriteLine($"Token already exists in database: {sessionToken}");
                        SessionManager.SetSessionToken(sessionToken);
                        return;
                    }
                }

                using (var insertCommand = new NpgsqlCommand(insertQuery, connection))
                {
                    insertCommand.Parameters.AddWithValue("@UserId", userId);
                    insertCommand.Parameters.AddWithValue("@SessionToken", sessionToken);
                    insertCommand.Parameters.AddWithValue("@ExpiresAt", expiresAt);

                    insertCommand.ExecuteNonQuery();
                    Console.WriteLine($"Token inserted into database: {sessionToken}");
                }

                // Zapisz token do pliku po upewnieniu się, że jest nowy
                SessionManager.SetSessionToken(sessionToken);
            }
        }




        public DBUser GetUserBySessionToken(string sessionToken)
        {
            if (!(sessionToken is string))
            {
                throw new ArgumentException("Session token must be a string.");
            }


            const string query = @"
            SELECT u.id, u.login, u.email, u.password AS PasswordHash
            FROM users u
            JOIN sessions us ON u.id = us.user_id
            WHERE us.session_token = @SessionToken AND us.expires_at > NOW()";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(query, connection))
                {
                    // Poprawnie dodajemy parametr
                    command.Parameters.AddWithValue("@SessionToken", sessionToken);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new DBUser
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Login = reader["login"].ToString(),
                                Email = reader["email"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString()
                            };
                        }
                    }
                }
            }

            return null;
        }

        public string GetExistingSessionToken(int userId)
        {
            string query = @"
                SELECT session_token 
                FROM sessions 
                WHERE user_id = @UserId 
                  AND expires_at > NOW()";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    var result = command.ExecuteScalar();
                    return result?.ToString();
                }
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