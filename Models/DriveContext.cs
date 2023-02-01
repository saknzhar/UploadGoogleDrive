using System;
using Microsoft.EntityFrameworkCore;
namespace UploadGoogleDrive.Models
{
	public class DriveContext : DbContext
	{

        public DbSet<DriveContext> DriveItems { get; set; }
    }
}

