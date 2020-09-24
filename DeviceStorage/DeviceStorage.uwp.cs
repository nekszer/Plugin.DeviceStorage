using System.Collections.Generic;
using System.Threading.Tasks;
using UWPFolder = Windows.Storage.KnownFolders;
using UWPFolders = Windows.Storage.StorageFolder;
using UWPFile = Windows.Storage.StorageFile;
using System;
using System.IO;
using System.Linq;
using Windows.Storage;

namespace Plugin.DeviceStorage
{
    /// <summary>
    /// Interface for DeviceStorage
    /// </summary>
    public class DeviceStorageImplementation : IDeviceStorage
    {
        public IStorageFolder Documents
        {
            get
            {
                return GetFolder(Folder.Documents);
            }
        }

        public IStorageFolder Downloads
        {
            get
            {
                return GetFolder(Folder.Downloads);
            }
        }

        public IStorageFolder Music
        {
            get
            {
                return GetFolder(Folder.Music);
            }
        }

        public IStorageFolder Pictures
        {
            get
            {
                return GetFolder(Folder.Pictures);
            }
        }

        public IStorageFolder CameraRoll
        {
            get
            {
                return GetFolder(Folder.CameraRoll);
            }
        }

        public IStorageFolder Movies
        {
            get
            {
                return GetFolder(Folder.Movie);
            }
        }

        public IStorageFolder Home
        {
            get
            {
                return GetFolder(Folder.Documents);
            }
        }

        public IStorageFolder SDCard
        {
            get
            {
                return null;
            }
        }

        public IStorageFolder Root
        {
            get
            {
                return GetFolder(Folder.Documents);
            }
        }

        private IStorageFolder GetFolder(Folder folder = Folder.Downloads)
        {
            UWPFolders myfolder;
            switch (folder)
            {

                case Folder.Downloads:
                    string localfolder = ApplicationData.Current.LocalFolder.Path;
                    var array = localfolder.Split('\\');
                    var username = array[2];
                    string downloads = @"C:\Users\" + username + @"\Downloads";
                    return new StorageFolder
                    {
                        FullPath = downloads,
                        Name = "Downloads"
                    };

                case Folder.Movie:
                    myfolder = UWPFolder.VideosLibrary;
                    break;

                case Folder.Music:
                    myfolder = UWPFolder.MusicLibrary;
                    break;

                case Folder.Pictures:
                    myfolder = UWPFolder.PicturesLibrary;
                    break;

                case Folder.CameraRoll:
                    myfolder = UWPFolder.CameraRoll;
                    break;

                default:
                    myfolder = UWPFolder.DocumentsLibrary;
                    break;
            }

            return new StorageFolder
            {
                FullPath = myfolder.Path,
                Name = myfolder.DisplayName
            };
        }

        public void SetPermissionRequest(IPermissionRequest permissionsolitier)
        {
            
        }
    }

    public class StorageFolder : IStorageFolder
    {

        public string FullPath
        {
            get;
            internal set;
        }

        public string Name
        {
            get;
            internal set;
        }

        public async Task<IEnumerable<IStorageFile>> GetFiles()
        {
            List<IStorageFile> files = new List<IStorageFile>();
            var winfolder = await UWPFolders.GetFolderFromPathAsync(FullPath);
            var winfiles = await winfolder.GetFilesAsync();
            foreach (var file in winfiles)
            {
                files.Add(new StorageFile
                {
                    Attributes = FileAttributes.Normal,
                    DisplayName = file.DisplayName,
                    DisplayType = file.DisplayType,
                    FullPath = file.Path,
                    Name = file.Name,
                    DateCreated = file.DateCreated.DateTime
                });
            }
            return files;
        }

        public async Task<IStorageFile> GetFile(string filenamewithextension)
        {

            var winfolder = await UWPFolders.GetFolderFromPathAsync(FullPath);
            var file = await winfolder.GetFileAsync(filenamewithextension);
            return await StorageFile.GetFileFromPath(file.Path);
        }

