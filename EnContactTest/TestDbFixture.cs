using Microsoft.EntityFrameworkCore;
using System;
using TesteBackendEnContact.Core.Domain.ContactBook;
using TesteBackendEnContact.Core.Domain.ContactBook.Company;
using TesteBackendEnContact.Core.Domain.ContactBook.Contact;
using TesteBackendEnContact.Database;

namespace EnContactTest
{
    public partial class TestDbFixture : IDisposable
    {
        public EnContactContext Db { get; private set; }
        //public DatabaseConfig Config { get; private set; }

        public TestDbFixture()
        {
            // var guid = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<EnContactContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            //Config.ConnectionString = $"DataSource={guid};";
            Db = new EnContactContext(options);            
            Db.Database.EnsureCreated();

            Company[] companies = new Company[]
            {
                new Company(5, "Company1"),
                new Company(4, "Company2"),
                new Company(3, "Company3"),
                new Company(2, "Company4"),
                new Company(1, "Company5")                
            };

            ContactBook[] contactBooks = new ContactBook[]
            {
                new ContactBook("ContactBook1"),
                new ContactBook("ContactBook2"),
                new ContactBook("ContactBook3"),
                new ContactBook("ContactBook4"),
                new ContactBook("ContactBook5")
            };

            Contact[] contacts = new Contact[]
            {
                new Contact(1, 5, "Contact1", "99999999991", "teste1@teste.com", "Address Contact 1"),
                new Contact(2, 4, "Contact2", "99999999992", "teste2@teste.com", "Address Contact 2"),
                new Contact(3, 3, "Contact3", "99999999993", "teste3@teste.com", "Address Contact 3"),
                new Contact(4, 2, "Contact4", "99999999994", "teste4@teste.com", "Address Contact 4"),
                new Contact(5, 1, "Contact5", "99999999995", "teste5@teste.com", "Address Contact 5"),
            };

            Db.Companies.AddRange(companies);
            Db.ContactBooks.AddRange(contactBooks);
            Db.Contacts.AddRange(contacts);
            Db.SaveChanges();
        }

        public void Dispose()
        {
            Db.Database.EnsureDeleted();
            Db.Dispose();
        }
    }
}
