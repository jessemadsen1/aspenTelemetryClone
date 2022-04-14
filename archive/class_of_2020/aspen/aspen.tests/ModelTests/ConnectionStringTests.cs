using System;
using Aspen.Core.Models;
using FluentAssertions;
using NUnit.Framework;

namespace Aspen.Tests.ModelTests
{
    public class ConnectionStringTests
    {
        [Test]
        public void CanCreateValidConnectionString()
        {
            var validConnectionString = "Host=database;Port=5432;Database=admin;Username=Aspen;Password=Aspen;";
            Action act = () => new ConnectionString(validConnectionString);

            act.Should().NotThrow<Exception>();
        }

        [Test]
        public void InvalidPortThrowsArgumentError()
        {
            var invalidConnectionString = "Host=database;Port=70000;Database=admin;Username=Aspen;Password=Aspen;";
            Action act = () => new ConnectionString(invalidConnectionString);

            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("Invalid port");
        }

        [Test]
        public void InvalidHostThrowsError()
        {
            var invalidConnectionString = "Host=database%$#$%#$%#$%#;Port=5432;Database=admin;Username=Aspen;Password=Aspen;";
            Action act = () => new ConnectionString(invalidConnectionString);

            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("Invalid Host");
        }

        [Test]
        public void InvalidDatabaseThrowsErrors()
        {
            var invalidConnectionString = "Host=database;Port=5432;Database=0admin;Username=Aspen;Password=Aspen;";
            Action act = () => new ConnectionString(invalidConnectionString);

            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("Invalid database");
        }

        [Test]
        public void InvalidUserIdThrowsErrors()
        {
            var invalidConnectionString = "Host=database;Port=5432;Database=admin;Username=0Aspen;Password=Aspen;";
            Action act = () => new ConnectionString(invalidConnectionString);

            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("Invalid Username");
        }

        [Test]
        public void InvalidPasswordThrowsErrors()
        {
            var invalidConnectionString = "Host=database;Port=5432;Database=admin;Username=Aspen;Password=Aspe n;";
            Action act = () => new ConnectionString(invalidConnectionString);

            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("Invalid password");
        }

        [Test]
        public void CanGetCorrectConnectionStringBack()
        {
            var validConnectionString = "SSL Mode=Require; Trust Server Certificate=True;Host=database;Port=5432;Database=admin;Username=Aspen;Password=;";
            var connectionString = new ConnectionString(validConnectionString);

            connectionString.ToString().Should().Be(validConnectionString);
        }

        [Test]
        public void AnotherTestForUserIds()
        {
            var validConnectionString = "SSL Mode=Require; Trust Server Certificate=True;Host=database;Port=5432;Database=admin;Username=charity_7cc774fd93c945d2a48520d6b2ce05e6;Password=;";
            var connectionString = new ConnectionString(validConnectionString);

            connectionString.ToString().Should().Be(validConnectionString);
        }
        
    }
}