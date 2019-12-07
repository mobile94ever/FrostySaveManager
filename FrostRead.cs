using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;
using GooglePlayGames.BasicApi.SavedGame;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using MessagePack;



namespace FrostyMaxSaveManager
{

    public class FrostRead<T> : FrostReadWrite, ICloudSave
    {
        public delegate void CloudDataHasBeenRead(T data);
        public static event CloudDataHasBeenRead _CloudDataHasBeenRead;

        private ISavedGameMetadata currentGame = null;
        



        public T LoadTheGame(bool ForceLocal=true)
        {

            if (ForceLocal) return DoLocalLoad();


            if (!ForceLocal && Social.localUser.authenticated)
            {
                LoadFromCloud();
                return default(T);
               
            }
            else
            {
                ReadOperationFailed(OperationFailed.ReadFromCloud);
                return default(T);
            }

        }



        private T DoLocalLoad()
        {
            try
            {
                FileStream file = File.Open(Application.persistentDataPath + "/" + SaveFilePlayer, FileMode.Open);
                T save = MessagePackSerializer.Deserialize<T>(file);
                file.Close();
                return save;
            }
            catch
            {
                ReadOperationFailed(OperationFailed.ReadFromLocal);
                return default(T);
            }
        }




        private void LoadFromCloud()
        {

            try
            {
                // Local variable
                 currentGame = null;

                // Read the current data and kick off the callback chain
             
                ReadSavedGame(SaveFilePlayer, readCallback);
            }


            //In Case of any error we just use the local save
            catch 
            {
                ReadOperationFailed(OperationFailed.ReadFromCloud);

            }
        }



        // Callbacks for read/write operations
        public void readBinaryCallback(SavedGameRequestStatus status, byte[] data)
        {
            Debug.Log("Saved Game Binary Read: " + status.ToString());


            if (status == SavedGameRequestStatus.Success)
            {

                //Check if its the first time and data is empty
                if (data.Length == 0)
                {
                    Debug.Log("The data hasn't been saved yet on cloud. Starting Fresh.");

                }
                else
                {
                    try
                    {


                        T save = MessagePackSerializer.Deserialize<T>(data);
                        _CloudDataHasBeenRead(save);


                    }
                    catch (Exception e)
                    {

                        Debug.Log("Saved Game Write: convert exception " + e);
                        return;
                    }
                }




            }
        }

        public void readCallback(SavedGameRequestStatus status, ISavedGameMetadata game)
        {

            if (status == SavedGameRequestStatus.Success)
            {
                // Read the binary game data
                currentGame = game;
                PlayGamesPlatform.Instance.SavedGame.ReadBinaryData(game,
                                                    readBinaryCallback);
            }
        }

        public void writeCallback(SavedGameRequestStatus status, ISavedGameMetadata game)
        {
            Debug.Log("(Progress was saved: " + status.ToString());
        }





    }
}