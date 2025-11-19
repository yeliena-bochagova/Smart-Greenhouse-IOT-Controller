using System.Collections.Concurrent;
using System.Security.Claims;

namespace SmartGreenhouse.Web.Services
{
    public record UserRecord(string Username, string FullName, string Email, string Phone, string PasswordHash, string Role);

    public class UserStore
    {
        private readonly ConcurrentDictionary<string, UserRecord> _users = new();

        public bool TryAdd(UserRecord user) => _users.TryAdd(user.Username.ToLowerInvariant(), user);

        public bool UsernameExists(string username) => _users.ContainsKey(username.ToLowerInvariant());

        public UserRecord? GetByUsername(string username)
        {
            _users.TryGetValue(username.ToLowerInvariant(), out var user);
            return user;
        }

        public IEnumerable<UserRecord> GetAll() => _users.Values;
    }
}
