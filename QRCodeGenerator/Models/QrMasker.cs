namespace QRCodeGenerator.Models
{
    public class QrMasker
    {
        public static void ApplyMask(bool[,] matrix, int size, int pattern)
        {
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (IsReserved(matrix, r, c)) continue;

                    if (ShouldMask(pattern, r, c))
                        matrix[r, c] ^= true;
                }
            }
        }

        private static bool ShouldMask(int pattern, int r, int c)
        {
            return pattern switch
            {
                0 => (r + c) % 2 == 0,
                1 => r % 2 == 0,
                2 => c % 3 == 0,
                3 => (r + c) % 3 == 0,
                _ => (r * c) % 2 + (r * c) % 3 == 0,
            };
        }

        private static bool IsReserved(bool[,] matrix, int r, int c)
        {
            // Basic logic: skip corners and timing patterns (can be improved)
            return (r < 9 && c < 9) || (r < 9 && c >= matrix.GetLength(0) - 8) || (r >= matrix.GetLength(0) - 8 && c < 9);
        }
    }
}
