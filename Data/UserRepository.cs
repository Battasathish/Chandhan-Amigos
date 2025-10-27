using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

public interface IUserRepository
{
    Task<bool> RegisterUserAsync(string FullName,string username, string password, String Email,String PhoneNumber);
    Task CreateUserAsync(string username, string password); // helper for seeding
    Task<MyLoginApp.Data.UserModel.UserDto> GetUserByUsernameAsync(string username);
}

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;
    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<bool> RegisterUserAsync(string fullName, string username, string password, string email, string phoneNumber)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        // First check if username already exists
        const string checkSql = "SELECT COUNT(*) FROM Users WHERE Username = @username";
        await using var checkCmd = new SqlCommand(checkSql, conn);
        checkCmd.Parameters.AddWithValue("@username", username);

        var exists = (int)await checkCmd.ExecuteScalarAsync();
        if (exists > 0)
        {
            // Username already taken
            return false;
        }

        // Hash the password before saving
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        const string insertSql = @"
        INSERT INTO Users (FullName, Username, Password, Email, PhNo) 
        VALUES (@FullName, @Username, @Password, @Email, @PhoneNumber)";

        await using var insertCmd = new SqlCommand(insertSql, conn);
        insertCmd.Parameters.AddWithValue("@FullName", fullName);
        insertCmd.Parameters.AddWithValue("@Username", username);
        insertCmd.Parameters.AddWithValue("@Password", hashedPassword);
        insertCmd.Parameters.AddWithValue("@Email", email);
        insertCmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

        int rows = await insertCmd.ExecuteNonQueryAsync();
        return rows > 0; // true if inserted successfully
    }

    public async Task<MyLoginApp.Data.UserModel.UserDto> GetUserByUsernameAsync(string username)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = @"SELECT Id, FullName, Username, Password, Email, PhNo 
                         FROM Users WHERE Username = @username";

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@username", username);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new MyLoginApp.Data.UserModel.UserDto
            {
                Id = reader.GetInt32(0),
                FullName = reader.GetString(1),
                Username = reader.GetString(2),
                Password = reader.GetString(3), // this is hashed
                Email = reader.GetString(4),
                PhoneNumber = reader.GetString(5)
            };
        }

        return null; // user not found
    }


    public async Task CreateUserAsync(string username, string password)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = "INSERT INTO Users (Username, Password) VALUES (@username, @hash)";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@username", username);
        cmd.Parameters.AddWithValue("@hash", hash);
        await cmd.ExecuteNonQueryAsync();
    }
}

