﻿using Android.App;
using Android.Content.PM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Plugin.DeviceStorage
{

	/// <summary>
	/// Permission Request
	/// </summary>
	public class PermissionRequest : IPermissionRequest
	{
		public async Task<bool> Request()
		{
			var read = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
			var write = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
			if (read != PermissionStatus.Granted)
				read = await Permissions.RequestAsync<Permissions.StorageRead>();
			if (write != PermissionStatus.Granted)
				write = await Permissions.RequestAsync<Permissions.StorageWrite>();
			return read == PermissionStatus.Granted && write == PermissionStatus.Granted;
		}
	}

	/// <summary>
	/// Interface for DeviceStorage
	/// </summary>
	public class DeviceStorageImplementation : IDeviceStorage
	{

		public Task<IStorageFolder> Documents
		{
			get
			{
				return GetFolder(Folder.Documents);
			}
		}

		public Task<IStorageFolder> Downloads
		{
			get
			{
				return GetFolder(Folder.Downloads);
			}
		}

		public Task<IStorageFolder> Music
		{
			get
			{
				return GetFolder(Folder.Music);
			}
		}

		public Task<IStorageFolder> Pictures
		{
			get
			{
				return GetFolder(Folder.Pictures);
			}
		}

		public Task<IStorageFolder> CameraRoll
		{
			get
			{
				return GetFolder(Folder.CameraRoll);
			}
		}

		public Task<IStorageFolder> Movies
		{
			get
			{
				return GetFolder(Folder.Movie);
			}
		}

		public Task<IStorageFolder> Home
		{
			get
			{
				var path = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath);
				return StorageFolder.GetFolderFromPath(path);
			}
		}

		public Task<IStorageFolder> SDCard
		{
			get
			{
				string path = Path.Combine("/sdcard/");
				return StorageFolder.GetFolderFromPath(path);
			}
		}

		public Task<IStorageFolder> Root
		{
			get
			{
				string path = Path.Combine(Android.OS.Environment.RootDirectory.AbsolutePath);
				return StorageFolder.GetFolderFromPath(path);
			}
		}

		private Task<IStorageFolder> GetFolder(Folder folder = Folder.Downloads)
		{
			switch (folder)
			{
				case Folder.Downloads:
					return StorageFolder.GetFolderFromPath("Downloads");

				case Folder.Movie:
					return StorageFolder.GetFolderFromPath("Movies");

				case Folder.Music:
					return StorageFolder.GetFolderFromPath("Music");

				case Folder.Pictures:
					return StorageFolder.GetFolderFromPath("DCIM");

				case Folder.CameraRoll:
					return StorageFolder.GetFolderFromPath("DCIM/Camera");

				default:
					return Home;
			}
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
			set;
		}

		private StorageFolder()
        {

        }

		public async Task<IEnumerable<IStorageFile>> GetFiles()
		{
			var status = await new PermissionRequest().Request();
			if (!status)
				return new List<IStorageFile>(0);
			List<IStorageFile> files = new List<IStorageFile>();
			if (Directory.Exists(FullPath))
			{
				var permissionok = await new PermissionRequest().Request();
				if (!permissionok) throw new ApplicationException("No tienes permisos para acceder a los archivos");
				var enumeratefiles = Directory.EnumerateFiles(FullPath);
				foreach (var file in enumeratefiles)
				{
					files.Add(await StorageFile.GetFileFromPath(file));
				}
			}
			else
			{
				throw new DirectoryNotFoundException("No es posible obtener los archivos, al parecer, el directorio ya no existe");
			}
			return files;
		}

		public async Task<IStorageFile> GetFile(string filenamewithextension)
		{
			var status = await new PermissionRequest().Request();
			if (!status)
				return null;
			if (Directory.Exists(FullPath))
			{
				var filepath = Path.Combine(FullPath, filenamewithextension);
				return await StorageFile.GetFileFromPath(filepath);
			}
			else
			{
				throw new DirectoryNotFoundException("No es posible obtener los archivos, al parecer, el directorio ya no existe");
			}
		}

		public async Task<IStorageFile> CreateFileAsync(string filename)
		{
			return await CreateFile(filename, CreationCollisionOption.FailIfExists);
		}

		public async Task<IStorageFile> CreateFile(string filename, CreationCollisionOption option)
		{
			var status = await new PermissionRequest().Request();
			if (!status)
				return null;
			var filepath = Path.Combine(FullPath, filename);
			if (option == CreationCollisionOption.OpenIfExists)
			{
				if (File.Exists(filepath))
				{
					return await StorageFile.GetFileFromPath(filepath);
				}
			}
			else
			{
				if (option == CreationCollisionOption.FailIfExists)
				{
					if (File.Exists(filepath))
					{
						throw new Exception("El archivo ya existe");
					}
				}
				else
				{
					if (option == CreationCollisionOption.ReplaceExisting)
					{
						File.Delete(filepath);
					}
					else
					{
						filepath = Path.Combine(FullPath, UniqueString() + filename);
					}
				}
			}

			if (Directory.Exists(FullPath))
			{
				using (var fileStream = File.Create(filepath))
				{
					fileStream.Dispose();
					return await StorageFile.GetFileFromPath(filepath);
				}
			}
			else
			{
				throw new DirectoryNotFoundException("El directorio actual no existe, posiblemente haya sido borrado");
			}
		}

		public async Task<IStorageFolder> CreateFolder(string foldername)
		{
			return await CreateFolder(foldername, CreationCollisionOption.FailIfExists);
		}

		public async Task<IStorageFolder> CreateFolder(string foldername, CreationCollisionOption option)
		{
			var status = await new PermissionRequest().Request();
			if (!status)
				return null;
			var dirpath = Path.Combine(FullPath, foldername);
			if (option == CreationCollisionOption.FailIfExists)
			{
				if (Directory.Exists(dirpath))
				{
					throw new Exception("El directorio que intentas crear ya existe");
				}
			}
			else
			{
				if (option == CreationCollisionOption.OpenIfExists)
				{
					await GetFolderFromPath(dirpath);
				}
				else
				{
					if (option == CreationCollisionOption.ReplaceExisting)
					{
						Directory.Delete(dirpath);
					}
					else
					{
						dirpath = Path.Combine(FullPath, UniqueString() + foldername);
					}
				}
			}

			if (Directory.Exists(FullPath))
			{
				var directoryInfo = Directory.CreateDirectory(dirpath);
				if (directoryInfo.Exists)
				{
					return await GetFolderFromPath(dirpath);
				}
				else
				{
					throw new DirectoryNotFoundException("Imposible crear el directorio");
				}
			}
			else
			{
				throw new DirectoryNotFoundException("El directorio actual no existe, posiblemente haya sido borrado");
			}
		}

		public async Task<bool> Delete()
		{
			var status = await new PermissionRequest().Request();
			if (!status)
				return false;
			if (Directory.Exists(FullPath))
			{
				Directory.Delete(FullPath);
				return true;
			}
			throw new DirectoryNotFoundException("El directorio actual no existe, posiblemente haya sido borrado");
		}

		public async Task<IStorageFolder> GetFolder(string name)
		{
			var status = await new PermissionRequest().Request();
			if (!status)
				return null;
			if (Directory.Exists(FullPath))
			{
				var dir = Path.Combine(FullPath, name);
				if (Directory.Exists(dir))
				{
					return await GetFolderFromPath(dir);
				}
				else
				{
					throw new DirectoryNotFoundException("El directorio a buscar no existe");
				}
			}
			else
			{
				throw new DirectoryNotFoundException("El directorio actual no existe, posiblemente haya sido borrado");
			}
		}

		public async Task<IEnumerable<IStorageFolder>> GetFolders()
		{
			var status = await new PermissionRequest().Request();
			if (!status)
				return new List<IStorageFolder>(0);
			List<IStorageFolder> folders = new List<IStorageFolder>();
			if (Directory.Exists(FullPath))
			{
				var directorios = Directory.EnumerateDirectories(FullPath);
				foreach (var directorio in directorios)
				{
					folders.Add(await GetFolderFromPath(directorio));
				}
			}
			else
			{
				throw new DirectoryNotFoundException("El directorio actual no existe, posiblemente haya sido borrado");
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
			var status = await new PermissionRequest().Request();
			if (!status)
				return null;
			var storagefolder = new StorageFolder();
			string[] element = dirpath.Split('/');
			string foldername = element[element.Length - 1];
			if (Directory.Exists(dirpath))
			{
				storagefolder.FullPath = dirpath;
				storagefolder.Name = foldername;
			}
			else
			{
				throw new DirectoryNotFoundException("No existe el directorio " + foldername);
			}
			return storagefolder;
		}
	}

	public class StorageFile : IStorageFile
	{

		private StorageFile()
        {

        }

		public string FullPath
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public string DisplayName
		{
			get;
			set;
		}

		public string DisplayType
		{
			get;
			set;
		}

		public DateTime DateCreated
		{
			get;
			set;
		}

		public FileAttributes Attributes
		{
			get;
			set;
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
			var status = await new PermissionRequest().Request();
			if (!status) 
				return null;
			if (await Exists())
			{
				string outfilepath = Path.Combine(folderdestination.FullPath, filenamewithextension);
				if (option == NameCollisionOption.FailIfExists)
				{
					if (File.Exists(outfilepath))
					{
						throw new Exception("El archivo que tratas de copiar ya existe");
					}
				}
				else
				{
					if (option == NameCollisionOption.GenerateUniqueName)
					{
						outfilepath = Path.Combine(folderdestination.FullPath, UniqueString() + filenamewithextension);
					}
					else
					{
						File.Delete(outfilepath);
					}
				}
				File.Copy(FullPath, outfilepath);
				if (File.Exists(outfilepath))
				{
					return await GetFileFromPath(outfilepath);
				}
				else
				{
					throw new FileNotFoundException("No fue posible copiar el archivo");
				}
			}
			return null;
		}

		public async Task<bool> Delete()
		{
			var status = await new PermissionRequest().Request();
			if (!status)
				return false;
			if (!await Exists())
				return false;
			File.Delete(FullPath);
			return true;
		}

		public static async Task<StorageFile> GetFileFromPath(string path)
		{
			var status = await new PermissionRequest().Request();
			if (!status)
				return null;
			string[] element = path.Split('/');
			string filenameext = element[element.Length - 1];
			string[] elementfile = filenameext.Split('.');
			string filename = elementfile[0];
			string ext = elementfile[1];
			var filedatetime = File.GetCreationTime(path);
			var fileAttr = File.GetAttributes(path);
			return new StorageFile
			{
				Name = filenameext,
				DisplayName = filename,
				DisplayType = ext,
				DateCreated = filedatetime,
				Attributes = fileAttr,
				FullPath = path
			};
		}

		public async Task<Stream> Open(FileAccessMode mode)
		{
			var status = await new PermissionRequest().Request();
			if (!status)
				return null;
			if (await Exists())
			{
				if (mode == FileAccessMode.Read)
				{
					return File.OpenRead(FullPath);
				}
				else
				{
					return File.OpenWrite(FullPath);
				}
			}
			else
			{
				throw new FileNotFoundException("El archivo que tratas de copiar no existe, path: " + FullPath + ", name: " + Name);
			}
		}

		private async Task<bool> Exists()
		{
			var status = await new PermissionRequest().Request();
			if (!status)
				return false;
			if (File.Exists(FullPath))
			{
				return true;
			}
			else
			{
				throw new FileNotFoundException("El archivo que tratas de copiar no existe, path: " + FullPath + ", name: " + Name);
			}
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