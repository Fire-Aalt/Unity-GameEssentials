namespace RenderDream.GameEssentials
{
    public interface IDataPersistence
    {
        void LoadData(GameData data);

        void SaveData(GameData data);
    }
}