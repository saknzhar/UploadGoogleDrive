using System;
using Google.Apis.Drive.v3;

namespace UploadGoogleDrive.Services
{
	public interface IGoogleApiService
	{
        string PathToServiceAccountKeyFile { get; set; }
        DriveService Service { get; set; }
        void DoSomething();
    }
}

