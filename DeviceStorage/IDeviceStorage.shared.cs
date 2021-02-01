using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Plugin.DeviceStorage
{
    public interface IDeviceStorage
    {
        Task<IStorageFolder> CameraRoll { get; }
        Task<IStorageFolder> Documents { get; }
        Task<IStorageFolder> Downloads { get; }
        Task<IStorageFolder> Home { get; }
        Task<IStorageFolder> Movies { get; }
        Task<IStorageFolder> Music { get; }
        Task<IStorageFolder> Pictures { get; }
        Task<IStorageFolder> Root { get; }
        Task<IStorageFolder> SDCard { get; }
    }

    public interface IPermissionRequest
    {
        Task<bool> Request();
    }

    public interface IStorageFile
    {
        DateTime DateCreated { get; }
        string DisplayName { get; }
        string DisplayType { get; }
        string FullPath { get; }
        string Name { get; }
        Task<IStorageFile> CopyTo(IStorageFolder folderdestination);
        Task<IStorageFile> CopyTo(IStorageFolder folderdestination, string filenamewithextension);
        Task<IStorageFile> CopyTo(IStorageFolder folderdestination, string filenamewithextension, NameCollisionOption option);
        Task<bool> Delete();
        Task<Stream> Open(FileAccessMode mode);
    }

    public interface IStorageFolder
    {
        string FullPath { get; }
        string Name { get; }

        Task<IStorageFile> CreateFile(string filename, CreationCollisionOption option);
        Task<IStorageFile> CreateFileAsync(string filename);
        Task<IStorageFolder> CreateFolder(string foldername);
        Task<IStorageFolder> CreateFolder(string foldername, CreationCollisionOption option);
        Task<bool> Delete();
        Task<IStorageFile> GetFile(string filenamewithextension);
        Task<IEnumerable<IStorageFile>> GetFiles();
        Task<IStorageFolder> GetFolder(string name);
        Task<IEnumerable<IStorageFolder>> GetFolders();

    }

    public enum CreationCollisionOption
    {
        GenerateUniqueName = 0,
        ReplaceExisting = 1,
        FailIfExists = 2,
        OpenIfExists = 3
    }

    public enum FileAccessMode
    {
        Read, Write
    }

    public enum NameCollisionOption
    {
        GenerateUniqueName = 0,
        ReplaceExisting = 1,
        FailIfExists = 2
    }

    public enum Folder
    {
        Downloads, Music, Pictures, Documents, Movie, CameraRoll
    }
}
