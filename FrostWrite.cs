using GooglePlayGames;
using UnityEngine;
using GooglePlayGames.BasicApi.SavedGame;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using MessagePack;
using System.Reflection;


namespace FrostyMaxSaveManager
{
    public class FrostWrite<T> : FrostReadWrite, ICloudSave
    {

        private ISavedGameMetadata currentGame = null;
        private T InstanceToSave;
        public static bool IsWriting;

        public void SaveTheGame(T _InstanceToSave)
        {
            if (IsWriting) WriteOperationFailed(OperationFailed.AlreadyWriting);
            IsWriting = true;

            InstanceToSave = _InstanceToSave;

            var TimeStamp = InstanceToSave.GetType().GetProperty(TimeStampConst);

            if (TimeStamp == null)
            {
                Debug.LogError("The instance to serialize must have a public property called "+ TimeStampConst+" of type float");
                IsWriting = false;
                WriteOperationFailed(OperationFailed.AlreadyWriting);
                return;
            }


            SetTotalPlayedTime();
            TimeStamp.SetValue(InstanceToSave, GetTotalPlayedTime());


            DoLocalSave(InstanceToSave);

            //Also try to save the game to cloud
            if (IsUserAutheticated())
                SaveToCloud();

        }

      

        private void DoLocalSave(T InstanceToSave)
        {
            try
            {
                //InstanceToSave.DateGameSaved = DateTime.Now.ToBinary().ToString();
                byte[] bytes = MessagePackSerializer.Serialize(InstanceToSave);
                FileStream file = File.Create(Application.persistentDataPath + "/" + SaveFilePlayer);
                file.Write(bytes, 0, bytes.Length);
                file.Close();
                IsWriting = false;
            }
            catch { IsWriting = false; WriteOperationFailed(OperationFailed.WriteToLocal); }
        }




        private void SaveToCloud()
        {
            try
            {
               
                // Read the current data and kick off the callback chain
    
                ReadSavedGame(SaveFilePlayer, readCallback);
            }

            catch { WriteOperationFailed(OperationFailed.WriteToCloud); }
        }





        // Callbacks for read/write operations
        public void readBinaryCallback(SavedGameRequestStatus status, byte[] data)
        {
            Debug.Log("Saved Game Binary Read: " + status.ToString());


            if (status == SavedGameRequestStatus.Success)
            {

                BinaryFormatter bff = new BinaryFormatter();

               // InstanceToSave.DateGameSaved = DateTime.Now.ToBinary().ToString();
                byte[] saveddata = MessagePackSerializer.Serialize(InstanceToSave);

                WriteSavedGame(currentGame, saveddata, writeCallback);




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
