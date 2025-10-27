using System.ComponentModel.DataAnnotations;

namespace MyLoginApp.Data
{
    public class UserModel
    {
        public class SignupModel
        {
            [Required] public string FullName { get; set; }
            [Required] public string Username { get; set; }
            [Required] public string Password { get; set; }
            [Required][EmailAddress] public string Email { get; set; }
            [Required] public string PhoneNumber { get; set; }
        }

        public class LoginModel
        {
            [Required] public string username { get; set; }
            [Required] public string password { get; set; }
        }
        public class UserDto
        {
            public int Id { get; set; }
            public string FullName { get; set; }
            public string Username { get; set; }
            public string Password { get; set; } // hashed password
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
        }
    }
}
