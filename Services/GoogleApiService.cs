using System;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using UploadGoogleDrive.Controllers;

namespace UploadGoogleDrive.Services
{
	public class GoogleApiService : IGoogleApiService
	{
        public string PathToServiceAccountKeyFile { get; set; }
        public DriveService Service { get; set; }
        public void DoSomething()
        {
            var credential = GoogleCredential.FromFile(PathToServiceAccountKeyFile)
                .CreateScoped(DriveService.ScopeConstants.Drive);
            DriveService service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential
            });
        }
    }
}

