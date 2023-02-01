using System;
using Microsoft.AspNetCore.Mvc;
using UploadGoogleDrive.Models;
namespace UploadGoogleDrive.Controllers
{
    public class Functions
    {
        internal static object extractURL(string SpreadsheetID)
        {
            int indexOfd = SpreadsheetID.LastIndexOf("s/") + 2;
            int indexOfSlash = SpreadsheetID.IndexOf("?", indexOfd);
            string output = SpreadsheetID.Substring(indexOfd, indexOfSlash - indexOfd);
            return output;
        }
    }
    [ApiController]
    [Route("[controller]")]
    public class DriveController : ControllerBase
    {
        [HttpPost]
        public IActionResult Post([FromBody] Drive model)
        {
            return Ok(Functions.extractURL(model.URL));
        }
    
    }
    
}

