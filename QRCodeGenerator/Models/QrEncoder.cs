using System.Text;

namespace QRCodeGenerator.Models
{
    public static class QrEncoder
    {
        public static List<byte> Encode(string input)
        {
            var bits = new List<bool>();

            // Mode indicator for byte mode: 0100
            bits.AddRange(new[] { false, true, false, false });

            // Character count (8 bits for version 1–9)
            byte length = (byte)input.Length;
            bits.AddRange(AddBits(length, 8));

            // Data bits (8 bits per char)
            foreach (char c in input)
            {
                bits.AddRange(AddBits((byte)c, 8));
            }

            // Terminator (up to 4 bits, if needed)
            int maxBits = 8 * 19;
            int remaining = maxBits - bits.Count;
            if (remaining > 0)
            {
                bits.AddRange(new bool[Math.Min(4, remaining)]);
            }

            // Pad to multiple of 8
            while (bits.Count % 8 != 0)
                bits.Add(false);

            // Convert to bytes
            var codewords = new List<byte>();
            for (int i = 0; i < bits.Count; i += 8)
            {
                byte b = 0;
                for (int j = 0; j < 8; j++)
                {
                    b <<= 1;
                    if (bits[i + j]) b |= 1;
                }
                codewords.Add(b);
            }

            // Pad with 0xEC, 0x11 alternately to reach 19 bytes
            byte[] padBytes = { 0xEC, 0x11 };
            int padIndex = 0;
            while (codewords.Count < 19)
            {
                codewords.Add(padBytes[padIndex % 2]);
                padIndex++;
            }

            return codewords.ToList();
        }

        private static List<bool> AddBits(int value, int bitCount)
        {
            var bits = new List<bool>();
            for (int i = bitCount - 1; i >= 0; i--)
                bits.Add(((value >> i) & 1) == 1);
            return bits;
        }

        public static List<bool> ToBitStream(List<byte> data, List<byte> ecc)
        {
            var allCodewords = new List<byte>();
            allCodewords.AddRange(data);
            allCodewords.AddRange(ecc);

            var bitStream = new List<bool>();
            foreach (byte b in allCodewords)
            {
                for (int i = 7; i >= 0; i--)
                {
                    bitStream.Add(((b >> i) & 1) == 1);
                }
            }

            return bitStream;
        }
    }
}