        public async Task<IStorageFile> CreateFileAsync(string filename)
        {
            return await CreateFile(filename, CreationCollisionOption.FailIfExists);
        }

        public async Task<IStorageFile> CreateFile(string filename, CreationCollisionOption option)
        {
            var folder = await GetUWPFolder(FullPath);
            UWPFile file = null;
            switch (option)
            {
                case CreationCollisionOption.GenerateUniqueName:
                    file = await folder.CreateFileAsync(filename, Windows.Storage.CreationCollisionOption.GenerateUniqueName);
                    break;
                case CreationCollisionOption.ReplaceExisting:
                    file = await folder.CreateFileAsync(filename, Windows.Storage.CreationCollisionOption.ReplaceExisting);
                    break;
                case CreationCollisionOption.FailIfExists:
                    file = await folder.CreateFileAsync(filename, Windows.Storage.CreationCollisionOption.FailIfExists);
                    break;
                case CreationCollisionOption.OpenIfExists:
                    file = await folder.CreateFileAsync(filename, Windows.Storage.CreationCollisionOption.OpenIfExists);
                    break;
            }
            return new StorageFile
            {
                Attributes = (FileAttributes)((int)file.Attributes),
                DateCreated = file.DateCreated.DateTime,
                DisplayName = file.DisplayName,
                DisplayType = file.DisplayType,
                FullPath = file.Path,
                Name = file.Name
            };
        }

        public async Task<IStorageFolder> CreateFolder(string foldername)
        {
            return await CreateFolder(foldername, CreationCollisionOption.FailIfExists);
        }

        public async Task<IStorageFolder> CreateFolder(string foldername, CreationCollisionOption option)
        {
            var folder = await GetUWPFolder(FullPath);
            UWPFolders newfolder = null;
            switch (option)
            {
                case CreationCollisionOption.GenerateUniqueName:
                    newfolder = await folder.CreateFolderAsync(foldername, Windows.Storage.CreationCollisionOption.GenerateUniqueName);
                    break;
                case CreationCollisionOption.ReplaceExisting:
                    newfolder = await folder.CreateFolderAsync(foldername, Windows.Storage.CreationCollisionOption.ReplaceExisting);
                    break;
                case CreationCollisionOption.FailIfExists:
                    newfolder = await folder.CreateFolderAsync(foldername, Windows.Storage.CreationCollisionOption.FailIfExists);
                    break;
                case CreationCollisionOption.OpenIfExists:
                    newfolder = await folder.CreateFolderAsync(foldername, Windows.Storage.CreationCollisionOption.OpenIfExists);
                    break;
            }

            return new StorageFolder
            {
                FullPath = newfolder.Path,
                Name = newfolder.Name
            };
        }

        public async Task Delete()
        {
            var folder = await GetUWPFolder(FullPath);
            await folder.DeleteAsync();
        }

        public async Task<IStorageFolder> GetFolder(string name)
        {
            var folder = await GetUWPFolder(FullPath);
            var uwpfolder = await folder.GetFolderAsync(name);
            return new StorageFolder
            {
                FullPath = uwpfolder.Path,
                Name = uwpfolder.Name
            };
        }

        public async Task<IEnumerable<IStorageFolder>> GetFolders()
        {
            List<IStorageFolder> folders = new List<IStorageFolder>();
            var folder = await GetUWPFolder(FullPath);
            var uwpfolders = await folder.GetFoldersAsync();
            foreach (var uwpfolder in uwpfolders)
            {
                folders.Add(new StorageFolder
                {
                    FullPath = uwpfolder.Path,
                    Name = uwpfolder.Name
                });
            }
            return folders;
        }

        private string UniqueString()
        {
            Guid g = Guid.NewGuid();
            string GuidString = Convert.ToBase64String(g.ToByteArray());
            GuidString = GuidString.Replace("=", "");
            GuidString = GuidString.Replace("+", "");
            return GuidString;
        }

