using Npgsql;
using Statki.Profile_Managment;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Statki.Database.Ranking
{
    public partial class RankingWindow : Window
    {
        private const string ConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=admin;Database=Battleships";

        public RankingWindow()
        {
            InitializeComponent();
            LoadRankings();
        }

        private void LoadRankings()
        {
            var pointsRanking = GetRankingData(false);  // Ranking po punktach
            var highestScoreRanking = GetRankingData(true);  // Ranking po najwyższym wyniku

            PointsRankingDataGrid.ItemsSource = pointsRanking;
            HighestScoreRankingDataGrid.ItemsSource = highestScoreRanking;
        }

        private List<RankingEntry> GetRankingData(bool isForHighestScore)
        {
            var rankingEntries = new List<RankingEntry>();
            string query;

            if (isForHighestScore)
            {
                query = @"
                    SELECT u.login, s.highest_score
                    FROM statistics s
                    JOIN users u ON u.id = s.user_id
                    ORDER BY s.highest_score DESC;";
            }
            else
            {
                query = @"
                    SELECT u.login, s.points
                    FROM statistics s
                    JOIN users u ON u.id = s.user_id
                    ORDER BY s.points DESC;";
            }

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        int position = 1;
                        while (reader.Read())
                        {
                            rankingEntries.Add(new RankingEntry
                            {
                                RankPosition = position++,
                                UserLogin = reader.GetString(0),
                                Points = isForHighestScore ? 0 : reader.GetInt32(1), // Tylko dla punktów
                                HighestScore = isForHighestScore ? reader.GetInt32(1) : 0  // Tylko dla najwyższego wyniku
                            });
                        }
                    }
                }
            }

            return rankingEntries;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Powrót na ekran startowy
            var startupWindow = new StartupWindow();
            startupWindow.Show();
            this.Close();
        }
    }

    public class RankingEntry
    {
        public int RankPosition { get; set; }
        public string UserLogin { get; set; }
        public int Points { get; set; }
        public int HighestScore { get; set; }
    }
}
