using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Aspen.Core.Models
{
    public class User
    {
        public User(Guid id, string firstname, string lastname, string username, string hashedpassword, string role, byte[] salt, string token)
        {
            this.Id = id;
            FirstName = firstname;
            LastName = lastname;
            Username = username;
            Role = role;
            HashedPassword = hashedpassword;
            Salt = salt;
            Token = token;
        }

        private User(Guid id, string firstname, string lastname, string username, byte[] salt, string hashedpassword)
        {
            this.Id = id;
            FirstName = firstname;
            LastName = lastname;
            Username = username;
            HashedPassword = hashedpassword;
            Salt = salt;
            Token = "";
        }

        private User(Guid id, string firstname, string lastname, string username, byte[] salt, string hashedpassword, string role)
        {
            this.Id = id;
            FirstName = firstname;
            LastName = lastname;
            Username = username;
            HashedPassword = hashedpassword;
            Salt = salt;
            Role = role;
            Token = "";
        }

        public Guid Id { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Username { get; }
        public string HashedPassword { get; }
        public string Role { get; }
        [JsonIgnore]
        public byte[] Salt { get; }
        [JsonIgnore]
        public string Token { get; }

        public User UpdateFirstName(string newFirstName)
        {
            return new User(Id, newFirstName, LastName, Username, HashedPassword, Role, Salt, Token);
        }

        public User UpdateToken(string newToken)
        {
            return new User(Id, FirstName, LastName, Username, HashedPassword, Role, Salt, newToken);
        }

        public User UpdateId(Guid guid)
        {
            return new User(guid, FirstName, LastName, Username, HashedPassword, Role, Salt, Token);

        }

        public User UpdatePassword(byte[] newSalt, string newPassword)
        {
            return new User(Id, FirstName, LastName, Username, newSalt, newPassword);
        }
    }
}