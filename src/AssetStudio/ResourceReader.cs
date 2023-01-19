using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using ArknightsResources.Utility;

namespace AssetStudio
{
    public class ResourceReader
    {
        private bool needSearch;
        private string path;
        private SerializedFile assetsFile;
        private long offset;
        private long size;
        private BinaryReader reader;

        public int Size { get => (int)size; }

        public ResourceReader(string path, SerializedFile assetsFile, long offset, long size)
        {
            needSearch = true;
            this.path = path;
            this.assetsFile = assetsFile;
            this.offset = offset;
            this.size = size;
        }

        public ResourceReader(BinaryReader reader, long offset, long size)
        {
            this.reader = reader;
            this.offset = offset;
            this.size = size;
        }

        private BinaryReader GetReader()
        {
            if (needSearch)
            {
                var resourceFileName = Path.GetFileName(path);
                if (assetsFile.assetsManager.resourceFileReaders.TryGetValue(resourceFileName, out reader))
                {
                    needSearch = false;
                    return reader;
                }
                var assetsFileDirectory = Path.GetDirectoryName(assetsFile.fullName);
                var resourceFilePath = Path.Combine(assetsFileDirectory, resourceFileName);
                if (!File.Exists(resourceFilePath))
                {
                    var findFiles = Directory.GetFiles(assetsFileDirectory, resourceFileName, SearchOption.AllDirectories);
                    if (findFiles.Length > 0)
                    {
                        resourceFilePath = findFiles[0];
                    }
                }
                if (File.Exists(resourceFilePath))
                {
                    needSearch = false;
                    reader = new BinaryReader(File.OpenRead(resourceFilePath));
                    assetsFile.assetsManager.resourceFileReaders.Add(resourceFileName, reader);
                    return reader;
                }
                throw new FileNotFoundException($"Can't find the resource file {resourceFileName}");
            }
            else
            {
                return reader;
            }
        }

        public byte[] GetData()
        {
            var binaryReader = GetReader();
            binaryReader.BaseStream.Position = offset;
            return binaryReader.ReadBytes((int)size);
        }

        public void GetData(byte[] buff)
        {
            var binaryReader = GetReader();
            binaryReader.BaseStream.Position = offset;
            binaryReader.Read(buff, 0, (int)size);
        }

        //Added
        internal unsafe void GetData(Span<byte> buff)
        {
            BinaryReader binaryReader = GetReader();
            binaryReader.BaseStream.Position = offset;
            byte* bufPtr = (byte*)InternalNativeMemory.Alloc(buff.Length);
            try
            {
                int numRead = 0;
                for (int i = 0; i < buff.Length; i++)
                {
                    int byteVal = binaryReader.ReadByte();
                    if (byteVal == -1)
                    {
                        break;
                    }

                    byte value = (byte)byteVal;
                    bufPtr[i] = value;
                    numRead++;
                }

                new ReadOnlySpan<byte>(bufPtr, numRead).CopyTo(buff);
            }
            finally
            {
                InternalNativeMemory.Free(bufPtr);
            }
        }

        public void WriteData(string path)
        {
            var binaryReader = GetReader();
            binaryReader.BaseStream.Position = offset;
            using (var writer = File.OpenWrite(path))
            {
                binaryReader.BaseStream.CopyTo(writer, size);
            }
        }
    }
}
