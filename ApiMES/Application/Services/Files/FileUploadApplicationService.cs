using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace ApiMES.Application.Services.Files
{
    public class FileUploadApplicationService
    {
        public async Task<Dictionary<string, object>> UploadFileAsync(IFormFile file)
        {
            var result = new Dictionary<string, object>();

            try
            {
                if (file == null || file.Length == 0)
                {
                    result["message"] = "No file uploaded.";
                    result["error"] = true;
                    return result;
                }

                string filename = Path.GetFileName(file.FileName);
                string path = Path.Combine("C:\\uploads");

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string filePath = Path.Combine(path, filename);

                if (File.Exists(filePath))
                {
                    string backupName = Path.Combine(
                        path,
                        $"{Path.GetFileNameWithoutExtension(filename)}_ver_{Guid.NewGuid()}{Path.GetExtension(filename)}"
                    );
                    File.Move(filePath, backupName);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                result["message"] = "Upload Image Successful";
                result["error"] = false;
                result["image"] = GetImageBase64(filePath);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result["message"] = "Upload failed.";
                result["error"] = true;
                return result;
            }
        }

        private string GetImageBase64(string filePath)
        {
            try
            {
                using var stream = File.OpenRead(filePath);

                // Phát hiện định dạng ảnh
                IImageFormat? format = Image.DetectFormat(stream);
                if (format == null)
                {
                    Console.WriteLine("[GetImageBase64] Unsupported or unknown image format.");
                    return string.Empty;
                }

                stream.Position = 0; // Reset stream để load ảnh lại
                using var image = Image.Load<Rgba32>(stream);

                using var ms = new MemoryStream();
                image.Save(ms, format); // Lưu đúng định dạng
                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetImageBase64] Error: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
