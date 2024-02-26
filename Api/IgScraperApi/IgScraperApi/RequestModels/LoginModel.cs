using System.ComponentModel.DataAnnotations;

namespace IgScraperApi.RequestModels
{
    public class LoginModel
    {
        [Required]
        public string Sessionid { get; set; }
    }
}
