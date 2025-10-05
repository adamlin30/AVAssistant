using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace AVAssistant.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FilesController : ControllerBase
    {
        [HttpGet("folders")]
        public IActionResult GetFolders([FromQuery] string rootPaths)
        {
            if (string.IsNullOrWhiteSpace(rootPaths))
                return BadRequest(new { message = "請至少指定一個根目錄" });

            var paths = rootPaths.Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                                 .Select(p => p.Trim())
                                 .ToArray();

            var folders = new List<string>();
            var actressDict = new Dictionary<string, List<string>>();

            foreach (var root in paths)
            {
                if (!Directory.Exists(root)) continue;

                try
                {
                    var dirs = Directory.GetDirectories(root)
                                        .Where(d =>
                                        {
                                            var attr = new DirectoryInfo(d).Attributes;
                                            var name = Path.GetFileName(d);

                                            // 排除隱藏或系統資料夾
                                            if (attr.HasFlag(FileAttributes.Hidden) || attr.HasFlag(FileAttributes.System))
                                                return false;

                                            // 排除常見包裝或不必要的資料夾
                                            string[] excludePatterns = { "[REPACK]", "[WEBRip]", "[1080p]", "[5.1]", "[YTS.MX]" };
                                            if (excludePatterns.Any(pat => name.Contains(pat)))
                                                return false;

                                            return true;
                                        })
                                        .ToArray();

                    folders.AddRange(dirs);

                    foreach (var dir in dirs)
                    {
                        var folderName = Path.GetFileName(dir);

                        // 解析女優名字 (假設資料夾名稱是 "影片名稱 - 女優1, 女優2")
                        var parts = folderName.Split(" - ", 2);
                        if (parts.Length == 2)
                        {
                            var actressNames = parts[1].Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                                                       .Select(a => a.Trim());
                            foreach (var actress in actressNames)
                            {
                                if (!actressDict.ContainsKey(actress))
                                    actressDict[actress] = new List<string>();
                                actressDict[actress].Add(dir);
                            }
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }

            folders.Sort();
            return Ok(new
            {
                count = folders.Count,
                folders = folders,
                actressDict = actressDict
            });
        }


        [HttpGet("all")]
        public IActionResult GetAllFiles([FromQuery] string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                return BadRequest("資料夾不存在");

            var files = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly);
            return Ok(files);
        }

        [HttpGet("image")]
        public IActionResult GetImage([FromQuery] string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                return NotFound();

            var bytes = System.IO.File.ReadAllBytes(filePath);
            return File(bytes, "image/jpeg");
        }

        [HttpPost("play")]
        public IActionResult PlayMovie([FromBody] string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                return BadRequest("檔案不存在");

            try
            {
                var potPlayerPath = @"C:\Program Files\DAUM\PotPlayer\PotPlayerMini64.exe";
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = potPlayerPath,
                    Arguments = $"\"{filePath}\"",
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(startInfo);
                return Ok($"已播放：{filePath}");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"啟動 PotPlayer 失敗：{ex.Message}");
            }
        }
    }
}
