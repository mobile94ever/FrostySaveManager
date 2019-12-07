using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using System;
using UnityEngine;

namespace FrostyMaxSaveManager
{

    public enum OperationFailed { ReadFromCloud,WriteToCloud,ReadFromLocal,WriteToLocal,AlreadyWriting}


    public abstract class FrostReadWrite
    {
        //Change it according to needs
        protected const string SaveFilePlayer = "playerlocalsave.save";
        protected const string GlobalTimePlayed = "GlobalTimePlayed";
        protected const string TimeStampConst = "TimeStamp";

        //General Read Write Operations. Defines how a file will be read/write
        protected void ReadSavedGame(string filename,
                               Action<SavedGameRequestStatus, ISavedGameMetadata> callback)
        {

            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            savedGameClient.OpenWithAutomaticConflictResolution(
                filename,
                DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLongestPlaytime,
                callback);
        }
        protected void WriteSavedGame(ISavedGameMetadata game, byte[] savedData,
                                  Action<SavedGameRequestStatus, ISavedGameMetadata> callback)
        {

            SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder()
                .WithUpdatedPlayedTime(TimeSpan.FromMinutes(game.TotalTimePlayed.Minutes + 1))
                .WithUpdatedDescription("Saved at: " + System.DateTime.Now);

            // You can add an image to saved game data (such as as screenshot)
            // byte[] pngData = <PNG AS BYTES>;
            // builder = builder.WithUpdatedPngCoverImage(pngData);

            SavedGameMetadataUpdate updatedMetadata = builder.Build();

            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            savedGameClient.CommitUpdate(game, updatedMetadata, savedData, callback);
        }

        //Potentially not logged in
        protected bool IsUserAutheticated()
        {
            return Social.localUser.authenticated;
        }



        //Handle Read/Write Faliure
        protected void WriteOperationFailed(OperationFailed FailedOperation)
        {
            switch(FailedOperation)
            {
                case OperationFailed.WriteToLocal: {  break; }
            }
        }

        protected void ReadOperationFailed(OperationFailed FailedOperation)
        {

        }



        protected float GetTotalPlayedTime()
        {
            return PlayerPrefs.GetFloat(GlobalTimePlayed, 0);
        }

        protected void SetTotalPlayedTime()
        {
            float GlobalGameTime = PlayerPrefs.GetFloat(GlobalTimePlayed, 0);
            float ThisSessionTime = Time.realtimeSinceStartup;

            PlayerPrefs.SetFloat(GlobalTimePlayed, ThisSessionTime + GlobalGameTime);
        }


    }
}
