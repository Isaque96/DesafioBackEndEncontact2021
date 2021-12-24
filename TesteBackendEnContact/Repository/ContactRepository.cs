using CsvHelper;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Domain.ContactBook.Contact;
using TesteBackendEnContact.Core.Interface.ContactBook.Contact;
using TesteBackendEnContact.Database;
using TesteBackendEnContact.Repository.Interface;

namespace TesteBackendEnContact.Repository
{
    public class ContactRepository : IContactRepository
    {
        private readonly DatabaseConfig databaseConfig;
        private readonly EnContactContext _context;

        public ContactRepository(DatabaseConfig databaseConfig, EnContactContext context)
        {
            this.databaseConfig = databaseConfig;
            _context = context;
        }

        public ContactRepository(EnContactContext context)
        {
            _context = context;
        }

        public async Task<dynamic> GetContacts(string something, int skip, int take)
        {
            try
            {
                List<Contact> listOfAll = new();

                var byName = await _context.Contacts.AsNoTracking().Where(x => x.Name.Contains(something)).ToListAsync();
                foreach (var x in byName)
                {
                    listOfAll.Add(x);
                }

                var byPhone = await _context.Contacts.AsNoTracking().Where(x => x.Phone.Contains(something)).ToListAsync();
                foreach (var x in byPhone)
                {
                    listOfAll.Add(x);
                }

                var byEmail = await _context.Contacts.AsNoTracking().Where(x => x.Email.Contains(something)).ToListAsync();
                foreach (var x in byPhone)
                {
                    listOfAll.Add(x);
                }

                var byAddress = await _context.Contacts.AsNoTracking().Where(x => x.Address.Contains(something)).ToListAsync();
                foreach (var x in byAddress)
                {
                    listOfAll.Add(x);
                }

                var byCompanyName = await _context.Companies.AsNoTracking().Where(x => x.Name.Contains(something)).Select(x => x.Id).ToListAsync();
                foreach (var y in byCompanyName)
                {
                    var allByCompany = await _context.Contacts.AsNoTracking().Where(x => x.CompanyId == y).ToListAsync();
                    foreach (var x in allByCompany)
                    {
                        listOfAll.Add(x);
                    }
                }

                decimal totalAmount = listOfAll.Count;
                decimal currentPage = skip / take;
                decimal totalPages;
                if (totalAmount % take == 0)
                {
                    totalPages = totalAmount / take;
                }
                else
                {
                    totalPages = Math.Truncate(totalAmount / take) + 1;
                }

                dynamic result = new
                {
                    totalAmount,
                    totalPages,
                    currentPage,
                    skip,
                    take,
                    contact = listOfAll.Distinct().Skip(skip).Take(take).ToList()
                };

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<dynamic>();
            }
        }

        public async Task<IEnumerable<IContact>> GetAllAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(databaseConfig.ConnectionString);

                string contactList = "select * from \"Contact\";";

                var result = await connection.QueryAsync<Contact>(contactList);

                if (result != null)
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                using var connection = new NpgsqlConnection(databaseConfig.ConnectionString);

                var deleteSql = "delete from \"Contact\" where \"Id\" = {0};";
                deleteSql = String.Format(deleteSql, id);

                var deleted = await connection.ExecuteAsync(deleteSql);

