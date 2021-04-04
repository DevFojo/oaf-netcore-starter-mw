using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Common.Interfaces;

namespace Application.Common.Interfaces
{
    public interface ICouchbaseRepository<TEntity> where TEntity : AuditableEntity, ICouchbaseEntity
    {
        Task<IEnumerable<TEntity>> FindAllDocuments(int limit = 20, int offset = 0);
        Task<IAsyncEnumerable<int>> Count();
        Task<TEntity> FindOneDocument(string id);
        Task<TEntity> InsertDocument(TEntity entity);
        Task<TEntity> InsertSubDocument(string documentId, string subDocumentId, dynamic subDocumentValue);
        Task<string> RemoveDocument(string id, TEntity entity);
        Task<TEntity> UpsertDocument(string id, TEntity entity);
        Task<TEntity> UpsertSubDocument(string documentId, string subDocumentId, dynamic subDocumentValue);
        Task RemoveAllDocuments();
    }
}
