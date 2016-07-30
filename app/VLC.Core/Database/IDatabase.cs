namespace VLC.Database
{
    interface IDatabase
    {
        void Initialize();
        void Drop();
        void DeleteAll();
    }
}
