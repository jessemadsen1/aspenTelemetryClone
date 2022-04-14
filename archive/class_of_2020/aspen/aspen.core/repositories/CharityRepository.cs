using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Aspen.Core.Models;
using Aspen.Core.Services;
using Dapper;
using Newtonsoft.Json;
using Npgsql;

namespace Aspen.Core.Repositories
{
    public class CharityRepository : ICharityRepository
    {
        private IMigrationService migrationService { get; }

        public CharityRepository(IMigrationService migrationService)
        {
            this.migrationService = migrationService;
        }

        public async Task<Result<Charity>> Create(Charity charity)
        {
            if(charity.Domains.Count() == 0)
                return Result<Charity>.Failure("Cannot create charity without domain");

            var existingCharityResult = await GetById(charity.CharityId);
            if(existingCharityResult.IsSucccess)
                return Result<Charity>.Failure("Cannot create charity, it already exists");

            using (var dbConnection = migrationService.GetAdminDbConnection())
            {
                var startingConnectionString = new ConnectionString(dbConnection.ConnectionString);

                var connectionBuilder = new NpgsqlConnectionStringBuilder();
                connectionBuilder.Host = startingConnectionString.Host.data;
                connectionBuilder.Port = startingConnectionString.Port.data;
                
                await createCharityDatabase(charity, dbConnection, connectionBuilder);
                await createCharityDatabaseUser(charity, dbConnection, connectionBuilder);
                var newConnectionString = new ConnectionString(connectionBuilder.ConnectionString + ";");

                charity = charity.UpdateConnectionString(newConnectionString);
                charity = await createCharityInDb(charity, dbConnection);

                using(var userDbConnection = migrationService.GetDbConnection(charity.ConnectionString))
                {
                    await userDbConnection.ExecuteAsync(
                        $"REVOKE All ON Database {charity.ConnectionString.Database.data} FROM PUBLIC;" +
                        "REVOKE All ON schema public FROM PUBLIC;"
                    );
                }
                await migrationService.ApplyMigrations(charity.ConnectionString);
                //do not create charity if it has no domains
                //very bad if this happens
                //TODO: FIX THIS
                await createDomains(charity, dbConnection);

                return Result<Charity>.Success(charity);
            }
        }

        private static async Task createCharityDatabaseUser(Charity charity, IDbConnection dbConnection, NpgsqlConnectionStringBuilder connectionBuilder)
        {
            // use stored procedure in database
            var dbUser = "charity_" + charity.CharityId.ToString().Replace("-", "");
            var password = Guid.NewGuid().ToString();
            await dbConnection.ExecuteAsync(
                @"create user " + dbUser + " with password '" + password + "';",
                new { dbUser }
            );
            connectionBuilder.Username = dbUser;
            connectionBuilder.Password = password;
        }

        private static async Task createCharityDatabase(Charity charity, IDbConnection dbConnection, NpgsqlConnectionStringBuilder connectionBuilder)
        {
            // use stored procedure in database
            var dbName = "charity_" + charity.CharityId.ToString().Replace("-", "");
            await dbConnection.ExecuteAsync(
                // TODO: check for injection attacks
                // no user input is in the dbname, but I'm still scared
                $"create database {dbName};" 
            );
            connectionBuilder.Database = dbName;
        }

        private static async Task<Charity> createCharityInDb(Charity charity, IDbConnection dbConnection)
        {
            await dbConnection.ExecuteAsync(
                @"insert into Charity (CharityId, CharityName, CharityDescription, ConnectionString)
                    values (@CharityId, @CharityName, @CharityDescription, @ConnectionString);",
                new { 
                    CharityId = charity.CharityId, 
                    CharityName = charity.CharityName, 
                    CharityDescription = charity.CharityDescription, 
                    ConnectionString = charity.ConnectionString.ToString()
                }
            );
            return charity;
        }

        private static async Task createDomains(Charity charity, IDbConnection dbConnection)
        {
            foreach (var domain in charity.Domains)
            {
                await dbConnection.ExecuteAsync(
                    @"insert into Domain (CharityId, CharityDomain)
                        values (@charityId, @charityDomain);",
                    new { charityId = charity.CharityId, charityDomain = domain.ToString() }
                );
            }
        }

