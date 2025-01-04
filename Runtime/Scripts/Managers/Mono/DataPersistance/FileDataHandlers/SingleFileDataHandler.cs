namespace KrasCore.Essentials
{
    public class SingleFileDataHandler<T> : FileDataHandler<T> where T : DataModel
    {
        protected readonly string fileName;

        public SingleFileDataHandler(string dataDirPath, string encryptionKey, bool useEncryption, string fileName) :
            base(dataDirPath, encryptionKey, useEncryption)
        {
            this.fileName = fileName;
        }

        public T Load()
        {
            return LoadFromFile(fileName);
        }

        public void Save(T data)
        {
            SaveToFile(data, fileName);
        }
    }
}
