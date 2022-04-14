using System;
using System.Data;
using Aspen.Core.Data;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Aspen.Integration.Helpers
{
    public static class MigrationHelper
    {
        private static string defaultConnectionString = "Host=localhost; Port=5433; Database=admin; Username=Aspen; Password=Aspen;";
        public static string ConnectionString = Environment.GetEnvironmentVariable("INTEGRATION_TEST_CONNECTION")??defaultConnectionString;
    }
}