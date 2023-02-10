using System;
using Google.Apis.Drive.v3;

namespace UploadGoogleDrive.Services
{
	public interface IGoogleApiService
	{
        DriveService GetService();
    }
}

