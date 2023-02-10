using System;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using UploadGoogleDrive.Controllers;

namespace UploadGoogleDrive.Services
{
	public class GoogleApiService : IGoogleApiService
	{
        public DriveService GetService()
        {
            string PathToServiceAccountKeyFile = @"uploaddrive-376503-59084969f6b7.json";
            var credential = GoogleCredential.FromFile(PathToServiceAccountKeyFile)
                .CreateScoped(DriveService.ScopeConstants.Drive);
            return new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential
            });
            
        }
    }
}

