using Microsoft.AspNetCore.Mvc;
using QRCodeGenerator.Models;

namespace QRCodeGenerator.Controllers
{
    public class QRController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [Route("qr/display")]
        [HttpPost]
        public IActionResult Display(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return RedirectToAction("Index");

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text cannot be empty.");

            // Limit to Version 1-L capacity (17 bytes in Byte mode)
            if (text.Length > 17)
                throw new ArgumentException("Text too long. Max 17 characters for QR version 1-L.");

            var matrixBuilder = new QrMatrixBuilder();

            List<byte> data = QrEncoder.Encode(text);
            List<byte> codewords = ReedSolomonECC.GenerateECC(data.ToArray());

            var bitStream = QrEncoder.ToBitStream(data, codewords);
            var matrix = matrixBuilder.BuildMatrix(bitStream);

            var model = new QrViewModel { Matrix = matrix };
            return View("Result", model);
        }
    }
}
