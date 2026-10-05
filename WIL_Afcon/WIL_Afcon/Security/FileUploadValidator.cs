namespace WIL_Afcon.Security
{
    // Checks uploaded documents (payslips, IRP5s, sick notes) before they are
    // stored. The browser-supplied content type and file name are not trusted:
    // the extension must be on the allow-list AND the first bytes of the file
    // must match that format's signature.
    public static class FileUploadValidator
    {
        public const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        private static readonly Dictionary<string, byte[][]> Signatures =
            new(StringComparer.OrdinalIgnoreCase)
            {
                [".pdf"] = new[] { new byte[] { 0x25, 0x50, 0x44, 0x46 } },             // %PDF
                [".png"] = new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47 } },             // \x89PNG
                [".jpg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
                [".jpeg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
            };

        private static readonly Dictionary<string, string> ContentTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                [".pdf"] = "application/pdf",
                [".png"] = "image/png",
                [".jpg"] = "image/jpeg",
                [".jpeg"] = "image/jpeg",
            };

        // Returns an error message, or null when the file is acceptable.
        public static string? Validate(IFormFile file)
        {
            if (file.Length == 0)
            {
                return "The selected file is empty.";
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return "Files must be 5 MB or smaller.";
            }

            var extension = Path.GetExtension(file.FileName);

            if (!Signatures.TryGetValue(extension, out var signatures))
            {
                return "Only PDF, JPG and PNG files can be uploaded.";
            }

            var header = new byte[8];
            using (var stream = file.OpenReadStream())
            {
                var read = stream.Read(header, 0, header.Length);
                header = header[..read];
            }

            var matches = signatures.Any(signature =>
                header.Length >= signature.Length &&
                header.AsSpan(0, signature.Length).SequenceEqual(signature));

            return matches
                ? null
                : "The file's contents do not match its type. Upload a genuine PDF, JPG or PNG.";
        }

        // Content type to store/serve, derived from the validated extension
        // rather than the value the browser sent.
        public static string ContentTypeFor(string fileName)
        {
            return ContentTypes.TryGetValue(Path.GetExtension(fileName), out var type)
                ? type
                : "application/octet-stream";
        }

        // Strips any path the client included and keeps a safe display name.
        public static string SafeFileName(string fileName)
        {
            var name = Path.GetFileName(fileName);
            return string.IsNullOrWhiteSpace(name) ? "document" : name;
        }
    }
}
