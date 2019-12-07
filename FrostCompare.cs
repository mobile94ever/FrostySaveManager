

using UnityEngine;

namespace FrostyMaxSaveManager
{ 
public class FrostCompare<T> : FrostReadWrite
{
        private static bool isComparing;
        private T LocalData;

  public void CompareLocalWithCloudSave()
        {
            if (isComparing) return;
            isComparing = true;

            FrostRead<T>._CloudDataHasBeenRead += CloudDataReadHandler;

            FrostRead<T> _FrostRead = new FrostRead<T>();
             LocalData = _FrostRead.LoadTheGame(true); //Local Load
            _FrostRead.LoadTheGame(false); //Try Loading Cloud Data


        }

        private void CloudDataReadHandler(T clouddata)
        {
            FrostRead<T>._CloudDataHasBeenRead -= CloudDataReadHandler;
            var LocalTimeStamp = LocalData.GetType().GetProperty(TimeStampConst);
            var CloudTimeStamp = clouddata.GetType().GetProperty(TimeStampConst);

            float LocalStampF = (float)LocalTimeStamp.GetValue(LocalData);
            float CloudStampF = (float)CloudTimeStamp.GetValue(clouddata);

            Debug.Log("LocalTimeStamp " + LocalStampF);
            Debug.Log("CloudTimeStamp " + CloudStampF);


            if (LocalStampF > CloudStampF) { Debug.Log("Local is Ahead"); }
            else Debug.Log("Cloud is ahead");

            isComparing = false;



        }


}

}
