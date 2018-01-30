using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Linq;
using A1ToCatalogMenuLoader.Containers;
using EntityFramework.Utilities;

namespace A1ToCatalogMenuLoader
{
	class Program
	{
		static void Main(string[] args)
		{
			DoProcessing2();

			System.Console.WriteLine("Press any key to terminate");
			System.Console.ReadLine();
		}

		private static void DoProcessing2()
		{
			
			string path = ConfigurationManager.AppSettings["directoryToProcess"];

			if (string.IsNullOrWhiteSpace(path))
			{
				path = Directory.GetCurrentDirectory();
			}
			
			//get all 'gz' files, unzip them, Deserialize them into ElstarDataSet, and load them into a dictionary for processing
			Dictionary<string, MenuFileContainer> fileDictionary = FileHelper.GetMenuFiles(path);
			System.Console.WriteLine("Total files to process: " + fileDictionary.Count);

			//there is a lot of files, have to paginate
			int start = 0;
			int pageSize = 100;
			int iterationCount = fileDictionary.Count / pageSize;

			System.Console.WriteLine(string.Format("PageSize={0}, iterationCount={1}", pageSize, iterationCount));

			
			for (int i = 0; i <= iterationCount; i++)
			{
				System.Console.WriteLine(string.Format("Processing batch{0}...", i+1));
				System.Console.WriteLine(string.Format("Batch start time: {0}", System.DateTime.Now));
				Dictionary<string, ElstarDataSet> menuDictionary = FileHelper.DeserializeFileBatch(fileDictionary.Skip(start).Take(pageSize).ToDictionary(m => m.Key, m => m.Value));

				//now do db updates				
				UpdateCatalog4(menuDictionary);

				menuDictionary.Clear();
				menuDictionary = null;
				System.GC.Collect();
				System.GC.WaitForPendingFinalizers();

				//reset starting position
				start += pageSize;
				System.Console.WriteLine(string.Format("Batch end time: {0}", System.DateTime.Now));
				System.Console.WriteLine(string.Format("Finished processing batch{0}...", i + 1));
			}
		}

		