                if (deleted == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IContact> EditAsync(int id, IContact contact)
        {
            try
            {
                using var connection = new NpgsqlConnection(databaseConfig.ConnectionString);
                var initialContact = await GetAsync(id);

                string sqlEdit = "update \"Contact\" set \"Name\" = '{0}', \"Phone\" = '{1}', \"Email\" = '{2}', \"Address\" = '{3}' where \"Id\" = {4} returning *;";
                sqlEdit = String.Format(sqlEdit, contact.Name ?? initialContact.Name, contact.Phone ?? initialContact.Phone, contact.Email ?? initialContact.Email, contact.Address ?? initialContact.Address, id);

                var edited = await connection.QueryFirstAsync<Contact>(sqlEdit);
                if (edited == null)
                {
                    return null;
                }
                else
                {
                    return edited;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IContact> GetAsync(int id)
        {
            try
            {
                using var connection = new NpgsqlConnection(databaseConfig.ConnectionString);

                string contact = "select * from \"Contact\" where \"Id\" = {0};";
                contact = String.Format(contact, id);

                var result = await connection.QueryFirstOrDefaultAsync<Contact>(contact);

                if (result != null)
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IContact> SaveAsync(IContact contact)
        {
            try
            {
                using var connection = new NpgsqlConnection(databaseConfig.ConnectionString);

                var checkedContactBook = await _context.ContactBooks.AnyAsync(x => x.Id == contact.ContactBookId);
                //var contactBookCheck = "select exists(select \"Id\" from \"ContactBook\" where \"Id\" = {0});";
                //contactBookCheck = String.Format(contactBookCheck, contact.ContactBookId);
                //var checkedContactBook = await connection.QuerySingleAsync<bool>(contactBookCheck);

                var checkedCompany = await _context.Companies.AnyAsync(x => x.Id == contact.CompanyId);
                //var companyCheck = "select exists(select \"Id\" from \"Company\" where \"Id\" = {0});";
                //companyCheck = String.Format(companyCheck, contact.CompanyId);
                //var checkedCompany = await connection.QuerySingleAsync<bool>(companyCheck);

                if (checkedContactBook && checkedCompany)
                {
                    var sqlInsert = "insert into \"Contact\"(\"ContactBookId\", \"CompanyId\", \"Name\", \"Phone\", \"Email\", \"Address\") values ({0}, {1}, '{2}', '{3}', '{4}', '{5}') returning *;";
                    sqlInsert = String.Format(sqlInsert, contact.ContactBookId, contact.CompanyId, contact.Name, contact.Phone, contact.Email, contact.Address);
                    var insert = await connection.QueryFirstAsync<Contact>(sqlInsert);

                    return insert;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<int> GetContactBookId(string name)
        {
            try
            {
                //using var connection = new NpgsqlConnection(databaseConfig.ConnectionString);

                //string sqlCommand = "select \"Id\" from \"ContactBook\" where \"Name\" = '" + name + "';";
                //var result = await connection.QueryFirstOrDefaultAsync<int>(sqlCommand);
                var id = await _context.ContactBooks.Where(x => x.Name.Contains(name)).Select(x => x.Id).FirstAsync();

                return id;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        public async Task<int> GetCompanyId(string name)
        {
            try
            {
                //using var connection = new NpgsqlConnection(databaseConfig.ConnectionString);

                //string sqlCommand = "select \"Id\" from \"Company\" where \"Name\" = '" + name + "';";
                //var result = await connection.QueryFirstOrDefaultAsync<int>(sqlCommand);
                var id = await _context.Companies.Where(x => x.Name.Contains(name)).Select(x => x.Id).FirstAsync();

                return id;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        public async Task<IEnumerable<IContact>> GetCSVFile(string path)
        {
            try
            {
                //using var conn = new NpgsqlConnection(databaseConfig.ConnectionString);
                using var reader = new StreamReader(path);
                using var csv = new CsvReader(reader, new CultureInfo("pt-BR", false));


                var records = new List<Contact>();
                csv.Read();
                csv.ReadHeader();
                bool isRecordBad = false;
                while (csv.Read())
                {
                    isRecordBad = false;
                    csv.Context.Configuration.BadDataFound = context =>
                    {
                        isRecordBad = true;
                    };
                    if (isRecordBad) continue;

                    var str1 = csv.GetField<string>("CompanyId");
                    var str2 = csv.GetField<string>("ContactBookId");
                    int companyId = 0;
                    if (str1 != "") companyId = await GetCompanyId(str1);
                    if (str2 == "") continue;

                    var contactBookId = await GetContactBookId(str2);
                    //string addContact = "";

                    if (contactBookId == 0) continue;

                    string name = csv.GetField<string>("Name");
                    string phone = csv.GetField<string>("Phone");
                    string email = csv.GetField<string>("Email");
                    string address = csv.GetField<string>("Address");

                    Contact contact;
                    if (companyId == 0)
                    {
                        contact = new(contactBookId, null, name, phone, email, address);
                        //addContact = "insert into \"Contact\"(\"ContactBookId\", \"Name\", \"Phone\", \"Email\", \"Address\") values ({0}, '{1}', '{2}', '{3}', '{4}') returning \"Id\";";
                        //addContact = String.Format(addContact, contactBookId, name, phone, email, address);
                    }
                    else
                    {
                        contact = new(companyId, contactBookId, name, phone, email, address);
                        //addContact = "insert into \"Contact\"(\"ContactBookId\", \"CompanyId\", \"Name\", \"Phone\", \"Email\", \"Address\") values ({0}, {1}, '{2}', '{3}', '{4}', '{5}') returning \"Id\";";
                        //addContact = String.Format(addContact, contactBookId, companyId, name, phone, email, address);
                    }
                    //var id = await conn.QuerySingleAsync<int>(addContact);
                    var response = await _context.Contacts.AddAsync(contact);
                    _context.SaveChanges();
                    //Contact record = new(id, contactBookId, companyId, name, phone, email, address);

                    records.Add(contact);
                }

                return records;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<dynamic> GetContactsByCompany(int companyId)
        {
            try
            {
                var listCompleted = await _context.Companies.Where(x => x.Id == companyId)
                    .Join(_context.ContactBooks, company => company.ContactBookId, contactBook => contactBook.Id,
                    (company, contactBook) => new
                    {
                        company.Id,
                        company.Name,
                        contactBook
                    }).ToListAsync();
                    
                var result = listCompleted.GroupJoin(_context.Contacts, joined => joined.contactBook.Id, contact => contact.ContactBookId,
                    (joined, contacts) => new
                    {
                        joined.Id,
                        joined.Name,
                        joined.contactBook,
                        contacts
                    });

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
