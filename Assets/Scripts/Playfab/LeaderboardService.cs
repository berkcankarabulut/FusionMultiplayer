using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace FPSGame.PlayFab
{
    public class LeaderboardService
    {
        public void SendLeaderboard(
            int score,
            Action<UpdatePlayerStatisticsResult> onSuccess = null,
            Action<PlayFabError> onFail = null
        )
        {
            var request = new UpdatePlayerStatisticsRequest
            {
                Statistics = new List<StatisticUpdate>
                {
                    new StatisticUpdate { StatisticName = "Score", Value = score },
                },
            };
            PlayFabClientAPI.UpdatePlayerStatistics(
                request,
                result =>
                {
                    Debug.Log("Leaderboard updated");
                    onSuccess?.Invoke(result);
                },
                error =>
                {
                    Debug.Log(error.ErrorMessage);
                    onFail?.Invoke(error);
                }
            );
        }

        public void GetLeaderboard(
            Action<GetLeaderboardResult> onSuccess = null,
            Action<PlayFabError> onFail = null
        )
        {
            var request = new GetLeaderboardRequest
            {
                StatisticName = "Score",
                StartPosition = 0,
                MaxResultsCount = 10,
            };
            PlayFabClientAPI.GetLeaderboard(
                request,
                result =>
                {
                    Debug.Log("Leaderboard retrieved");
                    foreach (var item in result.Leaderboard)
                    {
                        Debug.Log(item.DisplayName + " " + item.StatValue);
                    }
                    onSuccess?.Invoke(result);
                },
                error =>
                {
                    Debug.Log(error.ErrorMessage);
                    onFail?.Invoke(error);
                }
            );
        }
    }
}