		private static void UpdateCatalog4(Dictionary<string, ElstarDataSet> menuDictionary)
		{
			List<Button> buttonGlobalList = new List<Button>();
			List<Category> categoryGlobalList = new List<Category>();
			List<ChainLink> chainLinkGlobalList = new List<ChainLink>();
			List<GroupType> groupTypeGlobalList = new List<GroupType>();

			List<Merchandise> merchGlobalList = new List<Merchandise>();
			List<MerchandiseDept> merchDeptGlobalList = new List<MerchandiseDept>();
			List<Price> priceGlobalList = new List<Price>();
			List<ScreenGroup> screenGroupGlobalList = new List<ScreenGroup>();

			using (APBMenuCatalogEntities db = new APBMenuCatalogEntities())
			{
				db.Configuration.AutoDetectChangesEnabled = false;

				foreach (var item in menuDictionary)
				{
					if (item.Value != null && item.Value.Items != null && item.Value.Items.Count() > 0)
					{
						string restNumber = item.Key;

						System.Console.WriteLine(string.Format("Generating data for Restaurant number {0}", restNumber));

						db.PurgeLandRestaurantData(restNumber);

						var buttonList = item.Value.Items.Where(m => m is ElstarDataSetButton).Select(m => (m as ElstarDataSetButton).ConvertToAPBCatalogFormat(restNumber)).ToList();
						var categoryList = item.Value.Items.Where(m => m is ElstarDataSetCategory).Select(m => (m as ElstarDataSetCategory).ConvertToAPBCatalogFormat(restNumber)).ToList();
						var chainLinkList = item.Value.Items.Where(m => m is ElstarDataSetChainLink).Select(m => (m as ElstarDataSetChainLink).ConvertToAPBCatalogFormat(restNumber)).ToList();
						var groupTypeList = item.Value.Items.Where(m => m is ElstarDataSetGroupType).Select(m => (m as ElstarDataSetGroupType).ConvertToAPBCatalogFormat(restNumber)).ToList();

						var merchList = item.Value.Items.Where(m => m is ElstarDataSetMerchandise).Select(m => (m as ElstarDataSetMerchandise).ConvertToAPBCatalogFormat(restNumber)).ToList();
						var merchDeptList = item.Value.Items.Where(m => m is ElstarDataSetMerchandiseDept).Select(m => (m as ElstarDataSetMerchandiseDept).ConvertToAPBCatalogFormat(restNumber)).ToList();
						var priceList = item.Value.Items.Where(m => m is ElstarDataSetPrice).Select(m => (m as ElstarDataSetPrice).ConvertToAPBCatalogFormat(restNumber)).ToList();
						var screenGroupList = item.Value.Items.Where(m => m is ElstarDataSetScreenGroup).Select(m => (m as ElstarDataSetScreenGroup).ConvertToAPBCatalogFormat(restNumber)).ToList();

						buttonGlobalList.AddRange(buttonList);
						categoryGlobalList.AddRange(categoryList);
						chainLinkGlobalList.AddRange(chainLinkList);
						groupTypeGlobalList.AddRange(groupTypeList);

						merchGlobalList.AddRange(merchList);
						merchDeptGlobalList.AddRange(merchDeptList);
						priceGlobalList.AddRange(priceList);
						screenGroupGlobalList.AddRange(screenGroupList);
					}
				}

				

				//now save them ALL
				EFBatchOperation.For(db, db.Buttons).InsertAll(buttonGlobalList);
				System.Console.WriteLine(string.Format("Persisted {0} Button records", buttonGlobalList.Count));

				EFBatchOperation.For(db, db.Categories).InsertAll(categoryGlobalList);
				System.Console.WriteLine(string.Format("Persisted {0} Category records", categoryGlobalList.Count));

				EFBatchOperation.For(db, db.ChainLinks).InsertAll(chainLinkGlobalList);
				System.Console.WriteLine(string.Format("Persisted {0} ChainLinks records", chainLinkGlobalList.Count));

				EFBatchOperation.For(db, db.GroupTypes).InsertAll(groupTypeGlobalList);
				System.Console.WriteLine(string.Format("Persisted {0} GroupType records", groupTypeGlobalList.Count));

				EFBatchOperation.For(db, db.Merchandises).InsertAll(merchGlobalList);
				System.Console.WriteLine(string.Format("Persisted {0} Merchandise records", merchGlobalList.Count));

				EFBatchOperation.For(db, db.MerchandiseDepts).InsertAll(merchDeptGlobalList);
				System.Console.WriteLine(string.Format("Persisted {0} MerchandiseDept records", merchDeptGlobalList.Count));

				EFBatchOperation.For(db, db.Prices).InsertAll(priceGlobalList);
				System.Console.WriteLine(string.Format("Persisted {0} Price records", priceGlobalList.Count));

				EFBatchOperation.For(db, db.ScreenGroups).InsertAll(screenGroupGlobalList);
				System.Console.WriteLine(string.Format("Persisted {0} ScreenGroup records", screenGroupGlobalList.Count));
			}
		}


		private static void UpdateCatalog2(Dictionary<string, ElstarDataSet> menuDictionary)
		{
			using (APBMenuCatalogEntities db = new APBMenuCatalogEntities())
			{
				db.Configuration.AutoDetectChangesEnabled = false;

				foreach (var item in menuDictionary)
				{
					if (item.Value != null && item.Value.Items != null && item.Value.Items.Count() > 0)
					{
						string restNumber = item.Key;

						db.PurgeLandRestaurantData(restNumber);

						var buttonList = item.Value.Items.Where(m => m is ElstarDataSetButton).Select(m => (m as ElstarDataSetButton).ConvertToAPBCatalogFormat(restNumber)).ToList();
						var categoryList = item.Value.Items.Where(m => m is ElstarDataSetCategory).Select(m => (m as ElstarDataSetCategory).ConvertToAPBCatalogFormat(restNumber)).ToList();
						var chainLinkList = item.Value.Items.Where(m => m is ElstarDataSetChainLink).Select(m => (m as ElstarDataSetChainLink).ConvertToAPBCatalogFormat(restNumber)).ToList();
						var groupTypeList = item.Value.Items.Where(m => m is ElstarDataSetGroupType).Select(m => (m as ElstarDataSetGroupType).ConvertToAPBCatalogFormat(restNumber)).ToList();

						var merchList = item.Value.Items.Where(m => m is ElstarDataSetMerchandise).Select(m => (m as ElstarDataSetMerchandise).ConvertToAPBCatalogFormat(restNumber)).ToList();
						var merchDeptList = item.Value.Items.Where(m => m is ElstarDataSetMerchandiseDept).Select(m => (m as ElstarDataSetMerchandiseDept).ConvertToAPBCatalogFormat(restNumber)).ToList();
						var priceList = item.Value.Items.Where(m => m is ElstarDataSetPrice).Select(m => (m as ElstarDataSetPrice).ConvertToAPBCatalogFormat(restNumber)).ToList();
						var screenGroupList = item.Value.Items.Where(m => m is ElstarDataSetScreenGroup).Select(m => (m as ElstarDataSetScreenGroup).ConvertToAPBCatalogFormat(restNumber)).ToList();

						db.Buttons.AddRange(buttonList);
						db.Categories.AddRange(categoryList);
						db.ChainLinks.AddRange(chainLinkList);
						db.GroupTypes.AddRange(groupTypeList);
						db.Merchandises.AddRange(merchList);
						db.MerchandiseDepts.AddRange(merchDeptList);
						db.Prices.AddRange(priceList);
						db.ScreenGroups.AddRange(screenGroupList);

						db.SaveChanges();
					}
				}
			}
		}

