using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Plugin.DeviceStorage
{
    public interface IDeviceStorage
    {
        IStorageFolder CameraRoll { get; }
        IStorageFolder Documents { get; }
        IStorageFolder Downloads { get; }
        IStorageFolder Home { get; }
        IStorageFolder Movies { get; }
        IStorageFolder Music { get; }
        IStorageFolder Pictures { get; }
        IStorageFolder Root { get; }
        IStorageFolder SDCard { get; }

        void SetPermissionRequest(IPermissionRequest permissionsolitier);
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
        Task Delete();
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
        Task Delete();
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