        public static async Task<IStorageFolder> GetFolderFromPath(string dirpath)
        {
            var folder = await GetUWPFolder(dirpath);
            return new StorageFolder
            {
                FullPath = folder.Path,
                Name = folder.Name
            };
        }

        private static async Task<UWPFolders> GetUWPFolder(string path)
        {
            var folder = await UWPFolders.GetFolderFromPathAsync(path);
            return folder;
        }
    }

    public class StorageFile : IStorageFile
    {

        public string FullPath
        {
            get;
            internal set;
        }

        public string Name
        {
            get;
            internal set;
        }

        public string DisplayName
        {
            get;
            internal set;
        }

        public string DisplayType
        {
            get;
            internal set;
        }

        public DateTime DateCreated
        {
            get;
            internal set;
        }

        public FileAttributes Attributes
        {
            get;
            internal set;
        }

        public async Task<IStorageFile> CopyTo(IStorageFolder folderdestination)
        {
            return await CopyTo(folderdestination, Name);
        }

        public async Task<IStorageFile> CopyTo(IStorageFolder folderdestination, string filenamewithextension)
        {
            return await CopyTo(folderdestination, filenamewithextension, NameCollisionOption.FailIfExists);
        }

        public async Task<IStorageFile> CopyTo(IStorageFolder folderdestination, string filenamewithextension, NameCollisionOption option)
        {
            if (await Exists())
            {
                var file = await UWPFile.GetFileFromPathAsync(FullPath);
                var folder = await UWPFolders.GetFolderFromPathAsync(folderdestination.FullPath);

                switch (option)
                {
                    case NameCollisionOption.GenerateUniqueName:
                        await file.CopyAsync(folder, filenamewithextension, Windows.Storage.NameCollisionOption.GenerateUniqueName);
                        break;
                    case NameCollisionOption.ReplaceExisting:
                        await file.CopyAsync(folder, filenamewithextension, Windows.Storage.NameCollisionOption.ReplaceExisting);
                        break;
                    case NameCollisionOption.FailIfExists:
                        await file.CopyAsync(folder, filenamewithextension, Windows.Storage.NameCollisionOption.FailIfExists);
                        break;
                }
            }
            return null;
        }

        public async Task Delete()
        {
            if (await Exists())
            {
                var file = await UWPFile.GetFileFromPathAsync(FullPath);
                await file.DeleteAsync();
            }
        }

        public static async Task<StorageFile> GetFileFromPath(string path)
        {
            var file = await UWPFile.GetFileFromPathAsync(path);
            return new StorageFile
            {
                Attributes = (FileAttributes)((int)file.Attributes),
                DateCreated = file.DateCreated.DateTime,
                DisplayName = file.DisplayName,
                DisplayType = file.DisplayType,
                FullPath = file.Path,
                Name = file.Name
            };
        }

        public async Task<Stream> Open(FileAccessMode mode)
        {
            if (await Exists())
            {
                var file = await UWPFile.GetFileFromPathAsync(FullPath);
                switch (mode)
                {
                    case FileAccessMode.Read:
                        return await file.OpenStreamForReadAsync();

                    case FileAccessMode.Write:
                        return await file.OpenStreamForWriteAsync();
                }
                return null;
            }
            else
            {
                throw new FileNotFoundException("El archivo que tratas de copiar no existe, path: " + FullPath + ", name: " + Name);
            }
        }

        private async Task<bool> Exists()
        {
            var file = await UWPFile.GetFileFromPathAsync(FullPath);
            return file != null;
        }

        private string UniqueString()
        {
            Guid g = Guid.NewGuid();
            string GuidString = Convert.ToBase64String(g.ToByteArray());
            GuidString = GuidString.Replace("=", "");
            GuidString = GuidString.Replace("+", "");
            return GuidString;
        }
    }
}