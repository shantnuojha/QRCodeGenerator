namespace QRCodeGenerator.Models
{
    public class FormatInfo
    {
        public static int GetFormatBits(int eccLevel, int maskId)
        {
            int format = ((eccLevel & 0b11) << 3) | (maskId & 0b111);
            int bch = CalcBCHCode(format, 0b10100110111);
            return ((format << 10) | bch) ^ 0b101010000010010;
        }

        private static int CalcBCHCode(int data, int poly)
        {
            int dataShifted = data << 10;
            while (HighestBit(dataShifted) >= 11)
                dataShifted ^= poly << (HighestBit(dataShifted) - 10);
            return dataShifted;
        }

        private static int HighestBit(int val)
        {
            int pos = 0;
            while (val > 0)
            {
                pos++;
                val >>= 1;
            }
            return pos - 1;
        }
    }
}
