using GooglePlayGames;
using UnityEngine;
using GooglePlayGames.BasicApi.SavedGame;
using System.Runtime.Serialization.Formatters.Binary;
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
    public class FrostWrite<T> : FrostReadWrite, ICloudSave
    {

        private ISavedGameMetadata currentGame = null;
        private T InstanceToSave;
        private string SaveFileName;
        public static bool IsWritingLocal,isWritingCloud;



        /// <summary>
        /// //// Saves the game either to local, local and cloud or only to cloud based on
        /// the provided parameteres.
        /// </summary>
        /// <param name="_InstanceToSave" The instance you want to serialize and save></param>
        /// <param name="FileName" The name that you want to save your file as></param>
        /// <param name="SaveToCloudAlso" Also tries to save to cloud along with local></param>
        /// <param name="SaveOnlyToCloud" Only save the file to the cloud></param>
        public void SaveTheGame(T _InstanceToSave,string FileName,bool SaveToCloudAlso=false,bool SaveOnlyToCloud = false)
        {
            
            if (SaveOnlyToCloud) SaveToCloudAlso = true;

            //If we only want to save it to the cloud 
            if (SaveOnlyToCloud && isWritingCloud) { WriteOperationFailed<T>(OperationFailed.AlreadyWriting); return; }
            else if (SaveOnlyToCloud && !isWritingCloud) { isWritingCloud = true; }

            //If we want to write to local only or to both local and cloud
            //Note ** : In this case, cloud will only be written if local can be written
            else if(IsWritingLocal) { WriteOperationFailed<T>(OperationFailed.AlreadyWriting); return; }

            else if(!IsWritingLocal)
            {
                if(SaveToCloudAlso && !isWritingCloud) { isWritingCloud = true; }
                else if (SaveToCloudAlso && isWritingCloud) { SaveToCloudAlso = false; }
                IsWritingLocal = true;
            }


            


            InstanceToSave = _InstanceToSave;
            SaveFileName = FileName;


            var TimeStamp = InstanceToSave.GetType().GetProperty(TimeStampConst);

            if (TimeStamp == null)
            {
                Debug.LogError("The instance to serialize must have a public property called " + TimeStampConst + " of type float. \n " +
                    "ForExample: public float TimeStamp {get {return xxx;} set{ xxx=value; }}");
                
                IsWritingLocal = false;
                return;
            }

    

            SetTotalPlayedTime();
            TimeStamp.SetValue(InstanceToSave, GetTotalPlayedTime());

         

            if(!SaveOnlyToCloud)
            DoLocalSave(InstanceToSave);

            //Also try to save the game to cloud
            if (SaveToCloudAlso && IsUserAutheticated())
                SaveToCloud();

        }

      
        
        private void DoLocalSave(T InstanceToSave)
        {
          

            try
            {
              
                byte[] bytes = MessagePackSerializer.Serialize(InstanceToSave);
                FileStream file = File.Create(Application.persistentDataPath + "/" + SaveFileName);
                file.Write(bytes, 0, bytes.Length);
                file.Close();
                IsWritingLocal = false;
                Debug.Log("Local Save Successfull");
            }
            catch {  WriteOperationFailed<T>(OperationFailed.WriteToLocal); }
        }




        private void SaveToCloud()
        {
            Debug.Log("Save To CLoud Started");
            try
            {
               
                // Read the current data and kick off the callback chain
    
                ReadSavedGame(SaveFileName, readCallback);
            }

            catch { WriteOperationFailed<T>(OperationFailed.WriteToCloud); }
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
            isWritingCloud = false;
            Debug.Log("(Cloud save successfull: " + status.ToString());
        }


    }
}
