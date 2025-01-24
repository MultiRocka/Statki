using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Npgsql;
using System;
using static Statki.Database.DatabaseManager;

namespace Statki.Database
{
    public class StatisticsManager
    {
        private const string ConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=admin;Database=Battleships";

        private readonly DatabaseManager _databaseManager;

        public StatisticsManager(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        public void CreateOrUpdateStatistics()
        {
        //    string loggedInUser = SessionManager.GetLoggedInUser();

            //if (string.IsNullOrEmpty(loggedInUser))
            //{
            //    Console.WriteLine("Brak zalogowanego użytkownika.");
            //    return;
            //}

            DBUser user = _databaseManager.GetUserBySessionToken(SessionManager.CurrentSessionToken);

            if (user == null)
            {
                Console.WriteLine("Użytkownik nie znaleziony lub sesja wygasła.");
                return;
            }

            string query = @"INSERT INTO statistics (user_id, games_played, games_won, games_lost, points, highest_score)
                             VALUES (@UserId, 0, 0, 0, 0, 0)
                             ON CONFLICT (user_id) DO NOTHING;";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", user.Id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void IncrementGamesPlayed()
        {
            UpdateStatisticField("games_played", 1);
        }

        public void IncrementGamesWon()
        {
            UpdateStatisticField("games_won", 1);
        }

        public void IncrementGamesLost()
        {
            UpdateStatisticField("games_lost", 1);
        }

        public void AddPoints(int points)
        {
            UpdateStatisticField("points", points);
        }

        public void UpdateHighestScore(int newScore)
        {   
            DBUser user = _databaseManager.GetUserBySessionToken(SessionManager.CurrentSessionToken);

            if (user == null)
            {
                Console.WriteLine("Użytkownik nie znaleziony lub sesja wygasła.");
                return;
            }

            string query = @"UPDATE statistics
                             SET highest_score = GREATEST(highest_score, @NewScore)
                             WHERE user_id = @UserId;";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", user.Id);
                    command.Parameters.AddWithValue("@NewScore", newScore);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void UpdateStatisticField(string fieldName, int increment)
        {

            DBUser user = _databaseManager.GetUserBySessionToken(SessionManager.CurrentSessionToken);

            if (user == null)
            {
                Console.WriteLine("Użytkownik nie znaleziony lub sesja wygasła.");
                return;
            }

            string query = $@"UPDATE statistics
                             SET {fieldName} = {fieldName} + @Increment
                             WHERE user_id = @UserId;";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", user.Id);
                    command.Parameters.AddWithValue("@Increment", increment);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
