using TesteBackendEnContact.Core.Interface.ContactBook;

namespace TesteBackendEnContact.Core.Domain.ContactBook
{
    public class ContactBook : IContactBook
    {
        public int Id { get;  set; }
        public string Name { get;  set; }

        public ContactBook(string name)
        {
            Name = name;
        }

        public ContactBook(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
