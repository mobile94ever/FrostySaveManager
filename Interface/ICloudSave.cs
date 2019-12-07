
using GooglePlayGames.BasicApi.SavedGame;


public interface ICloudSave
{
    void writeCallback(SavedGameRequestStatus status, ISavedGameMetadata game);
    void readBinaryCallback(SavedGameRequestStatus status, byte[] data);

    void readCallback(SavedGameRequestStatus status, ISavedGameMetadata game);
}
