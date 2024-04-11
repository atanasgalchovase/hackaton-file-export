namespace hackaton_file_export.common.Dtos
{
    internal class VerifyTokenRequestDto
    {
        public string UserToken { get; set; }
        public string[] UserRoles { get; set; }
    }
}