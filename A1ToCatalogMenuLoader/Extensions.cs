using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1ToCatalogMenuLoader
{
	public static class Extensions
	{
		public static Button ConvertToAPBCatalogFormat(this ElstarDataSetButton source, string costCenter)
		{
			Button returnObject = new Button
			{
				CostCenter = costCenter,
				iButtonID = Convert.ToInt32(source.iButtonID),
				iCategoryID = Convert.ToInt32(source.iCategoryID),
				iColorID = Convert.ToInt32(source.iColorID),
				iGroupTypeID = 0,
				iItemTierID = 0,
				iMerchandiseID = Convert.ToInt32(source.iMerchandiseID),
				iScreenGroupID = Convert.ToInt32(source.iScreenGroupID),
				iScreenPosition = Convert.ToInt32(source.iScreenPosition),
			};

			return returnObject;
		}


		public static Category ConvertToAPBCatalogFormat(this ElstarDataSetCategory source, string costCenter)
		{
			Category returnObject = new Category
			{
				iCategoryID = Convert.ToInt32(source.iCategoryID),
				iPrintOrder = Convert.ToInt32(source.iPrintOrder),
				sName = source.sName,
				bActive = Convert.ToBoolean(source.bActive),
				CostCenter = costCenter,
			};

			return returnObject;
		}

		public static ChainLink ConvertToAPBCatalogFormat(this ElstarDataSetChainLink source, string costCenter)
		{
			ChainLink returnObject = new ChainLink
			{
				iChainLinkID = Convert.ToInt32(source.iChainLinkID),
				iSequence = Convert.ToInt32(source.iSequence),
				iButtonID = Convert.ToInt32(source.iButtonID),
				iScreenGroupID = Convert.ToInt32(source.iScreenGroupID),
				iMinRequired = Convert.ToInt32(source.iMinRequired),
				iMaxRequired = Convert.ToInt32(source.iMaxRequired),
				CostCenter = costCenter,
			};

			return returnObject;
		}

		public static GroupType ConvertToAPBCatalogFormat(this ElstarDataSetGroupType source, string costCenter)
		{
			GroupType returnObject = new GroupType
			{
				iGroupTypeID = Convert.ToInt32(source.iGroupTypeID),
				sName = source.sName,
				sDesc = source.sDesc,
				CostCenter = costCenter,
			};

			return returnObject;
		}

		

		public static Merchandise ConvertToAPBCatalogFormat(this ElstarDataSetMerchandise source, string costCenter)
		{
			Merchandise returnObject = new Merchandise
			{
				iMerchandiseID = Convert.ToInt32(source.iMerchandiseID),
				iPriceGroupID = Convert.ToInt32(source.iPriceGroupID),
				iMerchandiseDeptID = Convert.ToInt32(source.iMerchandiseDeptID),
				iAiiNum = Convert.ToInt32(source.iAiiNum),
				sItemName = source.sItemName,
				sOrderName = source.sOrderName,
				iCookTime = Convert.ToInt32(source.iCookTime),
				iPrepOrder = Convert.ToInt32(source.iPrepOrder),
				iQtyAvail = Convert.ToInt32(source.iQtyAvail),
				bIsModifier = Convert.ToBoolean(source.bIsModifier),
				bIsLocked = Convert.ToBoolean(source.bIsLocked),
				bIsDaily = Convert.ToBoolean(source.bIsDaily),
				bPrintSameLine = Convert.ToBoolean(source.bPrintSameLine),
				bForceRecipe = Convert.ToBoolean(source.bForceRecipe),
				bNotSold = Convert.ToBoolean(source.bNotSold),
				bPromptForPrice = Convert.ToBoolean(source.bPromptForPrice),
				bPrintOnOrder = Convert.ToBoolean(source.bPrintOnOrder),
				bPrintOnReceipt = Convert.ToBoolean(source.bPrintOnReceipt),
				bIgnoreItem = Convert.ToBoolean(source.bIgnoreItem),
				bPrintRecipe = Convert.ToBoolean(source.bPrintRecipe),
				bBeverage = Convert.ToBoolean(source.bBeverage),
				bEntreAppetizer = Convert.ToBoolean(source.bEntreAppetizer),
				bDeleted = Convert.ToBoolean(source.bDeleted),
				dtModified = Convert.ToDateTime(source.dtModified),				
				bGroupingItem = Convert.ToBoolean(source.bGroupingItem),
				bPrintGroupItems = Convert.ToBoolean(source.bPrintGroupItems),
				bAllowPartialSend = Convert.ToBoolean(source.bAllowPartialSend),
				CostCenter = costCenter,
			};

			return returnObject;
		}

		public static MerchandiseDept ConvertToAPBCatalogFormat(this ElstarDataSetMerchandiseDept source, string costCenter)
		{
			MerchandiseDept returnObject = new MerchandiseDept
			{
				iMerchandiseDeptID = Convert.ToInt32(source.iMerchandiseDeptID),
				iTipable = Convert.ToInt32(source.iTipable),
				sName = source.sName,
				bActive = Convert.ToBoolean(source.bActive),
				bVATTax = Convert.ToBoolean(source.bVATTax),
				bPriceIncludesTax = Convert.ToBoolean(source.bPriceIncludesTax),
				CostCenter = costCenter,
			};

			return returnObject;
		}

		public static Price ConvertToAPBCatalogFormat(this ElstarDataSetPrice source, string costCenter)
		{
			Price returnObject = new Price
			{
				iPriceID = Convert.ToInt32(source.iPriceID),
				iPriceDescID = Convert.ToInt32(source.iPriceDescID),
				iAmount = Convert.ToDecimal(source.iAmount),
				iDayOn = Convert.ToInt32(source.iDayOn),
				dtTimeOn = Convert.ToDateTime(source.dtTimeOn),
				//dtTimeOff = source.dtTimeOff == null ? Convert.ToDateTime("1900-01-01T00:00:00") : Convert.ToDateTime(source.dtTimeOff),
				dtTimeOff = (!string.IsNullOrWhiteSpace(source.dtTimeOff) && source.dtTimeOff != "1900-01-01T00:00:00") ? Convert.ToDateTime(source.dtTimeOff) : Convert.ToDateTime("1900-01-01T00:00:00"),
				//dtTimeOff = DateTime.MinValue,
				iButtonID = Convert.ToInt32(source.iButtonID),
				iGroupTypeID = Convert.ToInt32(source.iGroupTypeID),
				iWeight = 0, //no value supplied
				CostCenter = costCenter,
			};

			return returnObject;
		}

		public static ScreenGroup ConvertToAPBCatalogFormat(this ElstarDataSetScreenGroup source, string costCenter)
		{
			ScreenGroup returnObject = new ScreenGroup
			{
				iScreenGroupID = Convert.ToInt32(source.iScreenGroupID),
				iScreenPosition = Convert.ToInt32(source.iScreenPosition),
				sName = source.sName,
				iAutoSort = Convert.ToInt32(source.iAutoSort),
				bActive = Convert.ToBoolean(source.bActive),
				CostCenter = costCenter,
			};

			return returnObject;
		}

		public static string RemoveFromEnd(this string str, string segmentToRemove)
		{
			if (str.ToLower().EndsWith(segmentToRemove.ToLower()))
				return str.Substring(0, str.Length - segmentToRemove.Length);
			else
				return str;
		}
	}
}
