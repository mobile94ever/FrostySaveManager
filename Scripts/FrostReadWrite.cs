using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using System;
using UnityEngine;

/**********************************************************************;
* Program name      : FrostySaveManager
*
* Dependencies      : Google Play Services with save gamed enabled from console
*                     as well as initizlied within the code | MessagePacker for 
*                     binary serialization (optional), code can be changed if you 
*                     want to use default binary unity serializer
*
* Author            : Talha Hanif
*
* Date created      : 07-Dec-19
*
* Purpose           : Save and synchronizes data between PlayServices Cloud and Local Disk
*
* Revision History  : 
*
* Date Author      Ref Revision (Date in YYYYMMDD format)
* 20191207    Initial Build      1      In Testing Phase.
*
/**********************************************************************/

namespace FrostyMaxSaveManager
{

    public enum OperationFailed { ReadFromCloud,WriteToCloud,ReadFromLocal,WriteToLocal,AlreadyWriting}


    public abstract class FrostReadWrite
    {
        /// <summary>
        /// /// You can change the following constants based on your need
        /// The constants include the name of all your save files
        /// </summary>
        public const string SaveFilePlayer = "playerlocalsave.save";



        /// <summary>
        /// Default definition is recommanded. However, these can also be 
        /// changed if needed
        /// </summary>
        public const string GlobalTimePlayed = "GlobalTimePlayed";
        public const string TimeStampConst = "TimeStamp";


        // No need to modify these
        #region General PlayServices Read/Write Methods

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
        #endregion



        // Once authenticated, the method will still return true if user device goes offline.
        protected bool IsUserAutheticated()
        {
            return Social.localUser.authenticated;
        }




        // Any Read/Write operation faliure will be handled here.

        #region Read/Write Faliure Callbacks
        protected void WriteOperationFailed<T>(OperationFailed FailedOperation)
        {
            Debug.LogWarning("Write operation of type : " + FailedOperation + " failed");
            switch(FailedOperation)
            {
                case OperationFailed.WriteToLocal: { FrostWrite<T>.IsWritingLocal = false; break; }
                case OperationFailed.WriteToCloud: { FrostWrite<T>.isWritingCloud = false; break; }
            }
        }

        protected void ReadOperationFailed<T>(OperationFailed FailedOperation,FrostRead<T> Instance=null)
        {
            Debug.LogWarning("Read Operation of type: " + FailedOperation + " failed");
            switch (FailedOperation)
            {
                case OperationFailed.ReadFromCloud:
                    {
                        if (Instance == null) Debug.LogError("Instance parameter of ReadOperationFailed was null. This should never happen");
                        else if (Instance.LoadingForCompare)
                           FrostCompare<T>.isComparing = false;
                            break;
                    }
                
            }
        }
        #endregion


       /// <summary>
       /// Following methods are used to set and get the total time user has played the game
       /// The time is saved with the save file for the sake of synchronization and is retrieved
       /// when loaded
       /// </summary>
       /// <returns></returns>

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
