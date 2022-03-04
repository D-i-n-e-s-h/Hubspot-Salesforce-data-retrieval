using MongoDB.Driver;
using System.Collections.Generic;

namespace POC_dotnet.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> users;
        public UserService(IUserDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString ?? "mongodb://localhost:27017");
            var database = client.GetDatabase(settings.DatabaseName ?? "Identity_server");

            users = database.GetCollection<User>(settings.UsersCollectionName ?? "Users");

        }

        public List<User> Get()
        {
            List<User> employees;
            employees = users.Find(emp => true).ToList();
            return employees;
        }

        public User Get(string id) =>
            users.Find(emp => emp.Id == id).FirstOrDefault();

        public bool Authenticate(User userDetails)
        {
            return users.Find(emp => emp.Email == userDetails.Email && emp.password == userDetails.password).Count() > 0;
        }
    }
}
