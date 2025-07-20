namespace QRCodeGenerator.Models
{
    public class QrViewModel
    {
        public bool[,] Matrix { get; set; }
        public int Size => Matrix?.GetLength(0) ?? 0;
    }
}
