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
using TesteBackendEnContact.Core.Domain.ContactBook;
using TesteBackendEnContact.Core.Domain.ContactBook.Contact;
using TesteBackendEnContact.Core.Interface.ContactBook;
using TesteBackendEnContact.Database;
using TesteBackendEnContact.Repository.Interface;

namespace TesteBackendEnContact.Repository
{
    public class ContactBookRepository : IContactBookRepository
    {
        private readonly DatabaseConfig databaseConfig;
        private readonly EnContactContext _context;

        public ContactBookRepository(DatabaseConfig databaseConfig, EnContactContext encontactContext)
        {
            this.databaseConfig = databaseConfig;
            _context = encontactContext;
        }

        public async Task<IContactBook> SaveAsync(IContactBook contactBook)
        {
            try
            {
                using var connection = new NpgsqlConnection(databaseConfig.ConnectionString);

                var sqlInsert = "insert into \"ContactBook\"(\"Name\") values ('{0}') returning *;";
                sqlInsert = String.Format(sqlInsert, contactBook.Name);
                var insert = await connection.QueryFirstAsync<ContactBook>(sqlInsert);
                
                if (insert != null)
                {
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

        public async Task<IContactBook> EditAsync(int id, IContactBook contactBook)
        {
            try
            {
                using var connection = new NpgsqlConnection(databaseConfig.ConnectionString);
                var initialContactB = await GetAsync(id);

                string sqlEdit = "update \"ContactBook\" set \"Name\" = '{0}' where \"Id\" = {1} returning *;";
                sqlEdit = String.Format(sqlEdit, contactBook.Name ?? initialContactB.Name, id);

                var edited = await connection.QueryFirstAsync<ContactBook>(sqlEdit);
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

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                using var connection = new NpgsqlConnection(databaseConfig.ConnectionString);

                var sql = "delete from \"ContactBook\" where \"Id\" = {0};";
                sql = String.Format(sql, id);

                var deleted = await connection.ExecuteAsync(sql);

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

        public async Task<IEnumerable<IContactBook>> GetAllAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(databaseConfig.ConnectionString);

                var query = "select * from \"ContactBook\";";
                var result = await connection.QueryAsync<ContactBook>(query);
                                                
                return result.ToList();
            }
            catch (Exception)
            {                
                return null;
            }
            
        }
        public async Task<IContactBook> GetAsync(int id)
        {
            //var list = await GetAllAsync();
            //return list.ToList().Where(item => item.Id == id).FirstOrDefault();
            // Acho que a consulta unitária no banco deve gastar menos tempo
            try
            {
                using var connection = new NpgsqlConnection(databaseConfig.ConnectionString);

                var query = "select * from \"ContactBook\" where \"Id\" = {0};";
                query = String.Format(query, id);

                var result = await connection.QuerySingleAsync<ContactBook>(query);

                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<bool> ContactsCSV(int id, string path)
        {
            try
            {
                var contactBook = _context.ContactBooks.Where(c => c.Id == id).FirstOrDefault();
                List<Contact> contactList = new();
                if (contactBook != null)
                {
                    contactList = await _context.Contacts.Where(c => c.ContactBookId == contactBook.Id).ToListAsync();
                }
                else
                {
                    return false;
                }

                CultureInfo.CurrentCulture = new CultureInfo("pt-BR", false);

                var clean = path.Trim();
                var check = clean.Substring(clean.Length - 1, 1);
                if (check == @"\") clean = clean.Remove(clean.Length - 1);

                using var fs = new FileStream(clean + $@"\{contactBook.Name}.csv", FileMode.Create, FileAccess.ReadWrite);
                using (var writer = new StreamWriter(fs))
                using (var csv = new CsvWriter(writer, CultureInfo.CurrentCulture))
                {
                    //csv.WriteHeader<Contact>();
                    //csv.NextRecord();
                    csv.WriteRecords(contactList);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }            
        }
    }
}
