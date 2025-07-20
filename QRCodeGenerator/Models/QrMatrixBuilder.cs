namespace QRCodeGenerator.Models
{
    public class QrMatrixBuilder
    {
        private const int Version = 1;
        private const int Size = 21;
        private const int FormatInfo = 0b111011111000100; // Level L + Mask 0

        public bool[,] BuildMatrix(List<bool> dataBits)
        {
            var matrix = new bool[Size, Size];
            var reserved = new bool[Size, Size];

            PlaceFinderPatterns(matrix, reserved);
            PlaceTimingPatterns(matrix, reserved);
            PlaceFormatInfo(matrix);

            var dataModules = PlaceData(dataBits, reserved);
            ApplyMask(dataModules, 0); // Apply Mask 0

            Merge(matrix, dataModules);

            return matrix;
        }

        private void PlaceFinderPatterns(bool[,] matrix, bool[,] reserved)
        {
            void DrawFinder(int row, int col)
            {
                for (int r = -1; r <= 7; r++)
                {
                    for (int c = -1; c <= 7; c++)
                    {
                        int rr = row + r;
                        int cc = col + c;
                        if (rr < 0 || rr >= Size || cc < 0 || cc >= Size)
                            continue;

                        bool isBorder = (r == -1 || r == 7 || c == -1 || c == 7);
                        bool isDark = (r >= 0 && r <= 6 && c >= 0 && c <= 6 &&
                                      (r == 0 || r == 6 || c == 0 || c == 6 || (r >= 2 && r <= 4 && c >= 2 && c <= 4)));

                        matrix[rr, cc] = isDark;
                        reserved[rr, cc] = true;
                    }
                }
            }

            DrawFinder(0, 0);
            DrawFinder(0, Size - 7);
            DrawFinder(Size - 7, 0);
        }

        private void PlaceTimingPatterns(bool[,] matrix, bool[,] reserved)
        {
            for (int i = 8; i < Size - 8; i++)
            {
                bool val = i % 2 == 0;
                matrix[6, i] = val;
                matrix[i, 6] = val;
                reserved[6, i] = true;
                reserved[i, 6] = true;
            }
        }

        private void PlaceFormatInfo(bool[,] matrix)
        {
            int format = FormatInfo;

            for (int i = 0; i < 15; i++)
            {
                bool bit = ((format >> i) & 1) == 1;

                // Top-left
                if (i < 6)
                    matrix[8, i] = bit;
                else if (i < 8)
                    matrix[8, i + 1] = bit;
                else
                    matrix[14 - i, 8] = bit;

                // Mirror
                if (i < 8)
                    matrix[Size - 1 - i, 8] = bit;
                else
                    matrix[8, Size - 15 + i] = bit;
            }
        }

        private bool[,] PlaceData(List<bool> dataBits, bool[,] reserved)
        {
            var matrix = new bool[Size, Size];
            int row = Size - 1;
            int col = Size - 1;
            int dir = -1;
            int bitIdx = 0;

            while (col > 0)
            {
                if (col == 6) col--; // Skip timing col

                for (int i = 0; i < Size; i++)
                {
                    int r = row + dir * i;
                    if (r < 0 || r >= Size) continue;

                    for (int j = 0; j < 2; j++)
                    {
                        int c = col - j;
                        if (!reserved[r, c] && bitIdx < dataBits.Count)
                        {
                            matrix[r, c] = dataBits[bitIdx++];
                        }
                    }
                }

                col -= 2;
                dir *= -1;
            }

            return matrix;
        }

        private void ApplyMask(bool[,] matrix, int maskPattern)
        {
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    // Skip fixed patterns (handled in reserved)
                    bool mask = (r + c) % 2 == 0; // Mask 0
                    if (mask)
                    {
                        matrix[r, c] ^= true;
                    }
                }
            }
        }

        private void Merge(bool[,] baseMatrix, bool[,] overlay)
        {
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    if (!baseMatrix[r, c] && overlay[r, c])
                        baseMatrix[r, c] = true;
                }
            }
        }
    }
}
