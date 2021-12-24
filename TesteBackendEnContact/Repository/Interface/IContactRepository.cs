using System.Collections.Generic;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Interface.ContactBook.Contact;

namespace TesteBackendEnContact.Repository.Interface
{
    public interface IContactRepository
    {
        Task<IContact> SaveAsync(IContact contact);
        Task<bool> DeleteAsync(int id);
        Task<IContact> EditAsync(int id, IContact contact);
        Task<IEnumerable<IContact>> GetAllAsync();
        Task<IContact> GetAsync(int id);
        Task<IEnumerable<IContact>> GetCSVFile(string path);
        Task<dynamic> GetContacts(string something, int skip, int take);
        Task<dynamic> GetContactsByCompany(int companyId);
    }
}
