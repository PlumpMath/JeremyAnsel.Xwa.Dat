
namespace JeremyAnsel.Xwa.Dat
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;

    public class DatImage
    {
        public DatImage()
        {
        }

        public DatImage(short groupId, short imageId)
        {
            this.GroupId = groupId;
            this.ImageId = imageId;
        }

        public DatImageFormats Format { get; internal set; }

        public short Width { get; internal set; }

        public short Height { get; internal set; }

        public short ColorsCount { get; internal set; }

        public short GroupId { get; set; }

        public short ImageId { get; set; }

        public int OffsetX { get; set; }

        public int OffsetY { get; set; }

        internal byte[] rawData;

        public byte[] GetRawData()
        {
            return this.rawData;
        }

        public byte[] GetImageData()
        {
            switch (this.Format)
            {
                case DatImageFormats.Format25:
                    return this.ReadFormat25();

                case DatImageFormats.Format24:
                    return this.ReadFormat24();

                case DatImageFormats.Format7:
                    return this.ReadFormat7();

                case DatImageFormats.Format23:
                    return this.ReadFormat23();
            }

            return null;
        }

        private byte[] ReadFormat25()
        {
            return this.rawData;
        }

        private byte[] ReadFormat24()
        {
            int colorsOffset = 0;
            int dataOffset = this.ColorsCount * 3;

            int length = this.Width * this.Height;

            byte[] data = new byte[length * 4];

            for (int i = 0; i < length; i++)
            {
                byte pal = this.rawData[dataOffset + i * 2 + 0];
                byte alpha = this.rawData[dataOffset + i * 2 + 1];

                byte r = this.rawData[colorsOffset + pal * 3 + 0];
                byte g = this.rawData[colorsOffset + pal * 3 + 1];
                byte b = this.rawData[colorsOffset + pal * 3 + 2];

                data[i * 4 + 0] = b;
                data[i * 4 + 1] = g;
                data[i * 4 + 2] = r;
                data[i * 4 + 3] = alpha;
            }

            return data;
        }

        private byte[] ReadFormat7()
        {
            int colorsOffset = 0;
            int dataOffset = this.ColorsCount * 3;

            byte[] data = new byte[this.Width * this.Height * 4];

            int index = 0;

            for (int y = 0; y < this.Height; y++)
            {
                byte opCount = this.rawData[dataOffset + index];
                index++;

                int x = 0;

                for (int opIndex = 0; opIndex < opCount; opIndex++)
                {
                    byte op = this.rawData[dataOffset + index];
                    index++;

                    uint opW = op & 0x7FU;

                    if ((op & 0x80U) != 0)
                    {
                        byte alpha = 0;
                        byte r = this.rawData[colorsOffset + 0 * 3 + 0];
                        byte g = this.rawData[colorsOffset + 0 * 3 + 1];
                        byte b = this.rawData[colorsOffset + 0 * 3 + 2];

                        for (int i = 0; i < opW; i++)
                        {
                            data[y * this.Width * 4 + x * 4 + 0] = b;
                            data[y * this.Width * 4 + x * 4 + 1] = g;
                            data[y * this.Width * 4 + x * 4 + 2] = r;
                            data[y * this.Width * 4 + x * 4 + 3] = alpha;
                            x++;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < opW; i++)
                        {
                            byte pal = this.rawData[dataOffset + index];
                            index++;

                            byte alpha = 0xFF;

                            byte r = this.rawData[colorsOffset + pal * 3 + 0];
                            byte g = this.rawData[colorsOffset + pal * 3 + 1];
                            byte b = this.rawData[colorsOffset + pal * 3 + 2];

                            data[y * this.Width * 4 + x * 4 + 0] = b;
                            data[y * this.Width * 4 + x * 4 + 1] = g;
                            data[y * this.Width * 4 + x * 4 + 2] = r;
                            data[y * this.Width * 4 + x * 4 + 3] = alpha;
                            x++;
                        }
                    }
                }
            }

            return data;
        }

        private byte[] ReadFormat23()
        {
            int colorsOffset = 0;
            int dataOffset = this.ColorsCount * 3;

            byte[] data = new byte[this.Width * this.Height * 4];

            int index = 0;

            for (int y = 0; y < this.Height; y++)
            {
                byte opCount = this.rawData[dataOffset + index];
                index++;

                int x = 0;

                for (int opIndex = 0; opIndex < opCount; opIndex++)
                {
                    byte op = this.rawData[dataOffset + index];
                    index++;

                    uint opW = op & 0x3FU;

                    if ((op & 0xC0U) == 0xC0U)
                    {
                        byte alpha = 0;
                        byte r = this.rawData[colorsOffset + 0 * 3 + 0];
                        byte g = this.rawData[colorsOffset + 0 * 3 + 1];
                        byte b = this.rawData[colorsOffset + 0 * 3 + 2];

                        for (int i = 0; i < opW; i++)
                        {
                            data[y * this.Width * 4 + x * 4 + 0] = b;
                            data[y * this.Width * 4 + x * 4 + 1] = g;
                            data[y * this.Width * 4 + x * 4 + 2] = r;
                            data[y * this.Width * 4 + x * 4 + 3] = alpha;
                            x++;
                        }
                    }
                    else if ((op & 0xC0U) == 0)
                    {
                        for (int i = 0; i < opW; i++)
                        {
                            byte pal = this.rawData[dataOffset + index];
                            index++;

                            byte alpha = 0xFF;

                            byte r = this.rawData[colorsOffset + pal * 3 + 0];
                            byte g = this.rawData[colorsOffset + pal * 3 + 1];
                            byte b = this.rawData[colorsOffset + pal * 3 + 2];

                            data[y * this.Width * 4 + x * 4 + 0] = b;
                            data[y * this.Width * 4 + x * 4 + 1] = g;
                            data[y * this.Width * 4 + x * 4 + 2] = r;
                            data[y * this.Width * 4 + x * 4 + 3] = alpha;

                            x++;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < opW; i++)
                        {
                            byte alpha = this.rawData[dataOffset + index];
                            index++;

                            byte pal = this.rawData[dataOffset + index];
                            index++;

                            byte r = this.rawData[colorsOffset + pal * 3 + 0];
                            byte g = this.rawData[colorsOffset + pal * 3 + 1];
                            byte b = this.rawData[colorsOffset + pal * 3 + 2];

                            data[y * this.Width * 4 + x * 4 + 0] = b;
                            data[y * this.Width * 4 + x * 4 + 1] = g;
                            data[y * this.Width * 4 + x * 4 + 2] = r;
                            data[y * this.Width * 4 + x * 4 + 3] = alpha;

                            x++;
                        }
                    }

                }
            }

            return data;
        }

        public void Save(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToUpperInvariant();

            ImageFormat format;

            switch (ext)
            {
                case ".BMP":
                    format = ImageFormat.Bmp;
                    break;

                case ".PNG":
                    format = ImageFormat.Png;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("fileName");
            }

            var data = this.GetImageData();

            if (data == null)
            {
                return;
            }

            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);

            try
            {
                using (var bitmap = new Bitmap(this.Width, this.Height, this.Width * 4, PixelFormat.Format32bppArgb, handle.AddrOfPinnedObject()))
                {
                    bitmap.Save(fileName, format);
                }
            }
            finally
            {
                handle.Free();
            }
        }

        public void ReplaceWithFile(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToUpperInvariant();

            switch (ext)
            {
                case ".BMP":
                case ".PNG":
                case ".JPG":
                case ".GIF":
                    if (!File.Exists(fileName))
                    {
                        throw new FileNotFoundException(null, fileName);
                    }

                    using (var file = new Bitmap(fileName))
                    {
                        if (file.Width > short.MaxValue)
                        {
                            throw new ArgumentOutOfRangeException("fileName");
                        }

                        if (file.Height > short.MaxValue)
                        {
                            throw new ArgumentOutOfRangeException("fileName");
                        }

                        var rect = new Rectangle(0, 0, file.Width, file.Height);
                        int length = file.Width * file.Height;

                        byte[] bytes = new byte[length * 4];

                        using (var bitmap = file.Clone(rect, PixelFormat.Format32bppArgb))
                        {
                            var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

                            try
                            {
                                Marshal.Copy(data.Scan0, bytes, 0, length * 4);
                            }
                            finally
                            {
                                bitmap.UnlockBits(data);
                            }
                        }

                        this.Format = DatImageFormats.Format25;
                        this.Width = (short)file.Width;
                        this.Height = (short)file.Height;
                        this.ColorsCount = 0;
                        this.rawData = bytes;
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException("fileName");
            }
        }

        public void ReplaceWithMemory(DatImageFormats format, short width, short height, short colorsCount, byte[] data)
        {
            this.Format = format;
            this.Width = width;
            this.Height = height;
            this.ColorsCount = colorsCount;
            this.rawData = data;
        }

        public static DatImage FromFile(short groupId, short imageId, string fileName)
        {
            DatImage image = new DatImage(groupId, imageId);

            image.ReplaceWithFile(fileName);

            return image;
        }

        public static DatImage FromMemory(short groupId, short imageId, DatImageFormats format, short width, short height, short colorsCount, byte[] rawData)
        {
            DatImage image = new DatImage(groupId, imageId);

            image.ReplaceWithMemory(format, width, height, colorsCount, rawData);

            return image;
        }

        public void ConvertToFormat(DatImageFormats format)
        {
            switch (format)
            {
                case DatImageFormats.Format25:
                    this.ConvertToFormat25();
                    break;

                case DatImageFormats.Format24:
                    this.ConvertToFormat24();
                    break;

                case DatImageFormats.Format7:
                    this.ConvertToFormat7();
                    break;

                case DatImageFormats.Format23:
                    this.ConvertToFormat23();
                    break;

                default:
                    throw new ArgumentOutOfRangeException("format");
            }
        }

        public void ConvertToFormat25()
        {
            if (this.Format == DatImageFormats.Format25)
            {
                return;
            }

            var data = this.GetImageData();

            if (data == null)
            {
                return;
            }

            this.Format = DatImageFormats.Format25;
            this.ColorsCount = 0;
            this.rawData = data;
        }

        public void ConvertToFormat24()
        {
            if (this.Format == DatImageFormats.Format24)
            {
                return;
            }

            var data = this.GetImageData();

            if (data == null)
            {
                return;
            }

            int length = data.Length / 4;

            byte[] palette;
            byte[] colors;

            var dataColors = Enumerable.Range(0, length)
                .Select(t => Color.FromArgb(data[t * 4 + 2], data[t * 4 + 1], data[t * 4 + 0]))
                .Distinct()
                .ToArray();

            if (dataColors.Length <= 256)
            {
                palette = new byte[dataColors.Length * 3];

                for (int i = 0; i < dataColors.Length; i++)
                {
                    palette[i * 3 + 0] = dataColors[i].R;
                    palette[i * 3 + 1] = dataColors[i].G;
                    palette[i * 3 + 2] = dataColors[i].B;
                }

                colors = Enumerable.Range(0, length)
                    .Select(t => Color.FromArgb(data[t * 4 + 2], data[t * 4 + 1], data[t * 4 + 0]))
                    .Select(t =>
                    {
                        for (int i = 0; i < dataColors.Length; i++)
                        {
                            if (dataColors[i] == t)
                            {
                                return (byte)i;
                            }
                        }

                        return (byte)0;
                    })
                    .ToArray();
            }
            else
            {
                var image = new ColorQuant.WuColorQuantizer().Quantize(data);
                int paletteColorsCount = image.Palette.Length / 4;

                palette = new byte[paletteColorsCount * 3];

                for (int i = 0; i < paletteColorsCount; i++)
                {
                    palette[i * 3 + 0] = image.Palette[i * 4 + 2];
                    palette[i * 3 + 1] = image.Palette[i * 4 + 1];
                    palette[i * 3 + 2] = image.Palette[i * 4 + 0];
                }

                colors = image.Bytes;
            }

            byte[] imageData = new byte[palette.Length + length * 2];

            palette.CopyTo(imageData, 0);

            for (int i = 0; i < length; i++)
            {
                imageData[palette.Length + i * 2 + 0] = colors[i];
                imageData[palette.Length + i * 2 + 1] = data[i * 4 + 3];
            }

            this.Format = DatImageFormats.Format24;
            this.ColorsCount = (short)(palette.Length / 3);
            this.rawData = imageData;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Reviewed")]
        public void ConvertToFormat7()
        {
            if (this.Format == DatImageFormats.Format7)
            {
                return;
            }

            this.ConvertToFormat24();

            if (this.Format != DatImageFormats.Format24)
            {
                return;
            }

            int alphaValue = 0x80;

            Action<List<Tuple<bool, ArraySegment<byte>>>, byte[], bool, int, int> addSegment = (values, array, t, c, n) =>
            {
                while (n > 0)
                {
                    if (n <= 127)
                    {
                        values.Add(new Tuple<bool, ArraySegment<byte>>(
                            t,
                            new ArraySegment<byte>(array, c, n * 2)));
                        c += n * 2;
                        n = 0;
                    }
                    else
                    {
                        values.Add(new Tuple<bool, ArraySegment<byte>>(
                            t,
                            new ArraySegment<byte>(array, c, 127 * 2)));
                        c += 127 * 2;
                        n -= 127;
                    }
                }
            };

            Func<ArraySegment<byte>, List<Tuple<bool, ArraySegment<byte>>>> parseLine = t =>
            {
                int tLength = t.Offset + t.Count;

                var values = new List<Tuple<bool, ArraySegment<byte>>>();

                for (int i = t.Offset; i < tLength; )
                {
                    int c;
                    int n;

                    c = i;
                    n = 0;

                    for (; i < tLength; i += 2)
                    {
                        if (t.Array[i + 1] >= alphaValue)
                        {
                            break;
                        }

                        n++;
                    }

                    addSegment(values, t.Array, false, c, n);

                    c = i;
                    n = 0;

                    for (; i < tLength; i += 2)
                    {
                        if (t.Array[i + 1] < alphaValue)
                        {
                            break;
                        }

                        n++;
                    }

                    addSegment(values, t.Array, true, c, n);
                }

                return values;
            };

            var lines = Enumerable.Range(0, this.Height)
                .Select(t => new ArraySegment<byte>(this.rawData, this.ColorsCount * 3 + t * this.Width * 2, this.Width * 2))
                .Select(t => parseLine(t))
                .ToArray();

            if (lines.Any(t => t.Count > 256))
            {
                return;
            }

            Func<List<Tuple<bool, ArraySegment<byte>>>, List<byte>> writeLine = t =>
            {
                List<byte> data = new List<byte>();

                data.Add((byte)t.Count);

                foreach (var block in t)
                {
                    if (block.Item1 == false)
                    {
                        data.Add((byte)(block.Item2.Count / 2 | 0x80));
                    }
                    else
                    {
                        data.Add((byte)(block.Item2.Count / 2));

                        for (int i = block.Item2.Offset; i < block.Item2.Offset + block.Item2.Count; i += 2)
                        {
                            data.Add(block.Item2.Array[i]);
                        }
                    }
                }

                return data;
            };

            var linesData = lines.SelectMany(t => writeLine(t))
                .ToArray();

            byte[] raw = new byte[this.ColorsCount * 3 + linesData.Length];

            Array.Copy(this.rawData, raw, this.ColorsCount * 3);
            Array.Copy(linesData, 0, raw, this.ColorsCount * 3, linesData.Length);

            this.Format = DatImageFormats.Format7;
            this.rawData = raw;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Reviewed")]
        public void ConvertToFormat23()
        {
            if (this.Format == DatImageFormats.Format23)
            {
                return;
            }

            this.ConvertToFormat24();

            if (this.Format != DatImageFormats.Format24)
            {
                return;
            }

            Action<List<Tuple<byte, ArraySegment<byte>>>, byte[], byte, int, int> addSegment = (values, array, t, c, n) =>
            {
                while (n > 0)
                {
                    if (n <= 63)
                    {
                        values.Add(new Tuple<byte, ArraySegment<byte>>(
                            t,
                            new ArraySegment<byte>(array, c, n * 2)));
                        c += n * 2;
                        n = 0;
                    }
                    else
                    {
                        values.Add(new Tuple<byte, ArraySegment<byte>>(
                            t,
                            new ArraySegment<byte>(array, c, 63 * 2)));
                        c += 63 * 2;
                        n -= 63;
                    }
                }
            };

            Func<ArraySegment<byte>, List<Tuple<byte, ArraySegment<byte>>>> parseLine = t =>
            {
                int tLength = t.Offset + t.Count;

                var values = new List<Tuple<byte, ArraySegment<byte>>>();

                for (int i = t.Offset; i < tLength; )
                {
                    int c;
                    int n;

                    c = i;
                    n = 0;

                    for (; i < tLength; i += 2)
                    {
                        if (t.Array[i + 1] != 0)
                        {
                            break;
                        }

                        n++;
                    }

                    addSegment(values, t.Array, 0, c, n);

                    c = i;
                    n = 0;

                    for (; i < tLength; i += 2)
                    {
                        if (t.Array[i + 1] != 0xFF)
                        {
                            break;
                        }

                        n++;
                    }

                    addSegment(values, t.Array, 1, c, n);

                    c = i;
                    n = 0;

                    for (; i < tLength; i += 2)
                    {
                        if (t.Array[i + 1] == 0 || t.Array[i + 1] == 0xFF)
                        {
                            break;
                        }

                        n++;
                    }

                    addSegment(values, t.Array, 2, c, n);
                }

                if (values.Sum(i => i.Item2.Count) != this.Width * 2)
                {
                    return values;
                }

                return values;
            };

            var lines = Enumerable.Range(0, this.Height)
                .Select(t => new ArraySegment<byte>(this.rawData, this.ColorsCount * 3 + t * this.Width * 2, this.Width * 2))
                .Select(t => parseLine(t))
                .ToArray();

            if (lines.Any(t => t.Count > 256))
            {
                return;
            }

            Func<List<Tuple<byte, ArraySegment<byte>>>, List<byte>> writeLine = t =>
            {
                List<byte> data = new List<byte>();

                data.Add((byte)t.Count);

                foreach (var block in t)
                {
                    if (block.Item1 == 0)
                    {
                        data.Add((byte)(block.Item2.Count / 2 | 0xC0));
                    }
                    else if (block.Item1 == 1)
                    {
                        data.Add((byte)(block.Item2.Count / 2));

                        for (int i = block.Item2.Offset; i < block.Item2.Offset + block.Item2.Count; i += 2)
                        {
                            data.Add(block.Item2.Array[i]);
                        }
                    }
                    else
                    {
                        data.Add((byte)(block.Item2.Count / 2 | 0x80));

                        for (int i = block.Item2.Offset; i < block.Item2.Offset + block.Item2.Count; i += 2)
                        {
                            data.Add(block.Item2.Array[i + 1]);
                            data.Add(block.Item2.Array[i]);
                        }
                    }
                }

                return data;
            };

            var linesData = lines.SelectMany(t => writeLine(t))
                .ToArray();

            byte[] raw = new byte[this.ColorsCount * 3 + linesData.Length];

            Array.Copy(this.rawData, raw, this.ColorsCount * 3);
            Array.Copy(linesData, 0, raw, this.ColorsCount * 3, linesData.Length);

            this.Format = DatImageFormats.Format23;
            this.rawData = raw;
        }

        public void MakeColorTransparent(byte red, byte green, byte blue)
        {
            DatImageFormats format = this.Format;

            var data = this.GetImageData();

            if (data == null)
            {
                return;
            }

            int length = data.Length / 4;

            for (int i = 0; i < length; i++)
            {
                byte r = data[i * 4 + 2];
                byte g = data[i * 4 + 1];
                byte b = data[i * 4 + 0];

                if (r == red && g == green && b == blue)
                {
                    data[i * 4 + 3] = 0;
                }
            }

            this.Format = DatImageFormats.Format25;
            this.ColorsCount = 0;
            this.rawData = data;

            this.ConvertToFormat(format);
        }

        public void MakeColorTransparent(byte red0, byte green0, byte blue0, byte red1, byte green1, byte blue1)
        {
            DatImageFormats format = this.Format;

            var data = this.GetImageData();

            if (data == null)
            {
                return;
            }

            int length = data.Length / 4;

            for (int i = 0; i < length; i++)
            {
                byte r = data[i * 4 + 2];
                byte g = data[i * 4 + 1];
                byte b = data[i * 4 + 0];

                if (r >= red0 && r <= red1 && g >= green0 && g <= green1 && b >= blue0 && b <= blue1)
                {
                    data[i * 4 + 3] = 0;
                }
            }

            this.Format = DatImageFormats.Format25;
            this.ColorsCount = 0;
            this.rawData = data;

            this.ConvertToFormat(format);
        }
    }
}
