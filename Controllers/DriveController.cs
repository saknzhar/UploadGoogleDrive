﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.FileIO;
using UploadGoogleDrive.Models;
using UploadGoogleDrive.Services;
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
            System.IO.File.Delete(name);
        }
        internal static bool isGoogleDocs(string url)
        {
            Uri uri = new Uri(url);
            string host = uri.Host;
            if (host == "docs.google.com")
            {
                return true;
            }
            return false;
        }
        internal static bool isGoogleDrive(string url)
        {
            Uri uri = new Uri(url);
            string host = uri.Host;
            if (host == "drive.google.com")
            {
                return true;
            }
            return false;
        }
        internal static string ParseDocsId(string url)
        {
            string docsId = string.Empty;
            int indexOfd = url.LastIndexOf("d/") + 2;
            int indexOfSlash = url.IndexOf("/", indexOfd);
            docsId = url.Substring(indexOfd, indexOfSlash - indexOfd);
            return docsId;
        }
        internal static bool isGosZacup(string url)
        {
            Uri uri = new Uri(url);
            string host = uri.Host;
            if (host == "goszakup.gov.kz")
            {
                return true;
            }
            return false;
        }
        internal static string getID(string s)
        {
            return new Uri(s).Segments[new Uri(s).Segments.Length - 1].TrimEnd('/');
        }
        internal static void GetURlToDownload(string secondValue, string id, string FolderID)
        {
            string url = "https://goszakup.gov.kz/ru/announce/actionAjaxModalShowFiles/" + id + "/" + secondValue;
            var web = new HtmlWeb();
            var document = web.Load(url);
            var links = document.DocumentNode.SelectNodes("//a");
            var filteredHrefValues = links.Where(n => n.Attributes["href"].Value.StartsWith("https://v3bl.goszakup.gov.kz/")).Select(n => n.Attributes["href"].Value).ToList();
            var filteredNodes = links.Where(n => n.Attributes["href"].Value.StartsWith("https://v3bl.goszakup.gov.kz/")).ToList();
            for (int i = 0; i < filteredHrefValues.Count; i++){
                UploadFiles(filteredHrefValues[i] + filteredNodes[i].InnerHtml, FolderID);
            }
        }
        internal static async void UploadFiles(string url, string folderid)
        {
            string[] fullname = new string[2];
            fullname = ExtractFileNameAndExtension(url);
            Functions.DownLoadFileAsync(url, fullname[0]);
            var credential = GoogleCredential.FromFile(Variables.PathToServiceAccountKeyFile)
                    .CreateScoped(DriveService.ScopeConstants.Drive);
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential
            });
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fullname[0] + "." + fullname[1], // название файлка как мы его хотим сохранить
                Parents = new List<string> { folderid }
            };
            string fileType = SetType(fullname[1]);

            await using (var fsSource = new FileStream(fullname[0], FileMode.Open, FileAccess.Read))
            {
                // Create a new file, with metadata and stream.
                var request = service.Files.Create(fileMetadata, fsSource, fileType);
                request.Fields = "*";
                var results = await request.UploadAsync(CancellationToken.None);
            }
            Functions.deleteFile(Path.Combine(Directory.GetCurrentDirectory(), fullname[0]));

        }
        internal static string CreateFolder(string FolderName, string ParentFolderID)
        {
            string id = String.Empty;
            var credential = GoogleCredential.FromFile(Variables.PathToServiceAccountKeyFile).CreateScoped(DriveService.ScopeConstants.Drive);
            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "test"
            });
            var searchQuery = "mimeType='application/vnd.google-apps.folder' and trashed = false and name='" + FolderName + "' and parents in  '" + ParentFolderID + "'";
            var requestSearch = service.Files.List();
            requestSearch.Q = searchQuery;
            var result = requestSearch.Execute();
            if (result.Files.Count > 0)
            {
                id = result.Files[0].Id;
            }
            else
            {
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = FolderName,
                    MimeType = "application/vnd.google-apps.folder",
                    Parents = new List<string> { ParentFolderID }
                };
                var request = service.Files.Create(fileMetadata);
                request.Fields = "id";
                var file = request.Execute();
                id = file.Id;
            }
            return id;
        }
        internal static string SetType(string fullname)
        {
            string fileType= String.Empty;
            switch (fullname)
            {
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
                case "zip":
                    fileType = "application/zip";
                    break;
                case "jpg":
                    fileType = "image/jpeg";
                    break;
                case "jpeg":
                    fileType = "image/jpeg";
                    break;
                case "rar":
                    fileType = "application/vnd.rar";
                    break;
                case "png":
                    fileType = "image/png";
                    break;
                default:
                    fileType = "text/plain";
                    break;
            }
            return fileType;
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
        private readonly IGoogleApiService _googleApiService;
        public DriveController (IGoogleApiService googleApiService)
        {
            _googleApiService = googleApiService;
        }
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] Models.Drive model)
        {

            var service = _googleApiService.Service;
            string[] fullname = new string[2];
            var RootfolderId = "1CYKO45fM5lw_t20FMoUXCuc9qhUdCVyU";
            if (model.URL.StartsWith("http://") || model.URL.StartsWith("https://"))
            {
                fullname = Functions.ExtractFileNameAndExtension(model.URL);
                if (Functions.isGoogleDocs(model.URL))
                {
                    _googleApiService.DoSomething();
                    var fileId = Functions.ParseDocsId(model.URL);
                    var file = service.Files.Get(fileId).Execute();
                    var copy = new Google.Apis.Drive.v3.Data.File
                    {
                        Name = file.Name,
                        Parents = new List<string> { RootfolderId },
                    };
                    copy = service.Files.Copy(copy, fileId).Execute();
                }
                else if (Functions.isGoogleDrive(model.URL))
                {
                    var fileId = Functions.ParseDocsId(model.URL);
                    var file = service.Files.Get(fileId).Execute();

                    var copiedFile = new Google.Apis.Drive.v3.Data.File();
                    copiedFile.Name = file.Name;
                    copiedFile.Parents = new List<string> { RootfolderId };

                    var request = service.Files.Copy(copiedFile, fileId).Execute();
                }
                else if (Functions.isGosZacup(model.URL))
                {
                    model.URL += "?tab=documents";
                    var web = new HtmlWeb();
                    var document = web.Load(model.URL);

                    string id = Functions.getID(model.URL);

                    var buttons = document.DocumentNode.SelectNodes("//button");
                    string s = "";
                    int i = 0;
                    var TimeExceeded = document.DocumentNode.SelectNodes("//table//tr//td//div");
                    if (TimeExceeded == null)
                    {
                        i = 0;
                    }
                    else
                    {
                        i = 1;
                    }
                    var tables = document.DocumentNode.SelectNodes("//table//tr//td");
                    for (; i < tables.Count; i++)
                    {
                        string td = tables[i].InnerHtml;
                        string[] DocName = new string[td.Length];
                        string[] separators = { "<tr>", "</tr>", "Нет", "Да", "                                    ", "                                " };
                        string[] parts = td.Split(separators, StringSplitOptions.None);
                        s += parts[1];
                        if (parts[1] != "")
                        {
                            s += "\n";
                        }
                    }
                    string[] separ = { "\n" };
                    string[] TableName = s.Split(separ, StringSplitOptions.None);
                    string RootFolderID = Functions.CreateFolder(id, RootfolderId);
                    int j = 0;
                    foreach (var button in buttons)
                    {
                        if (button.Attributes.Contains("onclick"))
                        {
                            var onClickValue = button.Attributes["onclick"].Value;
                            var match = Regex.Match(onClickValue, @"actionModalShowFiles\((\d+),(\d+)\)");
                            if (match.Success)
                            {
                                var secondValue = match.Groups[2].Value;
                                string d = Functions.CreateFolder(TableName[j], RootFolderID);
                                Functions.GetURlToDownload(secondValue, id, d);
                                j++;
                            }
                        }
                    }

                }
                else
                {
                    Functions.DownLoadFileAsync(model.URL, fullname[0]);
                    Functions.UploadFiles(model.URL, RootfolderId);
                }
                return Ok("Загружен файл по ссылке");
            }
            else if (model.URL.StartsWith("/") || model.URL.StartsWith("\\") || model.URL[1] == ':')
            {
                fullname = Functions.ExtractFileNameAndExtension(model.URL);
                string filename = fullname[0];
                string fileType = fullname[1];

                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = fullname[0] + "." + fullname[1], // название файлка как мы его хотим сохранить
                    Parents = new List<string> { RootfolderId }
                };

                fileType = Functions.SetType(fullname[1]);

                await using (var fsSource = new FileStream(model.URL, FileMode.Open, FileAccess.Read))
                {
                    // Create a new file, with metadata and stream.
                    var request = service.Files.Create(fileMetadata, fsSource, fileType);
                    request.Fields = "*";
                    var results = await request.UploadAsync(CancellationToken.None);
                }
                return Ok("Загружен локальный файл");
            }
            else
            {
                return BadRequest("Это не ссылка");
            }
        }

    }

}

