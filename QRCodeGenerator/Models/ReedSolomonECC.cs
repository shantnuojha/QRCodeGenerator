namespace QRCodeGenerator.Models
{
    public static class ReedSolomonECC
    {
        private const int FieldSize = 256;
        private const int Primitive = 0x11D;

        private static byte[] expTable = new byte[FieldSize * 2];
        private static byte[] logTable = new byte[FieldSize];

        static ReedSolomonECC()
        {
            InitTables();
        }

        private static void InitTables()
        {
            byte x = 1;
            for (int i = 0; i < FieldSize; i++)
            {
                expTable[i] = x;
                logTable[x] = (byte)i;

                int xi = x << 1;
                if ((xi & 0x100) != 0)
                {
                    xi ^= Primitive;
                }
                x = (byte)(xi & 0xFF); // Ensure stays in byte range
            }

            // Extend expTable for easy mod 255 indexing
            for (int i = FieldSize; i < expTable.Length; i++)
            {
                expTable[i] = expTable[i - 256];
            }
        }

        private static byte GFMul(byte a, byte b)
        {
            if (a == 0 || b == 0) return 0;
            return expTable[logTable[a] + logTable[b]];
        }

        /// <summary>
        /// Generate ECC bytes using Reed-Solomon for QR version 1-L (7% recovery)
        /// </summary>
        public static List<byte> GenerateECC(byte[] data)
        {
            int eccLength = 7; // For version 1-L: 19 data bytes + 7 ECC
            var generator = BuildGeneratorPoly(eccLength);
            var ecc = new byte[eccLength];

            foreach (var b in data)
            {
                byte factor = (byte)(b ^ ecc[0]);
                for (int i = 0; i < eccLength - 1; i++)
                {
                    ecc[i] = (byte)(ecc[i + 1] ^ GFMul(generator[i], factor));
                }
                ecc[eccLength - 1] = GFMul(generator[eccLength - 1], factor);
            }

            return ecc.ToList();
        }

        /// <summary>
        /// Builds a generator polynomial of given degree
        /// </summary>
        private static byte[] BuildGeneratorPoly(int degree)
        {
            List<byte> poly = new List<byte> { 1 };

            for (int i = 0; i < degree; i++)
            {
                List<byte> next = new List<byte>(new byte[poly.Count + 1]);

                for (int j = 0; j < poly.Count; j++)
                {
                    next[j] ^= GFMul(poly[j], expTable[i]);
                    next[j + 1] ^= poly[j];
                }

                poly = next;
            }

            return poly.ToArray();
        }
    }
}
