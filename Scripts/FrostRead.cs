using GooglePlayGames;
using UnityEngine;
using GooglePlayGames.BasicApi.SavedGame;
using System;
using System.IO;
using MessagePack;

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

    public class FrostRead<T> : FrostReadWrite, ICloudSave 
    {
        public delegate void CloudDataHasBeenRead(T data);
        public static event CloudDataHasBeenRead _CloudDataHasBeenRead;

        public bool LoadingForCompare { get; private set; }

        private ISavedGameMetadata currentGame = null;
        private string SaveFileName;
        

        /// <summary>
        /// /// Loads the game from local, local and cloud or only from the cloud
        /// based on the provided parameters
        /// </summary>
        /// <param name="FileName" The name of the file to be saved></param>
        /// <param name="ForceLocal" If true the method will only load the save from local disk></param>
        /// <param name="ForComparing" If true it will mark the method as loading for comparing purpose between local and cloud></param>
        /// <returns></returns>
        public T LoadTheGame(string FileName, bool ForceLocal = true, bool ForComparing = false)
        {

            SaveFileName = FileName;
            LoadingForCompare = ForComparing;

            if (ForceLocal) return DoLocalLoad();


            if (!ForceLocal && Social.localUser.authenticated)
            {
                LoadFromCloud();
                return default(T);
               
            }
            else
            {
                ReadOperationFailed<T>(OperationFailed.ReadFromCloud,this);
                return default(T);
            }

        }



        //Loads the game locally and returns the loaded instance
        private T DoLocalLoad()
        {
            try
            {
                FileStream file = File.Open(Application.persistentDataPath + "/" + SaveFileName, FileMode.Open);
                T save = MessagePackSerializer.Deserialize<T>(file);
                file.Close();
                Debug.Log("SaveGame Successfully Read");
                return save;
            }
            catch
            {
          
               
                ReadOperationFailed<T>(OperationFailed.ReadFromLocal);
                T o = (T)Activator.CreateInstance(typeof(T));
                return o;
            }
        }



        //Loads the game from cloud and sends an event containing the loaded instance
        // You should subscribe to this event where you want to recieve the cloud data
        private void LoadFromCloud()
        {

            try
            {
                // Local variable
                 currentGame = null;

                // Read the current data and kick off the callback chain
             
                ReadSavedGame(SaveFileName, readCallback);
            }


            //In Case of any error we just use the local save
            catch 
            {
                ReadOperationFailed<T>(OperationFailed.ReadFromCloud, this);

            }
        }



        // Callbacks for read/write operations on cloud
        public void readBinaryCallback(SavedGameRequestStatus status, byte[] data)
        {
            Debug.Log("Saved Game Binary Read: " + status.ToString());


            if (status == SavedGameRequestStatus.Success)
            {

                //Check if its the first time and data is empty
                if (data.Length == 0)
                {
                    Debug.Log("The data hasn't been saved yet on cloud. Starting Fresh.");
                    T o = (T)Activator.CreateInstance(typeof(T));
                    _CloudDataHasBeenRead(o);


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
                        ReadOperationFailed<T>(OperationFailed.ReadFromCloud, this);
                        return;
                    }
                }




            }
            else ReadOperationFailed<T>(OperationFailed.ReadFromCloud, this);
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
            else ReadOperationFailed<T>(OperationFailed.ReadFromCloud, this);

        }

        public void writeCallback(SavedGameRequestStatus status, ISavedGameMetadata game)
        {
            Debug.Log("(Progress was saved: " + status.ToString());
        }





    }
}