

using UnityEngine;

namespace FrostyMaxSaveManager
{

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

    public class FrostCompare<T> : FrostReadWrite
{
        public delegate void CloudDataPulled();
        public static event CloudDataPulled _CloudDataPulled;

        public static bool isComparing;
        private T LocalData;
        private string SaveFileName;


        public void CompareLocalWithCloudSave(string FileName)
        {
            if (isComparing) return;
            isComparing = true;
            SaveFileName = FileName;

            FrostRead<T>._CloudDataHasBeenRead += CloudDataReadHandler;

             FrostRead<T> _FrostRead = new FrostRead<T>();
             LocalData = _FrostRead.LoadTheGame(SaveFileName,true,true); //Local Load Synchronously 
            _FrostRead.LoadTheGame(SaveFileName,false,true); //Try Loading Cloud Data Asynchronously (Will only run after local data has been obtained)


        }

        /// <summary>
        /// /// The event is recieved upon cloud data downloaded and is ready to be compared 
        /// with the local data. The TimeStamps of both are compared and the data with a higher
        /// stamp overwrites the other one. In case the cloud was ahead of local, an event 
        /// _CloudDataPulled is sent after overwriting the local data. (Can be used to restart the
        /// scene for a fresh session?)
        /// </summary>
        /// <param name="clouddata" Data recovered from the cloud></param>
        /// 
        private void CloudDataReadHandler(T clouddata)
        {
            FrostRead<T>._CloudDataHasBeenRead -= CloudDataReadHandler;
            if (clouddata == null) { Debug.LogError("Event received after cloud data read had an null instance."); isComparing = false; return; }

           
            var LocalTimeStamp = LocalData.GetType().GetProperty(TimeStampConst);
            var CloudTimeStamp = clouddata.GetType().GetProperty(TimeStampConst);

            if (LocalTimeStamp == null || CloudTimeStamp == null)
            {
                Debug.LogError("The instance to serialize must have a public property called " + TimeStampConst + " of type float. \n " +
                    "ForExample: public float TimeStamp {get {return xxx;} set{ xxx=value; }}");

                isComparing = false;
                return;
            }


            float LocalStampF = (float)LocalTimeStamp.GetValue(LocalData);
            float CloudStampF = (float)CloudTimeStamp.GetValue(clouddata);

            Debug.Log("LocalTimeStamp " + LocalStampF);
            Debug.Log("CloudTimeStamp " + CloudStampF);


            if (LocalStampF > CloudStampF)
            {
                Debug.Log("Local is Ahead. Trying to update cloud aswell");
                FrostWrite<T> _FrostWrite = new FrostWrite<T>();
                _FrostWrite.SaveTheGame(clouddata, SaveFileName, true, true);
            }


            else if (CloudStampF > LocalStampF)
            {
                Debug.Log("Cloud is ahead. Replacing Local File With Cloud One's");
                PlayerPrefs.SetFloat(TimeStampConst, CloudStampF);
                FrostWrite<T> _FrostWrite = new FrostWrite<T>();
                _FrostWrite.SaveTheGame(clouddata, SaveFileName);

                //Event
                try { _CloudDataPulled(); } catch { Debug.LogWarning("CloudDataPulled has no subscribers"); };

            }
            else
                Debug.Log("Cloud and Local data are the same");

            isComparing = false;



        }


}

}