		private static void UpdateCatalog3(Dictionary<string, ElstarDataSet> menuDictionary)
		{


			using (APBMenuCatalogEntities db = new APBMenuCatalogEntities())
			{
				db.Configuration.AutoDetectChangesEnabled = false;

				foreach (var item in menuDictionary)
				{
					if (item.Value != null && item.Value.Items != null && item.Value.Items.Count() > 0)
					{
						string restNumber = item.Key;

						db.PurgeLandRestaurantData(restNumber);

						var buttonList = item.Value.Items.Where(m => m is ElstarDataSetButton).Select(m => (m as ElstarDataSetButton).ConvertToAPBCatalogFormat(restNumber)).ToList();
						var categoryList = item.Value.Items.Where(m => m is ElstarDataSetCategory).Select(m => (m as ElstarDataSetCategory).ConvertToAPBCatalogFormat(restNumber)).ToList();
						var chainLinkList = item.Value.Items.Where(m => m is ElstarDataSetChainLink).Select(m => (m as ElstarDataSetChainLink).ConvertToAPBCatalogFormat(restNumber)).ToList();
						var groupTypeList = item.Value.Items.Where(m => m is ElstarDataSetGroupType).Select(m => (m as ElstarDataSetGroupType).ConvertToAPBCatalogFormat(restNumber)).ToList();

						var merchList = item.Value.Items.Where(m => m is ElstarDataSetMerchandise).Select(m => (m as ElstarDataSetMerchandise).ConvertToAPBCatalogFormat(restNumber)).ToList();
						var merchDeptList = item.Value.Items.Where(m => m is ElstarDataSetMerchandiseDept).Select(m => (m as ElstarDataSetMerchandiseDept).ConvertToAPBCatalogFormat(restNumber)).ToList();
						var priceList = item.Value.Items.Where(m => m is ElstarDataSetPrice).Select(m => (m as ElstarDataSetPrice).ConvertToAPBCatalogFormat(restNumber)).ToList();
						var screenGroupList = item.Value.Items.Where(m => m is ElstarDataSetScreenGroup).Select(m => (m as ElstarDataSetScreenGroup).ConvertToAPBCatalogFormat(restNumber)).ToList();

						//db.Buttons.AddRange(buttonList);
						//db.Categories.AddRange(categoryList);
						//db.ChainLinks.AddRange(chainLinkList);
						//db.GroupTypes.AddRange(groupTypeList);
						//db.Merchandises.AddRange(merchList);
						//db.MerchandiseDepts.AddRange(merchDeptList);
						//db.Prices.AddRange(priceList);
						//db.ScreenGroups.AddRange(screenGroupList);

						//db.SaveChanges();

						EFBatchOperation.For(db, db.Buttons).InsertAll(buttonList);
						EFBatchOperation.For(db, db.Categories).InsertAll(categoryList);
						EFBatchOperation.For(db, db.ChainLinks).InsertAll(chainLinkList);
						EFBatchOperation.For(db, db.GroupTypes).InsertAll(groupTypeList);
						EFBatchOperation.For(db, db.Merchandises).InsertAll(merchList);
						EFBatchOperation.For(db, db.MerchandiseDepts).InsertAll(merchDeptList);
						EFBatchOperation.For(db, db.Prices).InsertAll(priceList);
						EFBatchOperation.For(db, db.ScreenGroups).InsertAll(screenGroupList);
					}
				}
			}
		}
		private static void UpdateCatalog(Dictionary<string, ElstarDataSet> menuDictionary)
		{
			using (APBMenuCatalogEntities db = new APBMenuCatalogEntities())
			{
				foreach (var item in menuDictionary)
				{
					string restNumber = item.Key;

					db.PurgeLandRestaurantData(restNumber);

					foreach (var mi in item.Value.Items)
					{
						//if (mi is ElstarDataSetPrice)
						//{
						//	ElstarDataSetPrice newEntity = mi as ElstarDataSetPrice;
						//	//if (newEntity.dtTimeOff)
						//	db.Prices.Add(newEntity.ConvertToAPBCatalogFormat(restNumber));
						//}


						if (mi is ElstarDataSetButton)
						{
							ElstarDataSetButton newEntity = mi as ElstarDataSetButton;
							db.Buttons.Add(newEntity.ConvertToAPBCatalogFormat(restNumber));
						}
						else if (mi is ElstarDataSetCategory)
						{
							ElstarDataSetCategory newEntity = mi as ElstarDataSetCategory;
							db.Categories.Add(newEntity.ConvertToAPBCatalogFormat(restNumber));
						}
						else if (mi is ElstarDataSetChainLink)
						{
							ElstarDataSetChainLink newEntity = mi as ElstarDataSetChainLink;
							db.ChainLinks.Add(newEntity.ConvertToAPBCatalogFormat(restNumber));
						}
						else if (mi is ElstarDataSetGroupType)
						{
							ElstarDataSetGroupType newEntity = mi as ElstarDataSetGroupType;
							db.GroupTypes.Add(newEntity.ConvertToAPBCatalogFormat(restNumber));
						}
						else if (mi is ElstarDataSetMerchandise)
						{
							ElstarDataSetMerchandise newEntity = mi as ElstarDataSetMerchandise;
							db.Merchandises.Add(newEntity.ConvertToAPBCatalogFormat(restNumber));
						}
						else if (mi is ElstarDataSetMerchandiseDept)
						{
							ElstarDataSetMerchandiseDept newEntity = mi as ElstarDataSetMerchandiseDept;
							db.MerchandiseDepts.Add(newEntity.ConvertToAPBCatalogFormat(restNumber));
						}
						else if (mi is ElstarDataSetPrice)
						{
							ElstarDataSetPrice newEntity = mi as ElstarDataSetPrice;
							db.Prices.Add(newEntity.ConvertToAPBCatalogFormat(restNumber));
						}
						else if (mi is ElstarDataSetScreenGroup)
						{
							ElstarDataSetScreenGroup newEntity = mi as ElstarDataSetScreenGroup;
							db.ScreenGroups.Add(newEntity.ConvertToAPBCatalogFormat(restNumber));
						}
					}

					db.SaveChanges();
				}
			}
		}

