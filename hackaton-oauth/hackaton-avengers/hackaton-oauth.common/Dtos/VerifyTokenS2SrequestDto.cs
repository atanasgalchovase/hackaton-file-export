namespace hackaton_oauth.api.Controllers
{
    public class VerifyTokenS2SrequestDto
    {
        public string UserToken { get; set; }
        public string[] UserRoles { get; set; }
    }
}