using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ManoVecinaAPI.DTOs.Gigs
{
    public class UploadGigImageDto
    {
        [FromForm(Name = "file")]
        public IFormFile File { get; set; }
    }
}