		private static void DoProcessing1()
		{
			//requirement: set primary keys in land data structures
			//PurgeLandRestaurantData SP must be in place

			//what's left: log4net, individual processing - ignore all files that fail to load, test same restNum/different dates

			//string path1 = @"C:\Temp\VBoxShared";
			string path = ConfigurationManager.AppSettings["directoryToProcess"];
			//List<string> individualRestaurantsToLoad = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["directoryToProcess"]) ? null : ConfigurationManager.AppSettings["directoryToProcess"].Split(new char[] { ',' }).ToList();

			if (string.IsNullOrWhiteSpace(path))
			{
				path = Directory.GetCurrentDirectory();
			}

			//path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			//string path = @"C:\Temp\Menu Data"; 
			//string fileName = @"C:\Temp\Menu.xml.gz";

			//get all 'gz' files, unzip them, Deserialize them into ElstarDataSet, and load them into a dictionary for processing
			Dictionary<string, ElstarDataSet> menuDictionary = FileHelper.GetMenuFilesMetaData(path);

			//if (individualRestaurantsToLoad != null && individualRestaurantsToLoad.Count > 0)
			//{
			//	menuDictionary = menuDictionary.Where(m => individualRestaurantsToLoad.Contains(m.Key)).ToDictionary(m => m.Key, m=> m.Value);
			//}

			//now do db updates
			UpdateCatalog(menuDictionary);
		}
		private static void Testing()
		{

			//string fileName = @"C:\Temp\Menu.xml";
			//FileInfo file = Directory.GetFiles(path);
			//FileInfo file = new FileInfo(fileName);

			//Utilities.Decompress(file);
			//ElstarDataSet ds = Utilities.Deserialize(file);



			//var filteredFiles = Directory.EnumerateFiles(path).TakeWhile(m => m.EndsWith(".gz", StringComparison.CurrentCultureIgnoreCase) || m.EndsWith(".zip", StringComparison.CurrentCultureIgnoreCase)).ToList();

			//List<FileInfo> list = Utilities.GetFileLilst(path);

			//string t = @"C:\Temp\Menu.xml";
		}
	}
}
