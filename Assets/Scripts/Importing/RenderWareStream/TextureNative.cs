using UGameCore.Utilities;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    [SectionType(21)]
    public class TextureNative : SectionData
    {
        public readonly UInt32 PlatformID;
        public readonly Filter FilterFlags;
        public readonly WrapMode WrapV;
        public readonly WrapMode WrapU;
        public readonly string DiffuseName;
        public readonly string AlphaName;
        public readonly RasterFormat Format;
        public readonly bool Alpha;
        public readonly CompressionMode Compression;
        public readonly UInt16 Width;
        public readonly UInt16 Height;
        public readonly byte BPP;
        public readonly byte MipMapCount;
        public readonly byte RasterType;
        public readonly Int32 ImageDataSize;

        public readonly byte[] ImageData;
        public readonly byte[] ImageLevelData;

        public TextureNative(SectionHeader header, Stream stream)
            : base(header, stream)
        {
            SectionHeader.Read(stream);
            var reader = new BinaryReader(stream);

            PlatformID = reader.ReadUInt32();
            FilterFlags = (Filter)reader.ReadUInt16();
            WrapV = (WrapMode)reader.ReadByte();
            WrapU = (WrapMode)reader.ReadByte();
            DiffuseName = reader.ReadString(32);
            AlphaName = reader.ReadString(32);
            Format = (RasterFormat)reader.ReadUInt32();

            if (PlatformID == 9)
            {
                var dxt = reader.ReadString(4);
                switch (dxt)
                {
                    case "DXT1":
                        Compression = CompressionMode.DXT1;
                        break;

                    case "DXT3":
                        Compression = CompressionMode.DXT3;
                        break;

                    default:
                        Compression = CompressionMode.None;
                        break;
                }
            }
            else
            {
                Alpha = reader.ReadUInt32() == 0x1;
            }

            Width = reader.ReadUInt16();
            Height = reader.ReadUInt16();
            BPP = (byte)(reader.ReadByte() >> 3);
            MipMapCount = reader.ReadByte();
            RasterType = reader.ReadByte();

            if (RasterType != 0x4)
            {
                throw new Exception("Unexpected RasterType, expected 0x04.");
            }

            if (PlatformID == 9)
            {
                Alpha = (reader.ReadByte() & 0x1) == 0x1;
            }
            else
            {
                Compression = (CompressionMode)reader.ReadByte();
            }

            ImageDataSize = reader.ReadInt32();

            ImageData = reader.ReadBytes(ImageDataSize);

            if ((Format & RasterFormat.ExtMipMap) != 0)
            {
                var tot = ImageDataSize;
                for (var i = 0; i < MipMapCount; ++i)
                {
                    tot += ImageDataSize >> (2 * i);
                }

                ImageLevelData = reader.ReadBytes(tot);
            }
            else
            {
                ImageLevelData = ImageData;
            }
        }

        //public void Write(SectionHeader header, Stream stream)
        //{
        //    SectionHeader.Read(stream);
        //    var writer = new BinaryWriter(stream);

        //    writer.Write(PlatformID);
        //    writer.Write((int)FilterFlags);
        //    writer.Write((byte)WrapV);
        //    writer.Write((byte)WrapU);
        //    writer.Write(DiffuseName);
        //    writer.Write(AlphaName);
        //    writer.Write((UInt32)Format);

        //    if (PlatformID == 9)
        //    {
        //        switch (Compression)
        //        {
        //            case CompressionMode.None:
        //                writer.Write("none");
        //                break;
        //            case CompressionMode.DXT1:
        //                writer.Write("DXT1");
        //                break;
        //            case CompressionMode.DXT3:
        //                writer.Write("DXT3");
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    else
        //    {
        //        writer.Write(0x1);
        //    }

        //    writer.Write(Width);
        //    writer.Write(Height);
        //    writer.Write((byte)BPP);
        //    writer.Write((byte)MipMapCount);
        //    writer.Write((byte)RasterType);

        //    if (PlatformID == 9)
        //    {
        //        writer.Write((byte)0x1);
        //    }
        //    else
        //    {
        //        writer.Write((byte)Compression);
        //    }
        //    writer.Write(ImageDataSize);
        //    writer.Write(ImageData);

        //    if ((Format & RasterFormat.ExtMipMap) != 0)
        //    {
        //        var tot = ImageDataSize;
        //        for (var i = 0; i < MipMapCount; ++i)
        //        {
        //            tot += ImageDataSize >> (2 * i);
        //        }
        //        writer.Write(ImageLevelData);
        //    }
        //    else
        //    {
        //        ImageLevelData = ImageData;
        //    }
        //}
    }
}