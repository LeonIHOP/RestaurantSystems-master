using EntityFramework.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpComponentsLoader
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				RunProcess4();
			}
			catch (Exception)
			{

				throw;
			}
		}

		private static void RunProcess4()
		{
			using (APBMenuCatalogEntities db = new APBMenuCatalogEntities())
			{
				db.Database.CommandTimeout = 0;
				List<ComponentGroup> dbComponentGroupsList = new List<ComponentGroup>();

				//List<vwMenuItemCategoryModifierMap> dataList = db.vwMenuItemCategoryModifierMaps.Where(m => m.MenuCatalogItemID == 94).ToList();
				List<vwMenuItemCategoryModifierMap> dataList = db.vwMenuItemCategoryModifierMaps.ToList();

				//get a list of uniquie MenuItemIDs
				List<int> menuItemIDList = dataList.Select(m => m.MenuCatalogItemID).Distinct().ToList();

				//iterate through the list and build out ComponentGroups collection
				menuItemIDList.ForEach(menuCatalogID =>
				{
					//Group by Sequence - primary grouping
					var componentGroupList = dataList.Where(m => m.MenuCatalogItemID == menuCatalogID).GroupBy(n => n.Sequence).Select(l => new
					{
						Sequence = l.Key,

						//within each grouping group by CondimentGroup to determine the one that wins based on count 
						CondimentGroupContainer = l.GroupBy(w => w.CondimentGroup).Select(y => new
						{
							CondimentGroup = y.Key,
							CondimentGroupCount = y.Count(),
							//need to group by a tuple(Min,Max) to get a count of the most occuring combination
							MinMaxPairList = y.GroupBy(x => new { x.Min, x.Max }).Select(h => new
							{
								Tupple = h.Key,
								TuppleCount = h.Count()
							}),

						}).ToList(),
						MinList = string.Join(",", l.OrderBy(e => e.Min).Select(w => w.Min.ToString()).Distinct()),
						MaxList = string.Join(",", l.OrderBy(e => e.Max).Select(w => w.Max.ToString()).Distinct()),
						CondimentGroupList = string.Join(",", l.Select(w => w.CondimentGroup).Distinct()),
						ModifierList = l.Select(w => w.BrandModMenuCatalogItemID).Distinct().ToList(),

					}).ToList();

					//now use extracted info to create CondimentGroup object collection
					componentGroupList.ForEach(m =>
					{
						//find max group count
						int maxGroupCount = m.CondimentGroupContainer.Max(n => n.CondimentGroupCount);

						//select a container that corresponds to the max count
						var condimentContainer = m.CondimentGroupContainer.Where(n => n.CondimentGroupCount == maxGroupCount).FirstOrDefault();

						//use data from this container
						string componentGroupName = condimentContainer.CondimentGroup;

						//within each container pick the most occuring {min, max} tuple and use its data to set Min/Max
						int maxTupleCount = condimentContainer.MinMaxPairList.Max(n => n.TuppleCount);
						int min = condimentContainer.MinMaxPairList.Where(n => n.TuppleCount == maxTupleCount).Select(n => n.Tupple.Min).FirstOrDefault();
						int max = condimentContainer.MinMaxPairList.Where(n => n.TuppleCount == maxTupleCount).Select(n => n.Tupple.Max).FirstOrDefault();

						//extract data for Details, just in case
						StringBuilder sb = new StringBuilder();
						foreach (var item in m.CondimentGroupContainer)
						{
							var condgr = item.CondimentGroup;
							foreach (var minMaxTuple in item.MinMaxPairList)
							{
								sb.AppendFormat("{0}({1},{2}):count={3};", condgr, minMaxTuple.Tupple.Min, minMaxTuple.Tupple.Max, minMaxTuple.TuppleCount);
							}
						}

						//create a ComponentGroup object
						ComponentGroup componentGroup = new ComponentGroup
						{
							ComponentGroupName = componentGroupName,
							Min = min,
							Max = max,
							MenuCatalogItemID = menuCatalogID,
							Sequence = m.Sequence,
							ComponentGroupNameList = m.CondimentGroupList,
							Details = sb.ToString().TrimEnd(new char[] { ';' }),
							MaxList = m.MaxList,
							MinList = m.MinList,
							Components = new List<Component>()
						};

						//add Components collection here - EF will properly populate all tables
						m.ModifierList.ForEach(componentID =>
						{
							componentGroup.Components.Add(new Component
							{
								MenuCatalogItemID = componentID

							});
						});

						dbComponentGroupsList.Add(componentGroup);
					});
				});

				foreach (var item in dbComponentGroupsList)
				{
					db.CreateComponentGroup(item.ComponentGroupName, item.Min, item.Max, item.MenuCatalogItemID, item.Sequence, item.ComponentGroupNameList, item.MaxList, item.MinList, item.Details,
					string.Join(",", item.Components.Select(m => m.MenuCatalogItemID)));
				}
			}
		}

		private static void RunProcess5()
		{
			using (APBMenuCatalogEntities db = new APBMenuCatalogEntities())
			{
				db.Database.CommandTimeout = 0;
				List<ComponentGroup> dbComponentGroupsList = new List<ComponentGroup>();

				//List<vwMenuItemCategoryModifierMap> dataList = db.vwMenuItemCategoryModifierMaps.Where(m => m.MenuCatalogItemID == 94).ToList();
				List<vwMenuItemCategoryModifierMap> dataList = db.vwMenuItemCategoryModifierMaps.ToList();

				//get a list of uniquie MenuItemIDs
				List<int> menuItemIDList = dataList.Select(m => m.MenuCatalogItemID).Distinct().ToList();

				//iterate through the list and build out ComponentGroups collection
				menuItemIDList.ForEach(menuCatalogID =>
				{
					//Group by Sequence - primary grouping
					var componentGroupList = dataList.Where(m => m.MenuCatalogItemID == menuCatalogID).GroupBy(n => n.Sequence).Select(l => new
					{
						Sequence = l.Key,

						//within each grouping group by CondimentGroup to determine the one that wins based on count 
						CondimentGroupContainer = l.GroupBy(w => w.CondimentGroup).Select(y => new
						{
							CondimentGroup = y.Key,
							CondimentGroupCount = y.Count(),
							//need to group by a tuple(Min,Max) to get a count of the most occuring combination
							MinMaxPairList = y.GroupBy(x => new { x.Min, x.Max }).Select(h => new
							{
								Tupple = h.Key,
								TuppleCount = h.Count()
							}),

						}).ToList(),
						MinList = string.Join(",", l.OrderBy(e => e.Min).Select(w => w.Min.ToString()).Distinct()),
						MaxList = string.Join(",", l.OrderBy(e => e.Max).Select(w => w.Max.ToString()).Distinct()),
						CondimentGroupList = string.Join(",", l.Select(w => w.CondimentGroup).Distinct()),
						//ModifierList = l.Select(w => w.BrandModMenuCatalogItemID).Distinct().ToList(),
						ModifierList = l.Select(w =>  new MenuItemContainer
						{
							BrandModMenuCatalogItemID = w.BrandModMenuCatalogItemID,
							AiiNum = w.BrandModAii 

						}).ToList()

					}).ToList();

					//now use extracted info to create CondimentGroup object collection
					componentGroupList.ForEach(m =>
					{
						//find max group count
						int maxGroupCount = m.CondimentGroupContainer.Max(n => n.CondimentGroupCount);

						//select a container that corresponds to the max count
						var condimentContainer = m.CondimentGroupContainer.Where(n => n.CondimentGroupCount == maxGroupCount).FirstOrDefault();

						//use data from this container
						string componentGroupName = condimentContainer.CondimentGroup;

						//within each container pick the most occuring {min, max} tuple and use its data to set Min/Max
						int maxTupleCount = condimentContainer.MinMaxPairList.Max(n => n.TuppleCount);
						int min = condimentContainer.MinMaxPairList.Where(n => n.TuppleCount == maxTupleCount).Select(n => n.Tupple.Min).FirstOrDefault();
						int max = condimentContainer.MinMaxPairList.Where(n => n.TuppleCount == maxTupleCount).Select(n => n.Tupple.Max).FirstOrDefault();

						//extract data for Details, just in case
						StringBuilder sb = new StringBuilder();
						foreach (var item in m.CondimentGroupContainer)
						{
							var condgr = item.CondimentGroup;
							foreach (var minMaxTuple in item.MinMaxPairList)
							{
								sb.AppendFormat("{0}({1},{2}):count={3};", condgr, minMaxTuple.Tupple.Min, minMaxTuple.Tupple.Max, minMaxTuple.TuppleCount);
							}
						}

						//create a ComponentGroup object
						ComponentGroup componentGroup = new ComponentGroup
						{
							ComponentGroupName = componentGroupName,
							Min = min,
							Max = max,
							MenuCatalogItemID = menuCatalogID,
							Sequence = m.Sequence,
							ComponentGroupNameList = m.CondimentGroupList,
							Details = sb.ToString().TrimEnd(new char[] { ';' }),
							MaxList = m.MaxList,
							MinList = m.MinList,
							Components = new List<Component>()
						};


						var zeroAiiList = m.ModifierList.Where(n => n.AiiNum == 0).GroupBy(emp => emp.BrandModMenuCatalogItemID).Select(mm => new MenuItemContainer
						{
							AiiNum = 0,
							BrandModMenuCatalogItemID = mm.Key

						}).ToList();


						var regularAiiList = m.ModifierList.Where(n => n.AiiNum != 0).GroupBy(emp => emp.AiiNum).Select(mm => new MenuItemContainer
						{
							AiiNum = mm.Key,
							BrandModMenuCatalogItemID = mm.Select(zz => zz.BrandModMenuCatalogItemID).First()
						}).ToList();


						//add Components collection here - EF will properly populate all tables
						zeroAiiList.ForEach(component =>
						{
							componentGroup.Components.Add(new Component
							{
								MenuCatalogItemID = component.BrandModMenuCatalogItemID

							});
						});

						regularAiiList.ForEach(component =>
						{
							componentGroup.Components.Add(new Component
							{
								MenuCatalogItemID = component.BrandModMenuCatalogItemID

							});
						});


						
						//m.ModifierList.ForEach(componentID =>
						//{
						//	componentGroup.Components.Add(new Component
						//	{
						//		MenuCatalogItemID = componentID

						//	});
						//});

						dbComponentGroupsList.Add(componentGroup);
					});
				});

				foreach (var item in dbComponentGroupsList)
				{
					db.CreateComponentGroup(item.ComponentGroupName, item.Min, item.Max, item.MenuCatalogItemID, item.Sequence, item.ComponentGroupNameList, item.MaxList, item.MinList, item.Details,
					string.Join(",", item.Components.Select(m => m.MenuCatalogItemID)));
				}
			}
		}

		private static void RunProcess3()
		{
			using (APBMenuCatalogEntities db = new APBMenuCatalogEntities())
			{
				List<ComponentGroup> dbComponentGroupsList = new List<ComponentGroup>();

				//List<vwMenuItemCategoryModifierMap> dataList = db.vwMenuItemCategoryModifierMaps.Where(m => m.MenuCatalogItemID == 94).ToList();
				List<vwMenuItemCategoryModifierMap> dataList = db.vwMenuItemCategoryModifierMaps.ToList();

				//get a list of uniquie MenuItemIDs
				List<int> menuItemIDList = dataList.Select(m => m.MenuCatalogItemID).Distinct().ToList();

				//iterate through the list and build out ComponentGroups collection
				menuItemIDList.ForEach(menuCatalogID =>
				{
					//Group by Sequence - primary grouping
					var componentGroupList = dataList.Where(m => m.MenuCatalogItemID == menuCatalogID).GroupBy(n => n.Sequence).Select(l => new
					{
						Sequence = l.Key,

						//within each grouping group by CondimentGroup to determine the one that wins based on count 
						CondimentGroupContainer = l.GroupBy(w => w.CondimentGroup).Select(y => new
						{
							CondimentGroup = y.Key,
							CondimentGroupCount = y.Count(),
							//need to group by a tuple(Min,Max) to get a count of the most occuring combination
							MinMaxPairList = y.GroupBy(x => new { x.Min, x.Max }).Select(h => new
							{
								Tupple = h.Key,
								TuppleCount = h.Count()
							}),

						}).ToList(),
						MinList = string.Join(",", l.OrderBy(e => e.Min).Select(w => w.Min.ToString()).Distinct()),
						MaxList = string.Join(",", l.OrderBy(e => e.Max).Select(w => w.Max.ToString()).Distinct()),
						CondimentGroupList = string.Join(",", l.Select(w => w.CondimentGroup).Distinct()),
						ModifierList = l.Select(w => w.BrandModMenuCatalogItemID).ToList(),

					}).ToList();

					//now use extracted info to create CondimentGroup object collection
					componentGroupList.ForEach(m =>
					{
						//find max group count
						int maxGroupCount = m.CondimentGroupContainer.Max(n => n.CondimentGroupCount);

						//select a container that corresponds to the max count
						var condimentContainer = m.CondimentGroupContainer.Where(n => n.CondimentGroupCount == maxGroupCount).FirstOrDefault();

						//use data from this container
						string componentGroupName = condimentContainer.CondimentGroup;

						//within each container pick the most occuring {min, max} tuple and use its data to set Min/Max
						int maxTupleCount = condimentContainer.MinMaxPairList.Max(n => n.TuppleCount);
						int min = condimentContainer.MinMaxPairList.Where(n => n.TuppleCount == maxTupleCount).Select(n => n.Tupple.Min).FirstOrDefault();
						int max = condimentContainer.MinMaxPairList.Where(n => n.TuppleCount == maxTupleCount).Select(n => n.Tupple.Max).FirstOrDefault();

						//extract data for Details, just in case
						StringBuilder sb = new StringBuilder();
						foreach (var item in m.CondimentGroupContainer)
						{
							var condgr = item.CondimentGroup;
							foreach (var minMaxTuple in item.MinMaxPairList)
							{
								sb.AppendFormat("{0}({1},{2}):count={3};", condgr, minMaxTuple.Tupple.Min, minMaxTuple.Tupple.Max, minMaxTuple.TuppleCount);
							}
						}

						//create a ComponentGroup object
						ComponentGroup componentGroup = new ComponentGroup
						{
							ComponentGroupName = componentGroupName,
							Min = min,
							Max = max,
							MenuCatalogItemID = menuCatalogID,
							Sequence = m.Sequence,
							ComponentGroupNameList = m.CondimentGroupList,
							Details = sb.ToString().TrimEnd(new char[] { ';' }),
							MaxList = m.MaxList,
							MinList = m.MinList,
							Components = new List<Component>()
						};

						//add Components collection here - EF will properly populate all tables
						m.ModifierList.ForEach(componentID =>
						{
							componentGroup.Components.Add(new Component
							{
								MenuCatalogItemID = componentID

							});
						});

						dbComponentGroupsList.Add(componentGroup);
					});
				});


				db.ComponentGroups.AddRange(dbComponentGroupsList);
				db.SaveChanges();
			}
		}

		#region DeleteMe
		private static void RunProcess1()
		{
			try
			{
				using (APBMenuCatalogEntities db = new APBMenuCatalogEntities())
				{
					List<ComponentGroup> dbComponentGroupsList = new List<ComponentGroup>();

					List<vwMenuItemCategoryModifierMap> dataList = db.vwMenuItemCategoryModifierMaps.Where(m => m.MenuCatalogItemID == 94).ToList();


					//dataList = dataList.Where(m => m.MenuCatalogItemID == 83).ToList();

					//get a list of uniquie MenuItemIDs
					List<int> menuItemIDList = dataList.Select(m => m.MenuCatalogItemID).Distinct().ToList();

					menuItemIDList.ForEach(menuCatalogID =>
					{
						var componentGroupList = dataList.Where(m => m.MenuCatalogItemID == menuCatalogID).GroupBy(n => n.Sequence).Select(l => new
						{
							//CondimentGroup = string.Empty,
							Sequence = l.Key,

							CondimentGroupContainer = l.GroupBy(w => w.CondimentGroup).Select(y => new
							{
								CondimentGroup = y.Key,
								CondimentGroupCount = y.Count(),
								//MinList = y.GroupBy(x => x.Min).Select(z => new
								//{
								//	MinValue = z.Key,
								//	MinCount = z.Count()
								//}).ToList(),
								//MaxList = y.GroupBy(x => x.Max).Select(z => new
								//{
								//	MaxValue = z.Key,
								//	MaxCount = z.Count()
								//}).ToList(),
								Min = y.Min(i => i.Min),
								Max = y.Max(i => i.Max),
							}).ToList(),
							MinList = string.Join(",", l.Select(w => w.Min.ToString()).Distinct()),
							MaxList = string.Join(",", l.Select(w => w.Max.ToString()).Distinct()),
							CondimentGroupList = string.Join(",", l.Select(w => w.CondimentGroup).Distinct()),
							ModifierList = l.Select(w => w.BrandModMenuCatalogItemID).ToList(),


						}).ToList();

						componentGroupList.ForEach(m =>
						{
							int maxGroupCount = m.CondimentGroupContainer.Max(n => n.CondimentGroupCount);
							//var condimentContainer = m.CondimentGroupContainer.Where(n => n.CondimentGroupCount == maxGroupCount).FirstOrDefault();
							//string componentGroupName = condimentContainer.CondimentGroup;

							//int minCount = condimentContainer.MinList.Min(n => n.MinCount);
							//int min = condimentContainer.MinList.Where(n => n.MinCount == minCount).Select(n => n.MinValue).FirstOrDefault();

							string componentGroupName = m.CondimentGroupContainer.Where(n => n.CondimentGroupCount == maxGroupCount).Select(w => w.CondimentGroup).FirstOrDefault();
							int min = m.CondimentGroupContainer.Where(n => n.CondimentGroupCount == maxGroupCount).Select(w => w.Min).FirstOrDefault();
							int max = m.CondimentGroupContainer.Where(n => n.CondimentGroupCount == maxGroupCount).Select(w => w.Max).FirstOrDefault();

							ComponentGroup componentGroup = new ComponentGroup
							{
								ComponentGroupName = componentGroupName,
								Min = min,
								Max = max,
								MenuCatalogItemID = menuCatalogID,
								Sequence = m.Sequence,
								ComponentGroupNameList = m.CondimentGroupList,
								MaxList = m.MaxList,
								MinList = m.MinList,
								Components = new List<Component>()
							};

							foreach (var item in m.ModifierList)
							{
								componentGroup.Components.Add(new Component
								{
									MenuCatalogItemID = item

								});
							}

							dbComponentGroupsList.Add(componentGroup);

						});

					});

					db.ComponentGroups.AddRange(dbComponentGroupsList);
					db.SaveChanges();
				}
			}
			catch (Exception)
			{

				throw;
			}
		}

		private static void RunProcess2()
		{
			try
			{
				using (APBMenuCatalogEntities db = new APBMenuCatalogEntities())
				{
					List<ComponentGroup> dbComponentGroupsList = new List<ComponentGroup>();

					List<vwMenuItemCategoryModifierMap> dataList = db.vwMenuItemCategoryModifierMaps.Where(m => m.MenuCatalogItemID == 94).ToList();

					//get a list of uniquie MenuItemIDs
					List<int> menuItemIDList = dataList.Select(m => m.MenuCatalogItemID).Distinct().ToList();

					menuItemIDList.ForEach(menuCatalogID =>
					{
						var componentGroupList = dataList.Where(m => m.MenuCatalogItemID == menuCatalogID).GroupBy(n => n.Sequence).Select(l => new
						{
							Sequence = l.Key,

							CondimentGroupContainer = l.GroupBy(w => w.CondimentGroup).Select(y => new
							{
								CondimentGroup = y.Key,
								CondimentGroupCount = y.Count(),
								MinMaxPairList = y.GroupBy(x => new { x.Min, x.Max }).Select(h => new
								{
									Tupple = h.Key,
									TuppleCount = h.Count()
								}),

							}).ToList(),
							MinList = string.Join(",", l.Select(w => w.Min.ToString()).Distinct()),
							MaxList = string.Join(",", l.Select(w => w.Max.ToString()).Distinct()),
							CondimentGroupList = string.Join(",", l.Select(w => w.CondimentGroup).Distinct()),
							ModifierList = l.Select(w => w.BrandModMenuCatalogItemID).ToList(),


						}).ToList();


						componentGroupList.ForEach(m =>
						{
							int maxGroupCount = m.CondimentGroupContainer.Max(n => n.CondimentGroupCount);
							var condimentContainer = m.CondimentGroupContainer.Where(n => n.CondimentGroupCount == maxGroupCount).FirstOrDefault();
							string componentGroupName = condimentContainer.CondimentGroup;

							int maxCount = condimentContainer.MinMaxPairList.Max(n => n.TuppleCount);
							int min = condimentContainer.MinMaxPairList.Where(n => n.TuppleCount == maxCount).Select(n => n.Tupple.Min).FirstOrDefault();
							int max = condimentContainer.MinMaxPairList.Where(n => n.TuppleCount == maxCount).Select(n => n.Tupple.Max).FirstOrDefault();

							ComponentGroup componentGroup = new ComponentGroup
							{
								ComponentGroupName = componentGroupName,
								Min = min,
								Max = max,
								MenuCatalogItemID = menuCatalogID,
								Sequence = m.Sequence,
								ComponentGroupNameList = m.CondimentGroupList,
								MaxList = m.MaxList,
								MinList = m.MinList,
								Components = new List<Component>()
							};

							foreach (var item in m.ModifierList)
							{
								componentGroup.Components.Add(new Component
								{
									MenuCatalogItemID = item

								});
							}

							dbComponentGroupsList.Add(componentGroup);

						});
					});

					db.ComponentGroups.AddRange(dbComponentGroupsList);
					db.SaveChanges();
				}
			}
			catch (Exception)
			{

				throw;
			}
		}
		#endregion

	}
}