        public async Task<Result<Charity>> Update(Charity charity)
        {
            using (var dbConnection = migrationService.GetAdminDbConnection())
            {
                await dbConnection.ExecuteAsync(
                    @"update Charity set
                    CharityName = @charityName,
                    CharityDescription = @charityDescription
                    where CharityId = @charityId;",
                    charity
                );

                return Result<Charity>.Success(charity);
            }
        }

        public async Task<IEnumerable<Charity>> GetAll()
        {
            using (var dbConnection = migrationService.GetAdminDbConnection())
            {
                var charityDict = new Dictionary<Guid, Charity>();

                await dbConnection.QueryAsync<Charity, Domain, Charity>(
                    @"select * from Charity as c
                        inner join Domain as d on c.charityid = d.charityid;",
                    (dbCharity, dbDomain) =>
                    {
                        charityDict[dbCharity.CharityId] = charityDict.ContainsKey(dbCharity.CharityId)
                            ? charityDict[dbCharity.CharityId].AppendDomain(dbDomain)
                            : dbCharity.AppendDomain(dbDomain);

                        return dbCharity;
                    },
                    splitOn: "charityId"
                );
                return charityDict.Values;
            }
        }

        public async Task<Result<Charity>> GetByDomain(Domain domain)
        {
            using (var dbConnection = migrationService.GetAdminDbConnection())
            {
                try
                {
                    var charityId = await getCharityIdByDomain(domain, dbConnection);
                    var charity = await getCharityWithDomain(dbConnection, charityId);
                    return Result<Charity>.Success(charity);
                }
                catch(InvalidOperationException e)
                {
                    if(e.Message == "Sequence contains no elements")
                        return Result<Charity>.Failure("Domain does not exist");
                    else
                        return Result<Charity>.Failure(e.Message);
                }
            }
        }

        public async Task<Result<Charity>> GetById(Guid charityId)
        {
            using (var dbConnection = migrationService.GetAdminDbConnection())
            {
                var charity = await getCharityWithDomain(dbConnection, charityId);
                return charity != null
                    ? Result<Charity>.Success(charity)
                    : Result<Charity>.Failure("Charity id does not exist");
            }
        }

        private static async Task<Charity> getCharityWithDomain(IDbConnection dbConnection, Guid charityId)
        {
            Charity charity = null;

            var list = await dbConnection.QueryAsync<Charity, Domain, Charity>(
                @"select * from Charity as c
                inner join Domain as d on d.charityId = c.charityId
                where c.charityid = @charityId;",
                // This lambda exists so that we never have to create a charity without a domain list
                // we need this for immutability
                // it is applied to each row in our query result
                (dbCharity, dbDomain) =>
                {
                    charity = charity == null
                        ? dbCharity.AppendDomain(dbDomain)
                        : charity.AppendDomain(dbDomain);

                    return charity;
                },
                new { charityId },
                splitOn: "charityId");
            return charity;
        }

        private static async Task<Guid> getCharityIdByDomain(Domain domain, IDbConnection dbConnection)
        {
            return await dbConnection.QueryFirstAsync<Guid>(
                @"select charityId from Domain
                    where charityDomain = @charityDomain;",
                new { charityDomain = domain.ToString() }
            );
        }

        //canidate for optimization
        public IEnumerable<string> GetDomains()
        {
            using (var dbConnection = migrationService.GetAdminDbConnection())
            {
                return dbConnection.Query<string>(
                    "select CharitySubDomain from Charity;"
                );
            }
        }

        public async Task<Result<bool>> Delete(Charity charity)
        {
            using (var dbConnection = migrationService.GetAdminDbConnection())
            {
                return await Result<Charity>.Success(charity)
                    .ApplyAsync(async c => await deleteDomains(c, dbConnection))
                    .ApplyAsync(async c => await deleteCharity(c, dbConnection));
            }
        }

        private static async Task<Result<bool>> deleteCharity(Charity charity, IDbConnection dbConnection)
        {
            var rowsAffected = await dbConnection.ExecuteAsync(
                    @"delete from Charity
                    where CharityId = @charityId;",
                    charity
                );

            if(rowsAffected == 0)
                return Result<bool>.Failure("Cannot delete non-existant charity.");
            return Result<bool>.Success(true);
        }

        private static async Task<Result<Charity>> deleteDomains(Charity charity, IDbConnection dbConnection)
        {
            await dbConnection.ExecuteAsync(
                    @"delete from Domain
                    where charityId = @charityId;",
                    charity
                );
            return Result<Charity>.Success(charity);
        }
    }
}