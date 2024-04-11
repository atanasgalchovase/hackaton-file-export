namespace hackaton_oauth.common.Dtos
{
    public class AuthenticateResponseDto
    {
        public Guid Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Token { get; set; }
    }
}