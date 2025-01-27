using Npgsql;
using System;
using System.Collections.Generic;

namespace Statki.Database.Ranking
{
    public class RankingManager
    {
        private const string ConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=admin;Database=Battleships";
        private readonly int _loggedInUserId;

        public RankingManager(DatabaseManager databaseManager)
        {
            var sessionToken = SessionManager.GetSessionToken();
            if (string.IsNullOrEmpty(sessionToken))
            {
                throw new InvalidOperationException("Nie znaleziono aktywnej sesji.");
            }

            var user = databaseManager.GetUserBySessionToken(sessionToken);
            if (user == null)
            {
                throw new InvalidOperationException("Nie udało się uzyskać informacji o zalogowanym użytkowniku.");
            }

            _loggedInUserId = user.Id;
        }

        public void UpdateRanking()
        {
            var userStats = GetStatisticsForCurrentUser();

            if (userStats == null)
            {
                Console.WriteLine("Nie znaleziono statystyk dla aktualnie zalogowanego użytkownika.");
                return;
            }

            // Obliczanie miejsca użytkownika w rankingu dla punktów
            int rankPosition = CalculateRankPosition(userStats.Points);

            // Obliczanie miejsca użytkownika w rankingu dla najwyższego wyniku
            int rankHighestScorePosition = CalculateRankPosition(userStats.HighestScore, isForHighestScore: true);

            // Aktualizacja rankingu w bazie danych
            UpdatePlayerRankings(userStats, rankPosition, rankHighestScorePosition);

            // Aktualizacja rankingu dla wszystkich użytkowników
            UpdateAllUsersRanking();

            Console.WriteLine($"Miejsce użytkownika w rankingu punktów: {rankPosition}");
            Console.WriteLine($"Miejsce użytkownika w rankingu najwyższego wyniku: {rankHighestScorePosition}");
        }

        private PlayerRanking GetStatisticsForCurrentUser()
        {
            string query = @"
                SELECT s.user_id, s.points, s.highest_score
                FROM statistics s
                WHERE s.user_id = @UserId;";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", _loggedInUserId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new PlayerRanking
                            {
                                UserId = reader.GetInt32(0),
                                Points = reader.GetInt32(1),
                                HighestScore = reader.GetInt32(2)
                            };
                        }
                    }
                }
            }

            return null;
        }

        private void UpdatePlayerRankings(PlayerRanking userStats, int rankPosition, int rankHighestScorePosition)
        {
            string checkQuery = @"
        SELECT rank_points, rank_highest_score
        FROM ranking
        WHERE stat_id = (SELECT id FROM statistics WHERE user_id = @UserId);";

            int? currentRankPoints = null;
            int? currentRankHighestScore = null;

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                using (var checkCommand = new NpgsqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@UserId", userStats.UserId);

                    using (var reader = checkCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            currentRankPoints = reader.GetInt32(0);
                            currentRankHighestScore = reader.GetInt32(1);
                        }
                    }
                }

                string updateQuery = @"
                    INSERT INTO ranking (stat_id, rank_points, rank_highest_score, last_update_points, last_update_highest_score)
                    VALUES ((SELECT id FROM statistics WHERE user_id = @UserId), @RankPoints, @RankHighestScore, NOW(), NOW())
                    ON CONFLICT (stat_id) 
                    DO UPDATE SET ";

                List<string> updateParts = new List<string>();

                // Aktualizujemy datę tylko dla rankingu aktualnego użytkownika
                if (currentRankPoints != rankPosition)
                {
                    updateParts.Add("rank_points = @RankPoints");
                    updateParts.Add("last_update_points = NOW()");  // Tylko dla aktualnego użytkownika
                }

                if (currentRankHighestScore != rankHighestScorePosition)
                {
                    updateParts.Add("rank_highest_score = @RankHighestScore");
                    updateParts.Add("last_update_highest_score = NOW()");  // Tylko dla aktualnego użytkownika
                }

                if (updateParts.Count > 0)
                {
                    updateQuery += string.Join(", ", updateParts) + ";";

                    using (var updateCommand = new NpgsqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@UserId", userStats.UserId);
                        updateCommand.Parameters.AddWithValue("@RankPoints", rankPosition);
                        updateCommand.Parameters.AddWithValue("@RankHighestScore", rankHighestScorePosition);
                        updateCommand.ExecuteNonQuery();
                    }
                }
            }
        }


        private int CalculateRankPosition(int userPoints, bool isForHighestScore = false)
        {
            string query;

            // Przygotowanie zapytania w zależności od tego, czy jest to ranking dla punktów, czy dla najwyższego wyniku
            if (isForHighestScore)
            {
                query = @"
                    SELECT COUNT(*)
                    FROM statistics
                    WHERE highest_score > @UserPoints;";
            }
            else
            {
                query = @"
                    SELECT COUNT(*)
                    FROM statistics
                    WHERE points > @UserPoints;";
            }

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserPoints", userPoints);

                    int higherRankCount = Convert.ToInt32(command.ExecuteScalar());

                    // Pozycja użytkownika to liczba graczy z większą liczbą punktów (lub najwyższym wynikiem) + 1
                    return higherRankCount + 1;
                }
            }
        }

        // Nowa metoda do aktualizacji rankingu dla wszystkich użytkowników
        private void UpdateAllUsersRanking()
        {
            string query = @"
                SELECT s.user_id, s.points, s.highest_score
                FROM statistics s";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int userId = reader.GetInt32(0);
                            int points = reader.GetInt32(1);
                            int highestScore = reader.GetInt32(2);

                            // Obliczanie pozycji w rankingu dla punktów i najwyższego wyniku
                            int rankPoints = CalculateRankPosition(points);
                            int rankHighestScore = CalculateRankPosition(highestScore, isForHighestScore: true);

                            // Aktualizacja rankingu użytkownika
                            UpdatePlayerRankings(new PlayerRanking { UserId = userId, Points = points, HighestScore = highestScore }, rankPoints, rankHighestScore);
                        }
                    }
                }
            }
        }

        public class PlayerRanking
        {
            public int UserId { get; set; }
            public int Points { get; set; }
            public int HighestScore { get; set; }
        }
    }
}
