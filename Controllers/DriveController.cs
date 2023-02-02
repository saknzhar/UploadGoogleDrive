﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Microsoft.AspNetCore.Mvc;
using UploadGoogleDrive.Models;
namespace UploadGoogleDrive.Controllers
{
    public class WebClientWithTimeout : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest wr = base.GetWebRequest(address);
            wr.Timeout = 10000; // timeout in milliseconds (ms)
            return wr;
        }
    }
    public class Functions
    {
        internal static void DownLoadFileAsync(string URL, string filename)
        {
            var client = new WebClientWithTimeout();
            client.DownloadFile(new Uri(URL), filename);
        }
        internal static string[] ExtractFileNameAndExtension(string url)
        {
            string[] split = url.Split('/');
            string fileName = split[split.Length - 1]; // название и расширение файла
            string[] ans = fileName.Split('.'); ;
            return ans;
        }
        internal static void deleteFile(string name)
        {
            File.Delete(name);
        }
    }
    public class Variables
    {
        public const string PathToServiceAccountKeyFile = @"uploaddrive-376503-59084969f6b7.json";
        public const string ServiceAccountEmail = "uploaddrive@uploaddrive-376503.iam.gserviceaccount.com";
        public string UploadFileName = string.Empty;
        public string FolderId = string.Empty;
        
    }
    [ApiController]
    [Route("[controller]")]
    public class DriveController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] Drive model)
        {
            string[] fullname = Functions.ExtractFileNameAndExtension(model.URL);

            Functions.DownLoadFileAsync(model.URL, fullname[0]);
            var credential = GoogleCredential.FromFile(Variables.PathToServiceAccountKeyFile)
                .CreateScoped(DriveService.ScopeConstants.Drive);
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential
            });
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fullname[0], // название файлка как мы его хотим сохранить
                Parents = new List<string>{ "1CYKO45fM5lw_t20FMoUXCuc9qhUdCVyU" }
            };
            string fileType = string.Empty;

            switch (fullname[1]){
                case "doc":
                    fileType = "application/msword";
                    break;
                case "docx":
                    fileType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
                case "xls":
                    fileType = "application/vnd.ms-excel";
                    break;
                case "xlsx":
                    fileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                case "pdf":
                    fileType = "application/pdf";
                    break;

                default:
                    fileType = "text/plain";
                    break; 
            }

            await using (var fsSource = new FileStream(fullname[0], FileMode.Open, FileAccess.Read))
            {
                // Create a new file, with metadata and stream.
                var request = service.Files.Create(fileMetadata, fsSource, fileType);
                request.Fields = "*";
                var results = await request.UploadAsync(CancellationToken.None);
            }
            Functions.deleteFile(Path.Combine(Directory.GetCurrentDirectory(), fullname[0]));
           
            return Ok(model.URL);
        }
    
    }
    
}

