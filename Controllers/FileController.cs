using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using P2PFileSharing.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace P2PFileSharing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private static List<FileMetadata> files = new List<FileMetadata>();

        [HttpGet]
        public ActionResult<IEnumerable<FileMetadata>> Get()
        {
            return files;
        }

        [HttpPost]
        public IActionResult UploadFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            var filePath = Path.Combine("Uploads", file.FileName);

            if (!Directory.Exists("Uploads"))
            {
                Directory.CreateDirectory("Uploads");
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            var fileMetadata = new FileMetadata
            {
                Id = files.Count + 1,
                FileName = file.FileName,
                FilePath = filePath,
                FileSize = file.Length,
                UploadedAt = DateTime.Now
            };

            files.Add(fileMetadata);

            return Ok(fileMetadata);
        }

        [HttpGet("{id}")]
        public IActionResult DownloadFile(int id)
        {
            var fileMetadata = files.FirstOrDefault(f => f.Id == id);
            if (fileMetadata == null)
                return NotFound();

            var filePath = fileMetadata.FilePath;
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", fileMetadata.FileName);
        }
    }
}