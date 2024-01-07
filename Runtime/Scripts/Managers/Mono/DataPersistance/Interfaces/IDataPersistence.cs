namespace RenderDream.GameEssentials
{
    public interface IDataPersistence<T> where T : IGameData
    {
        void LoadData(T data);

        void SaveData(T data);
    }
}