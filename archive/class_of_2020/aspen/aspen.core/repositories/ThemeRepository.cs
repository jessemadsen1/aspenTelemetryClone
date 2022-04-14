using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Aspen.Core.Models;
using Aspen.Core.Services;
using Dapper;
using Newtonsoft.Json;

namespace Aspen.Core.Repositories
{

    public class ThemeRepository : IThemeRepository
    {
        private readonly IMigrationService migrationService;

        public ThemeRepository(IMigrationService migrationService)
        {
            this.migrationService = migrationService;
        }

        public async Task<Result<Theme>> Create(Theme theme, ConnectionString connectionString)
        {
            using (var dbConnection = migrationService.GetDbConnection(connectionString))
            {
                await dbConnection.ExecuteAsync(
                    @"insert into Theme (primarymaincolor, primarylightcolor, primarycontrastcolor, secondarymaincolor, fontfamily)
                        values (@primarymaincolor, @primarylightcolor, @primarycontrastcolor, @secondarymaincolor, @fontfamily);",
                    theme
                );
            }
            return Result<Theme>.Success(theme);
        }

        public async Task<Result<Theme>> GetByCharity(Charity charity)
        {
            using (var dbConnection = migrationService.GetDbConnection(charity.ConnectionString))
            {
                return Result<Theme>.Success(await dbConnection.QueryFirstAsync<Theme>(
                    @"select primarymaincolor, primarylightcolor, primarycontrastcolor, secondarymaincolor, fontfamily
                        from Theme;"
                ));
            }
        }

        public async Task<Result<bool>> Update(Theme theme, ConnectionString connectionString)
        {
            using (var dbConnection = migrationService.GetDbConnection(connectionString))
            {
                var affecteRows = await dbConnection.ExecuteAsync(
                    @"update Theme set
                        PrimaryMainColor = @PrimaryMainColor,
                        PrimaryLightColor = @PrimaryLightColor,
                        PrimaryContrastColor = @PrimaryContrastColor,
                        SecondaryMainColor = @SecondaryMainColor,
                        FontFamily = @FontFamily;",
                    theme
                );
                if(affecteRows == 1)
                    return Result<bool>.Success(true);
                else
                    return Result<bool>.Failure("Failed to update theme");
            }
        }
    }
}