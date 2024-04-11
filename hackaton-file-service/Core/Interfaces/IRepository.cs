namespace hackathon_file_import.Core.Interfaces
{
    public interface IRepository<T>
    {
        void Add(T entity);
    }
}
