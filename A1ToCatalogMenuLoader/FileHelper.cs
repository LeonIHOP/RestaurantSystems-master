using A1ToCatalogMenuLoader.Containers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace A1ToCatalogMenuLoader
{
	public static class FileHelper
	{
		private static XmlSerializer mySerializer = new XmlSerializer(typeof(ElstarDataSet));
		public static Dictionary<string, ElstarDataSet> GetMenuFilesMetaData(string path)
		{
			Dictionary<string, ElstarDataSet> menuDictionary = new Dictionary<string, ElstarDataSet>();


			List<FileInfo> fileList = GetFileLilst(path);

			Dictionary<string, MenuFileContainer> sortedFileList = SortFiles(fileList);

			foreach (var item in sortedFileList)
			{
				ElstarDataSet ds = Deserialize(item.Value.FileInstance);
				menuDictionary.Add(item.Key, ds);
			}

			return menuDictionary;
		}

		public static Dictionary<string, MenuFileContainer> GetMenuFiles(string path)
		{
			Dictionary<string, ElstarDataSet> menuDictionary = new Dictionary<string, ElstarDataSet>();


			List<FileInfo> fileList = GetFileLilst(path);

			Dictionary<string, MenuFileContainer> sortedFileList = SortFiles(fileList);

			return sortedFileList;
		}

		public static Dictionary<string, ElstarDataSet> DeserializeFileBatch(Dictionary<string, MenuFileContainer> sortedFileList)
		{
			Dictionary<string, ElstarDataSet> menuDictionary = new Dictionary<string, ElstarDataSet>();

			
			foreach (var item in sortedFileList)
			{
				ElstarDataSet ds = Deserialize(item.Value.FileInstance);
				menuDictionary.Add(item.Key, ds);
			}

			return menuDictionary;
		}

		/// <summary>
		/// Creates a dictionary of files to be processed.  if multiple submissions for the same restaurant are in place, the most recent one will be used
		/// </summary>
		/// <param name="fileList"></param>
		/// <returns></returns>
		private static Dictionary<string, MenuFileContainer> SortFiles(List<FileInfo> fileList)
		{
			Dictionary<string, MenuFileContainer> sortingContainer = new Dictionary<string, MenuFileContainer>();
			List<string> individualRestaurantsToLoad = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["individualRestaurantsToLoad"]) ? null : ConfigurationManager.AppSettings["individualRestaurantsToLoad"].Split(new char[] { ',' }).ToList();

			if (fileList.Count > 0)
			{
				fileList.ForEach(m =>
				{
					List<string> temp = Path.GetFileNameWithoutExtension(m.FullName).Split(new char[] { '_' }).ToList();
					string restaurantNum = temp.First();

					string date1 = temp.Last();

					//string t = date1.Insert(4, "/").Insert(7, "/")
					DateTime date = System.Convert.ToDateTime(temp.Last().RemoveFromEnd(".xml").Insert(4, "/").Insert(7, "/"));
					//DateTime date = System.Convert.ToDateTime(temp.Last());

					//if file is new, just add it
					if (!sortingContainer.Keys.Contains(restaurantNum))
					{
						sortingContainer.Add(restaurantNum, new MenuFileContainer
						{
							FileDate = date,
							FileInstance = m,
							RestNumber = restaurantNum
						});
					}
					else
					{
						//the key is there, check the date.  Replace if current file is more recent
						if (sortingContainer[restaurantNum].FileDate < date)
						{
							sortingContainer[restaurantNum].FileInstance = m;
							sortingContainer[restaurantNum].FileDate = date;
						}
					}
				}
				);
			}

			if (individualRestaurantsToLoad != null && individualRestaurantsToLoad.Count > 0)
			{
				//fileInfoList = fileInfoList.Where(m => individualRestaurantsToLoad.Contains(m.)).ToDictionary(m => m.Key, m => m.Value);
				sortingContainer = sortingContainer.Where(m => individualRestaurantsToLoad.Contains(m.Key)).ToDictionary(m => m.Key, m => m.Value);
			}

			return sortingContainer;
		}

		public static List<FileInfo> GetFileLilst(string path)
		{

			DirectoryInfo di = new DirectoryInfo(path);

			List<FileInfo> fileInfoList = di.GetFiles("*.gz", SearchOption.AllDirectories).ToList();


			fileInfoList = fileInfoList.Where(m => 
			m.Length > 0.0m
			&& Path.GetFileNameWithoutExtension(m.FullName).ToLower().EndsWith("xml")
			&& Path.GetFileNameWithoutExtension(m.FullName).Contains("_")
			&& Path.GetFileNameWithoutExtension(m.FullName).Split(new char[] { '_' }).Count() == 2
			&& Path.GetFileNameWithoutExtension(m.FullName).Split(new char[] { '_' }).First().Length == 4
			&& Path.GetFileNameWithoutExtension(m.FullName).Split(new char[] { '_' }).First().All(char.IsDigit)
			).ToList();

			return fileInfoList;
		}

		public static ElstarDataSet Deserialize(FileInfo fileToDecompress)
		{
			ElstarDataSet ds = null;

			try
			{
				using (FileStream originalFileStream = fileToDecompress.OpenRead())
				{
					//string currentFileName = fileToDecompress.FullName;
					if (fileToDecompress.Extension.ToLower() == ".gz")
					{
						//using (FileStream decompressedFileStream = File.Create(newFileName))
						using (MemoryStream decompressedStream = new MemoryStream())
						{

							//using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
							//{
							//	byte[] buffer = new byte[1024];
							//	int nRead;
							//	while ((nRead = decompressionStream.Read(buffer, 0, buffer.Length)) > 0)
							//	{
							//		decompressedStream.Write(buffer, 0, nRead);
							//	}
							//	decompressionStream.Close();

							//	decompressedStream.Seek(0, SeekOrigin.Begin);

							//	XmlSerializer mySerializer = new XmlSerializer(typeof(ElstarDataSet));
							//	ds = (ElstarDataSet)mySerializer.Deserialize(decompressedStream);

							//}

							using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress, false))
							{
								//decompressionStream.
								decompressionStream.CopyTo(decompressedStream);
								decompressionStream.Close();
								//Console.WriteLine("Decompressed: {0}", fileToDecompress.Name);
								decompressedStream.Seek(0, SeekOrigin.Begin);

								//XmlSerializer mySerializer = new XmlSerializer(typeof(ElstarDataSet));
								ds = (ElstarDataSet)mySerializer.Deserialize(decompressedStream);
							}
						}

					}
					else
					{
						//XmlSerializer mySerializer = new XmlSerializer(typeof(ElstarDataSet));
						ds = (ElstarDataSet)mySerializer.Deserialize(originalFileStream);
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Failed to deserialize.  Reason: {0}, StackTrace: {1}", e.Message, e.StackTrace);
			}

			return ds;

		}

		public static void Decompress(FileInfo fileToDecompress)
		{
			using (FileStream originalFileStream = fileToDecompress.OpenRead())
			{
				string currentFileName = fileToDecompress.FullName;
				string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

				using (FileStream decompressedFileStream = File.Create(newFileName))
				{
					using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
					{
						decompressionStream.CopyTo(decompressedFileStream);
						Console.WriteLine("Decompressed: {0}", fileToDecompress.Name);
					}
				}
			}
		}
	}
}
