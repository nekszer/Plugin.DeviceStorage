using Android.App;
using Android.Content.PM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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
				var path = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath);
				var storagefolder = new StorageFolder();
				string[] element = path.Split('/');
				string foldername = element[element.Length - 1];
				if (Directory.Exists(path))
				{
					storagefolder.FullPath = path;
					storagefolder.Name = foldername;
				}
				else
				{
					throw new DirectoryNotFoundException("No existe el directorio " + foldername);
				}
				return storagefolder;
			}
		}

		public IStorageFolder SDCard
		{
			get
			{
				string path = Path.Combine("/sdcard/");
				StorageFolder storagefolder = new StorageFolder();
				string[] element = path.Split('/');
				string foldername = element[element.Length - 1];
				if (Directory.Exists(path))
				{
					storagefolder.FullPath = path;
					storagefolder.Name = foldername;
				}
				else
				{
					throw new DirectoryNotFoundException("No existe el directorio " + foldername);
				}
				return storagefolder;
			}
		}

		public IStorageFolder Root
		{
			get
			{
				string path = Path.Combine(Android.OS.Environment.RootDirectory.AbsolutePath);
				var storagefolder = new StorageFolder();
				string[] element = path.Split('/');
				string foldername = element[element.Length - 1];
				if (Directory.Exists(path))
				{
					storagefolder.FullPath = path;
					storagefolder.Name = foldername;
				}
				else
				{
					throw new DirectoryNotFoundException("No existe el directorio " + foldername);
				}
				return storagefolder;
			}
		}

		internal static IPermissionRequest PermissionSolitier { get; set; }
		public void SetPermissionRequest(IPermissionRequest permissionsolitier)
		{
			PermissionSolitier = permissionsolitier;
		}

		private IStorageFolder GetFolder(Folder folder = Folder.Downloads)
		{

			var home = Home;
			string foldername;
			switch (folder)
			{
				case Folder.Downloads:
					foldername = "Downloads";
					break;

				case Folder.Movie:
					foldername = "Movies";
					break;

				case Folder.Music:
					foldername = "Music";
					break;

				case Folder.Pictures:
					foldername = "DCIM";
					break;

				case Folder.CameraRoll:
					foldername = "DCIM/Camera";
					break;

				default:
					return Home;
			}

			return new StorageFolder
			{
				FullPath = Path.Combine(home.FullPath, foldername),
				Name = foldername
			};
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

		public async Task<IEnumerable<IStorageFile>> GetFiles()
		{
			List<IStorageFile> files = new List<IStorageFile>();
			if (Directory.Exists(FullPath))
			{
				var permissionok = await DeviceStorageImplementation.PermissionSolitier?.Request();
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

		public async Task Delete()
		{
			await HackAwait();
			if (Directory.Exists(FullPath))
			{
				Directory.Delete(FullPath);
			}
			else
			{
				throw new DirectoryNotFoundException("El directorio actual no existe, posiblemente haya sido borrado");
			}
		}

		public async Task<IStorageFolder> GetFolder(string name)
		{
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
			await HackAwait();
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

		private static async Task HackAwait()
		{
			await Task.Run(async () =>
			{
				await Task.Delay(1);
			});
		}
	}

	public class StorageFile : IStorageFile
	{

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

		public StorageFile()
		{

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
			await HackAwait();
			return null;
		}

		public async Task Delete()
		{
			if (await Exists())
			{
				File.Delete(FullPath);
				await HackAwait();
			}
		}

		public static async Task<StorageFile> GetFileFromPath(string path)
		{
			string[] element = path.Split('/');
			string filenameext = element[element.Length - 1];
			string[] elementfile = filenameext.Split('.');
			string filename = elementfile[0];
			string ext = elementfile[1];
			var filedatetime = File.GetCreationTime(path);
			var fileAttr = File.GetAttributes(path);
			await HackAwait();
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
			if (await Exists())
			{
				await HackAwait();
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
			await HackAwait();
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

		private static async Task HackAwait()
		{
			await Task.Run(async () =>
			{
				await Task.Delay(1);
			});
		}
	}
}