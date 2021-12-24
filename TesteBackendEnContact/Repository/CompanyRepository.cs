using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Domain.ContactBook.Company;
using TesteBackendEnContact.Core.Interface.ContactBook.Company;
using TesteBackendEnContact.Database;
using TesteBackendEnContact.Repository.Interface;

namespace TesteBackendEnContact.Repository
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly DatabaseConfig databaseConfig;

        public CompanyRepository(DatabaseConfig databaseConfig)
        {
            this.databaseConfig = databaseConfig;
        }

        public async Task<ICompany> SaveAsync(ICompany company)
        {
            try
            {
                using var connection = new NpgsqlConnection(databaseConfig.ConnectionString);

                string verifyContactBook = "select * from \"ContactBook\" where \"Id\" = {0};";
                verifyContactBook = String.Format(verifyContactBook, company.ContactBookId);
                var verified = await connection.QueryAsync(verifyContactBook);

                if (verified.Any())
                {
                    string insert = "insert into \"Company\"(\"ContactBookId\", \"Name\") values ({0}, '{1}') returning *;";
                    insert = String.Format(insert, company.ContactBookId, company.Name);
                    var inserted = await connection.QuerySingleAsync<Company>(insert);

                    return inserted;
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

        public async Task<ICompany> EditAsync(int id, ICompany company)
        {
            try
            {
                using var connection = new NpgsqlConnection(databaseConfig.ConnectionString);

                var oldCompany = await GetAsync(id);

                string editedCommand = "update \"Company\" set \"Name\" = '{0}' WHERE \"Id\" = {1} and \"ContactBookId\" = {2} returning *;";
                editedCommand = String.Format(editedCommand, company.Name ?? oldCompany.Name, oldCompany.Id, oldCompany.ContactBookId);

                var result = await connection.QuerySingleOrDefaultAsync<Company>(editedCommand);

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

                string deleteCommand = "delete from \"Company\" where \"Id\" = {0};";
                deleteCommand = String.Format(deleteCommand, id);

                var deleted = await connection.ExecuteAsync(deleteCommand);

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

        public async Task<IEnumerable<ICompany>> GetAllAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(databaseConfig.ConnectionString);

                var query = "select * from \"Company\";";
                var result = await connection.QueryAsync<Company>(query);

                return result;
            }
            catch (Exception)
            {
                return null;
            }            
        }

        public async Task<ICompany> GetAsync(int id)
        {
            try
            {
                using var connection = new NpgsqlConnection(databaseConfig.ConnectionString);

                var query = "select * from \"Company\" where \"Id\"= {0};";
                query = String.Format(query, id);

                var result = await connection.QuerySingleOrDefaultAsync<Company>(query);

                return result;
            }
            catch (Exception)
            {
                return null;
            }           
        }
    }
}
