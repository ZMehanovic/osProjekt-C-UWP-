using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace OsProjekt
{
    class FilesHelper
    {
        private static StorageFolder dataFolder;


        public static async Task<IStorageFile> GetFile( String name)
        {
            IStorageFile newFile = null;
            if (dataFolder != null)
            {
                if (await dataFolder.TryGetItemAsync(name) != null)
                {
                    newFile = await dataFolder.GetFileAsync(name);
                }
                else
                {
                    newFile = await dataFolder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);

                }
            }
            return newFile;
        }
        public static async Task<byte[]> GetByteArrayAsync(String fileName)
        {
            IStorageFile file = await GetFile(fileName);
            IBuffer buffer = await FileIO.ReadBufferAsync(file);
            byte[] result = buffer.ToArray();

            return result;
        }
        public static StorageFolder DataFolder { get => dataFolder; set => dataFolder = value; }

    }
}
