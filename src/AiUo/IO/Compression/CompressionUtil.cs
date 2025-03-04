using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace AiUo.IO;

/// <summary>
/// 压缩辅助类
/// </summary>
public static class CompressionUtil
{
    #region GZip

    #region Compress

    /// <summary>
    /// Gzip压缩源Stream到目标Stream
    /// </summary>
    /// <param name="source"></param>
    /// <param name="zipStream"></param>
    /// <returns></returns>
    public static long GZip(Stream source, Stream zipStream)
    {
        byte[] buffer = new byte[1024];
        int count = 0;
        //source.Position = 0;
        using (var compressedStream = new GZipStream(zipStream, CompressionMode.Compress, true))
        {
            do
            {
                count = source.Read(buffer, 0, buffer.Length);
                compressedStream.Write(buffer, 0, count);
            }
            while (count > 0);
            //source.Close();
            compressedStream.Close();
        }
        return zipStream.Length;
    }

    /// <summary>
    /// Gzip压缩到文件
    /// </summary>
    /// <param name="source"></param>
    /// <param name="zipFile"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static long GZip(Stream source, string zipFile, FileMode mode = FileMode.Create)
    {
        using (var fsZip = new FileStream(zipFile, mode))
            return GZip(source, fsZip);
    }

    /// <summary>
    /// Gzip压缩到文件
    /// </summary>
    /// <param name="sourceFile"></param>
    /// <param name="zipFile"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static long GZip(string sourceFile, string zipFile, FileMode mode = FileMode.Create)
    {
        using (var fsSource = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
        {
            using (var fsZip = new FileStream(zipFile, mode))
            {
                return GZip(fsSource, fsZip);
            }
        }
    }

    /// <summary>
    /// Gzip压缩得到bytes
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static byte[] GZipToBytes(Stream source)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            GZip(source, ms);
            return ms.ToArray();
        }
    }
        
    /// <summary>
    /// Gzip压缩得到MemoryStream
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static MemoryStream GZipToStream(Stream source)
    {
        var ret = new MemoryStream();
        GZip(source, ret);
        return ret;
    }

    #endregion // Compress

    #region Decompress
    /// <summary>
    /// Gzip解压缩Stream
    /// </summary>
    /// <param name="zipStream"></param>
    /// <param name="unzipStream"></param>
    /// <returns></returns>
    public static long UnGZip(Stream zipStream, Stream unzipStream)
    {
        long ret = 0;
        //byte[] buf = new byte[1024 * 1024];
        //zipStream.Position = 0;
        using (var decompressionStream = new GZipStream(zipStream, CompressionMode.Decompress, true))
        {
            decompressionStream.CopyTo(unzipStream);
            //int count = 0;
            //do
            //{
            //    count = decompressionStream.Read(buf, 0, buf.Length);
            //    unzipStream.Write(buf, 0, count);
            //    //unzipStream.Flush();
            //}
            //while (count > 0);

            ret = unzipStream.Length;
            zipStream.Close();
        }
        return ret;
    }

    /// <summary>
    /// Gzip解压缩Stream
    /// </summary>
    /// <param name="zipStream"></param>
    /// <param name="unzipFile"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static long UnGZip(Stream zipStream, string unzipFile, FileMode mode = FileMode.Create)
    {
        using (var fsUnzip = new FileStream(unzipFile, mode))
            return UnGZip(zipStream, fsUnzip);
    }

    /// <summary>
    /// Gzip解压缩文件
    /// </summary>
    /// <param name="zipFile"></param>
    /// <param name="unzipFile"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static long UnGZip(string zipFile, string unzipFile, FileMode mode = FileMode.Create)
    {
        using (var fsZip = new FileStream(zipFile, FileMode.Open, FileAccess.Read))
        {
            using (var fsUnzip = new FileStream(unzipFile, mode))
                return UnGZip(fsZip, fsUnzip);
        }
    }

    /// <summary>
    /// GZip解压缩到bytes
    /// </summary>
    /// <param name="zipStream"></param>
    /// <returns></returns>
    public static byte[] UnGZipToBytes(Stream zipStream)
    {
        using (var unzipStream = UnGZipToStream(zipStream))
        {
            return unzipStream.ToArray();
        }
    }

    /// <summary>
    /// GZip解压缩到Stream
    /// </summary>
    /// <param name="zipStream"></param>
    /// <returns></returns>
    public static MemoryStream UnGZipToStream(Stream zipStream)
    {
        var ret = new MemoryStream();
        UnGZip(zipStream, ret);
        return ret;
    }
    #endregion // Decompress

    #endregion // Gzip

    #region Zip
    /// <summary>
    /// 压缩文本到数据流
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static Stream ZipText(string text)
    {
        var ret = new MemoryStream();
        using (var zipArchive = new ZipArchive(ret, ZipArchiveMode.Create, true))
        {
            var demoFile = zipArchive.CreateEntry("zipped.txt");
            using (var entryStream = demoFile.Open())
            {
                using (var writer = new StreamWriter(entryStream))
                {
                    writer.Write(text);
                }
            }
        }
        return ret;
    }
    /// <summary>
    /// 解压文本
    /// </summary>
    /// <param name="zippedStream"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static string UnZipText(Stream zippedStream, Encoding encoding = null)
    {
        string ret = null;
        var entries = UnZipStream(zippedStream);
        if (entries != null && entries.Count == 1)
        {
            ret = (encoding ?? Encoding.UTF8).GetString(entries[0].data);
        }
        return ret;
    }
    /// <summary>
    /// 解压Stream
    /// </summary>
    /// <param name="zippedStream"></param>
    /// <returns></returns>
    public static List<(string name, byte[] data)> UnZipStream(Stream zippedStream)
    {
        var ret = new List<(string FullName, byte[] Data)>();
        using (var archive = new ZipArchive(zippedStream))
        {
            foreach (var entry in archive.Entries)
            {
                using (var unzippedEntryStream = entry.Open())
                {
                    using (var ms = new MemoryStream())
                    {
                        unzippedEntryStream.CopyTo(ms);
                        ret.Add((entry.FullName, ms.ToArray()));
                    }
                }
            }
        }
        return ret;
    }

    /// <summary>
    /// ZIP压缩目录
    /// </summary>
    /// <param name="sourceDirectory"></param>
    /// <param name="zipFile"></param>
    public static void Zip(string sourceDirectory, string zipFile)
        => ZipFile.CreateFromDirectory(sourceDirectory, zipFile);
        
    /// <summary>
    /// ZIP解压缩到目录
    /// </summary>
    /// <param name="zipFile"></param>
    /// <param name="extractDirectory"></param>
    public static void UnZip(string zipFile, string extractDirectory)
        => ZipFile.ExtractToDirectory(zipFile, extractDirectory);

    /// <summary>
    /// 读取ZIP文件中的压缩项到Stream
    /// </summary>
    /// <param name="zipFile"></param>
    /// <param name="entryName"></param>
    /// <returns></returns>
    public static Stream ReadZipEntryToStream(string zipFile, string entryName)
    {
        using (var archive = ZipFile.OpenRead(zipFile))
        {
            return archive.GetEntry(entryName).Open();
        }
    }

    /// <summary>
    /// 读取ZIP文件中的压缩项到bytes
    /// </summary>
    /// <param name="zipFile"></param>
    /// <param name="entryName"></param>
    /// <returns></returns>
    public static byte[] ReadZipEntryToBytes(string zipFile, string entryName)
        => IOUtil.ReadStreamToBytes(ReadZipEntryToStream(zipFile, entryName), true);
    #endregion

    #region 7zip

    #region Compress
    /// <summary>
    /// 7zip压缩
    /// </summary>
    /// <param name="source"></param>
    /// <param name="zipStream"></param>
    /// <returns></returns>
    public static long SevenZip(Stream source, Stream zipStream)
    {
        var encoder = new SevenZip.Compression.LZMA.Encoder();
        //encoder.SetCoderProperties(_propIDs, _properties);
        encoder.WriteCoderProperties(zipStream);
        zipStream.Write(BitConverter.GetBytes(source.Length), 0, 8);

        encoder.Code(source, zipStream, source.Length, -1, null);
        zipStream.Flush();
        return zipStream.Length;
    }

    /// <summary>
    /// 7zip压缩
    /// </summary>
    /// <param name="sourceFile"></param>
    /// <param name="zipFile"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static long SevenZip(string sourceFile, string zipFile, FileMode mode = FileMode.Create)
    {
        using (var fsSource = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
        {
            using (var fsZip = new FileStream(zipFile, mode))
            {
                return SevenZip(fsSource, fsZip);
            }
        }
    }

    /// <summary>
    /// 7zip压缩
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static byte[] SevenZipToBytes(Stream source)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            SevenZip(source, ms);
            return ms.ToArray();
        }
    }

    /// <summary>
    /// 7zip压缩
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static MemoryStream SevenZipToStream(Stream source)
    {
        var ret = new MemoryStream();
        SevenZip(source, ret);
        return ret;
    }
    #endregion

    #region Decompress
    /// <summary>
    /// 7zip解压缩
    /// </summary>
    /// <param name="zipStream"></param>
    /// <param name="unzipStream"></param>
    public static long UnSevenZip(Stream zipStream, Stream unzipStream)
    {
        var decoder = new SevenZip.Compression.LZMA.Decoder();
        // properties  
        byte[] properties = new byte[5];
        zipStream.Read(properties, 0, 5);
        // 文件大小
        byte[] fileLengthBytes = new byte[8];
        zipStream.Read(fileLengthBytes, 0, 8);
        long fileLength = BitConverter.ToInt64(fileLengthBytes, 0);
        decoder.SetDecoderProperties(properties);
        decoder.Code(zipStream, unzipStream, zipStream.Length, fileLength, null);
        unzipStream.Flush();
        return unzipStream.Length;
    }
        
    /// <summary>
    /// 7zip解压缩
    /// </summary>
    /// <param name="zipFile"></param>
    /// <param name="unzipFile"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static long UnSevenZip(string zipFile, string unzipFile, FileMode mode = FileMode.Create)
    {
        using (var fsZip = new FileStream(zipFile, FileMode.Open, FileAccess.Read))
        {
            using (var fsUnzip = new FileStream(unzipFile, mode))
                return UnSevenZip(fsZip, fsUnzip);
        }
    }
    /// <summary>
    /// 7zip解压缩
    /// </summary>
    /// <param name="zipStream"></param>
    /// <returns></returns>
    public static byte[] UnSevenZipToBytes(Stream zipStream)
    {
        using (var unzipStream = UnSevenZipToStream(zipStream))
        {
            return unzipStream.ToArray();
        }
    }
    /// <summary>
    /// 7zip解压缩
    /// </summary>
    /// <param name="zipStream"></param>
    /// <returns></returns>
    public static MemoryStream UnSevenZipToStream(Stream zipStream)
    {
        var ret = new MemoryStream();
        UnSevenZip(zipStream, ret);
        return ret;
    }
    #endregion
    #endregion
}