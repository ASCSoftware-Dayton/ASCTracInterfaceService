using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ASCTracInterfaceDll.Imports
{
    public class ImportItemMaster
    {
        //private string funcType = "IM_ITEM";
        private string siteid = string.Empty;
        private Class1 myClass;
        private Model.Item.ItemImportConfig currImportConfig;
        public static HttpStatusCode doImportItem(Class1 myClass, ASCTracInterfaceModel.Model.Item.ItemMasterImport aData, ref string errmsg)
        {
            //myClass = Class1.InitParse(funcType, ref errmsg);
            HttpStatusCode retval = HttpStatusCode.OK;
            string ItemID = aData.PRODUCT_CODE;
            string updstr = string.Empty;
            try
            {
                if (!myClass.FunctionAuthorized(myClass.myLogRecord.FunctionID))
                    retval = HttpStatusCode.NonAuthoritativeInformation;
                else
                {
                    var siteid = myClass.GetSiteIdFromHostId(aData.FACILITY);
                    if (String.IsNullOrEmpty(siteid))
                    {
                        myClass.myLogRecord.LogType = "E";
                        errmsg = "No Facility or Site defined for record.";
                        retval = HttpStatusCode.BadRequest;
                    }

                    if (string.IsNullOrEmpty(ItemID))
                    {
                        myClass.myLogRecord.LogType = "E";
                        errmsg = "Itemid (PRODUCT_CODE) value is required.";
                        retval = HttpStatusCode.BadRequest;
                    }
                    else
                    {
                        var myImport = new ImportItemMaster(myClass, siteid);
                        retval = myImport.ImportItemRecord(aData, ref errmsg);
                    }
                }
            }
            catch (Exception ex)
            {
                myClass.LogException(ex);
                retval = HttpStatusCode.BadRequest;
                errmsg = ex.Message;
            }
            return (retval);
        }

        public ImportItemMaster(Class1 aClass, string aSiteID)
        {
            myClass = aClass;
            siteid = aSiteID;
            currImportConfig = Configs.ItemConfig.getImportSite(siteid, myClass.myParse.Globals);
        }

        private HttpStatusCode ImportItemRecord(ASCTracInterfaceModel.Model.Item.ItemMasterImport aData, ref string errmsg)
        {
            HttpStatusCode retval = HttpStatusCode.OK;
            string sqlStr, tmpStr;
            string itemId, ascItemId, vmiCustId, itemDesc, itemDesc2, fcCode = "";
            string abcZone, zoneId, stockUom, buyUom, cwUom, labelUom, vendorId;
            string conv12 = "", conv23 = "", conv34 = "";
            string uom1 = "", uom2 = "", uom3 = "", uom4 = "";
            double unitWidth, unitLength, unitHeight, unitWeight, itemCubic;
            bool recExists, dualUnitItem;

            myClass.myParse.Globals.initsite(siteid);
            myClass.myParse.Globals.mydmupdate.InitUpdate();

            itemId = aData.PRODUCT_CODE.ToUpper().Trim();
            vmiCustId = Utils.ASCUtils.GetTrimString(aData.VMI_CUSTID, string.Empty).ToUpper();


            bool fUseVMI = myClass.myParse.Globals.myConfig.iniGNVMI.boolValue;
            if (fUseVMI)  //(Service1.Parse.Globals.myConfig.iniGNVMI.boolValue)  //changed 08-11-15 (JXG) for Allen Dist
                ascItemId = siteid + "&" + itemId + "&" + vmiCustId;
            else
                ascItemId = siteid + "&" + itemId + "&";

            sqlStr = "SELECT ITEM_STATUS, FREIGHT_CLASS_CODE FROM ITEMMSTR WHERE ASCITEMID='" + ascItemId + "'";
            recExists = myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sqlStr, "", ref fcCode);

            string itemstatus = ascLibrary.ascStrUtils.GetNextWord(ref fcCode);
            itemDesc = Utils.ASCUtils.GetTrimString(aData.DESCRIPTION, string.Empty);
            itemDesc2 = Utils.ASCUtils.GetTrimString(aData.PROD_ALTDESC, string.Empty);
            vendorId = Utils.ASCUtils.GetTrimString(aData.VENDORID, String.Empty);
            if (itemDesc.Length > 60)  //added 03-11-16 (JXG)
                itemDesc = itemDesc.Substring(0, 60);
            if (itemDesc2.Length > 60)  //added 03-11-16 (JXG)
                itemDesc2 = itemDesc2.Substring(0, 60);

            zoneId = aData.ZONEID;
            if (string.IsNullOrEmpty(zoneId))
                zoneId = myClass.GetZone(siteid);

            stockUom = Utils.ASCUtils.GetTrimString(aData.STOCK_UOM, "EA");
            cwUom = Utils.ASCUtils.GetTrimString(aData.CW_UOM, String.Empty);
            labelUom = Utils.ASCUtils.GetTrimString(aData.LABEL_UOM, "PL");

            dualUnitItem = false;
            if (String.IsNullOrEmpty(cwUom) == false)
                dualUnitItem = true;

            if (dualUnitItem)
                buyUom = cwUom;
            else
                buyUom = Utils.ASCUtils.GetTrimString(aData.RECEIVING_UOM, stockUom);

            abcZone = Utils.ASCUtils.GetTrimString(aData.ABC_ZONE, "B");

            string updstr = string.Empty;

            if( string.IsNullOrEmpty( aData.STATUS_FLAG))
            {
                errmsg = "Invalid status flag";
                myClass.myLogRecord.LogType = "E";
                return (HttpStatusCode.BadRequest);
            }

            string status = aData.STATUS_FLAG.Trim();
            if (status == "D" || status == "O")
            {
                // If item not already obsolete, update status and set obsoleted date
                if (itemstatus != "O")
                {
                    Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "ITEM_STATUS", "O");
                    Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "OBSOLETED_DATE", "GETDATE()");
                }
            }
            else if (status == "I")
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "ITEM_STATUS", "I");
            else if ((status == "A") || (!recExists))  //added 10-12-16 (JXG) for Driscoll's
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "ITEM_STATUS", "A"); // Default to 'A' in case invalid flag used


            if (!recExists)
            {
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "ASCITEMID", ascItemId);
                //Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "ASCITEMID", ascItemId);
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "ITEMID", itemId);
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "SITE_ID", siteid);
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "ZONEID", zoneId);
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "ABCZONE", abcZone);

                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "CREATE_DATE", DateTime.Now.ToString());
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "CREATE_USERID", currImportConfig.GatewayUserID);

                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "ITEMHEIGHT", "1");
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "ITEMLENGTH", "1");
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "ITEMWIDTH", "1");
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "MAXSKIDSHIGH", "1");
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "SKIDWIDTH", "1");
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "SKIDLENGTH", "1");
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "SKID_HEIGHT", "1");

                conv12 = "1";

                if (!String.IsNullOrEmpty(labelUom))
                {
                    Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "LABEL_UOM", labelUom);
                    uom1 = labelUom;
                }
                else if (stockUom == "EA")
                {
                    Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "LABEL_UOM", "EA");
                    uom1 = "EA";
                }
                else
                {
                    Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "LABEL_UOM", currImportConfig.defLabelUOM);
                    uom1 = currImportConfig.defLabelUOM;
                }

                SetItemReqFields(vmiCustId, ascItemId);  //added 02-02-15 (JXG) for BMS
            }

            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "LAST_IMPORT_DATE", DateTime.Now.ToString());
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "LAST_UPDATE", DateTime.Now.ToString());
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "LAST_UPDATE_USERID", "GATEWAY");

            //moved from out of !recEists 06-10-16 (JXG)
            //changed 06-10-16 (JXG)
            string serTrack = Utils.ASCUtils.GetTrimString(aData.SERIAL_TRACKED, string.Empty);
            if (!string.IsNullOrEmpty(serTrack))
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "SER_NUM_FLAG", serTrack);

            //moved from out of !recEists 06-10-16 (JXG)
            string skidTrack = Utils.ASCUtils.GetTrimString(aData.SKID_TRACKED, string.Empty);
            if (!recExists && String.IsNullOrEmpty(skidTrack))  //added !recExists 06-10-16 (JXG)
                skidTrack = currImportConfig.defTrackBy;
            if (!string.IsNullOrEmpty(skidTrack) && !myClass.myParse.Globals.myDBUtils.ifRecExists("SELECT TOP 1 SKIDID FROM LOCITEMS WHERE ASCITEMID='" + ascItemId + "'"))
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "TRACKBYSKID", skidTrack);
            ////////////////////////////////////////////

            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "BUYER", Utils.ASCUtils.GetTrimString(aData.BUYER, string.Empty));

            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "CATID", Utils.ASCUtils.GetTrimString(aData.CATEGORY, string.Empty));

            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "CAT2ID", Utils.ASCUtils.GetTrimString(aData.CATEGORY_2, string.Empty));

            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "DESCRIPTION", itemDesc);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "DESCRIPTION2", itemDesc2);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "STANDARDCOST", aData.STD_COST.ToString());

            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "STOCK_UOM", stockUom);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "RETAILPRICE", aData.RETAIL_PRICE.ToString());

            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "SCC14", Utils.ASCUtils.GetTrimString(aData.SCC14, ""));

            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "SHELFLIFE", aData.SHELF_LIFE.ToString());

            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "HOLD_DATA", Utils.ASCUtils.GetTrimString(aData.AUTO_QC_REASON, string.Empty));

            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "BUNDLE_SIZE", Utils.ASCUtils.GetTrimString(aData.BUNDLE_SIZE, string.Empty));

            ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "ORDERMINIMUM", aData.ORDERMINIMUM.ToString());
            ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "MRP_SAFETY_STOCK", aData.MRP_SAFETY_STOCK.ToString());

            //ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty( ref updstr,"THUMBNAIL_FILENAME", thumbnailFilename);  //taken out 10-02-17 (JXG)
            if (String.IsNullOrEmpty(buyUom))
                buyUom = stockUom;
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "BUY_UOM", buyUom);

            double tmpConvFact = aData.BASE_TO_RECV_CONV_FACTOR;
            if (buyUom != stockUom && buyUom != "" && tmpConvFact != 0)
            {
                uom2 = buyUom;
                conv23 = tmpConvFact.ToString();
                uom3 = stockUom;
            }
            else if (stockUom != "")
            {
                uom2 = stockUom;
            }

            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "BOL_UNITWEIGHT", aData.PRODUCT_WEIGHT.ToString());

            if (String.IsNullOrEmpty(fcCode))
            {
                tmpStr = aData.FREIGHT_CLASS_CODE;
                if (!String.IsNullOrEmpty(tmpStr))
                    Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "FREIGHT_CLASS_CODE", tmpStr);
                else if (!String.IsNullOrEmpty(currImportConfig.defFreightClass))
                    Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "FREIGHT_CLASS_CODE", currImportConfig.defFreightClass);
            }

            if (dualUnitItem)
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "DUAL_UNIT_ITEM", "T");

            /*
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty( ref updstr,"LABELDATA1", aData.ITEM_CUSTOMDATA1.Trim());
                                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty( ref updstr,"LABELDATA2", aData.ITEM_CUSTOMDATA2.Trim());
                                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty( ref updstr,"LABELDATA3", aData.ITEM_CUSTOMDATA3.Trim());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty( ref updstr,"LABELDATA4", aData.CUSTOM_DATA4.Trim());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty( ref updstr,"LABELDATA5", aData.CUSTOM_DATA5.Trim());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty( ref updstr,"LABELDATA6", aData.CUSTOM_DATA6.Trim());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty( ref updstr,"LABELDATA7", aData.CUSTOM_DATA7.Trim());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty( ref updstr,"LABELDATA8", aData.CUSTOM_DATA8.Trim());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty( ref updstr,"LABELDATA9", aData.CUSTOM_DATA9.Trim());
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty( ref updstr,"LABELDATA10", aData.CUSTOM_DATA10.Trim());
            */
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "UPCCODE", Utils.ASCUtils.GetTrimString(aData.UPC_CODE, string.Empty));

            tmpStr = Utils.ASCUtils.GetTrimString(aData.ITEM_TYPE, string.Empty);
            if (string.IsNullOrEmpty(tmpStr) && !recExists)
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "PURORMFG", currImportConfig.defItemType);
            else
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "PURORMFG", tmpStr);

            uom1 = Utils.ASCUtils.GetTrimString(aData.UNIT1_UOM, uom1);
            if (String.IsNullOrEmpty(uom1))
            {
                if (stockUom == "EA")
                    uom1 = "EA";
            }
            if (recExists && currImportConfig.doNotUpdateUOMValues)
                uom1 = string.Empty;

            uom2 = Utils.ASCUtils.GetTrimString(aData.UNIT2_UOM, uom2);
            if (String.IsNullOrEmpty(uom2))
            {
            }
            if (recExists && currImportConfig.doNotUpdateUOMValues)
                uom2 = string.Empty;

            uom3 = Utils.ASCUtils.GetTrimString(aData.UNIT3_UOM, uom2);
            if (recExists && currImportConfig.doNotUpdateUOMValues)
                uom3 = string.Empty;

            uom4 = Utils.ASCUtils.GetTrimString(aData.UNIT4_UOM, uom4);
            if (recExists && currImportConfig.doNotUpdateUOMValues)
                uom4 = string.Empty;

            if (aData.CONVERSION_UNIT_1 > 0 )
                conv12 = aData.CONVERSION_UNIT_1.ToString();
            else if (recExists && currImportConfig.doNotUpdateUOMValues)
                conv12 = string.Empty;

            if (aData.CONVERSION_UNIT_2 > 0)
                conv23 = aData.CONVERSION_UNIT_2.ToString();
            else if (recExists && currImportConfig.doNotUpdateUOMValues)
                conv23 = string.Empty;

            if (aData.CONVERSION_UNIT_3 > 0)
                conv34 = aData.CONVERSION_UNIT_3.ToString();
            else if (recExists && currImportConfig.doNotUpdateUOMValues)
                conv34 = string.Empty;

            if (recExists && !currImportConfig.doNotUpdateUOMValues)
            {
                if (!String.IsNullOrEmpty(labelUom))
                    Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "LABEL_UOM", labelUom);
                else if (stockUom == "EA")
                    Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "LABEL_UOM", "EA");
            }

            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "UNIT_MEAS1", uom1);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "UNIT_MEAS2", uom2);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "UNIT_MEAS3", uom3);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "UNIT_MEAS4", uom4);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "CONV_FACT_12", conv12);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "CONV_FACT_23", conv23);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "CONV_FACT_34", conv34);

            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "GTIN_UNIT1_ID", Utils.ASCUtils.GetTrimString(aData.GTIN_CODE_1, string.Empty));
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "GTIN_UNIT2_ID", Utils.ASCUtils.GetTrimString(aData.GTIN_CODE_2, string.Empty));
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "GTIN_UNIT3_ID", Utils.ASCUtils.GetTrimString(aData.GTIN_CODE_3, string.Empty));
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "GTIN_UNIT4_ID", Utils.ASCUtils.GetTrimString(aData.GTIN_CODE_4, string.Empty)); 


            unitWidth = aData.UNITWIDTH;
            unitLength = aData.UNITLENGTH;
            unitHeight = aData.UNITHEIGHT;
            unitWeight = aData.UNITWEIGHT;


            if (unitWidth > 0)
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "UNITWIDTH", unitWidth.ToString());
            if (unitLength > 0)
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "UNITLENGTH", unitLength.ToString());
            if (unitHeight > 0)
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "UNITHEIGHT", unitHeight.ToString());
            if (unitWeight > 0)
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "UNITWEIGHT", unitWeight.ToString());

            itemCubic = aData.CUBIC_PER_EACH;
            if (itemCubic <= 0)
                itemCubic = unitWidth * unitLength * unitHeight;
            if (itemCubic > 0)
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "ITEMCUBIC", itemCubic.ToString());

            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "TARE_WEIGHT", aData.TARE_WEIGHT.ToString());
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "BULK_TARE_WEIGHT", aData.BULK_TARE_WEIGHT.ToString());

            if (!String.IsNullOrEmpty(aData.BILL_UOM))
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "BILL_UOM", aData.BILL_UOM);
            else
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "BILL_UOM", cwUom);

            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "COUNTRY_OF_ORIGIN", aData.COUNTRY_OF_ORIGIN);



            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "HAZMAT_FLAG", Utils.ASCUtils.GetTrimString( aData.HAZMAT_FLAG, string.Empty));
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "LOT_FLAG", aData.LOT_FLAG);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "LOT_PROD_FLAG", aData.LOT_PROD_FLAG);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "POST_LOT_TO_HOST_FLAG", aData.POST_LOT_TO_HOST_FLAG);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "EXP_DATE_REQ_FLAG", aData.EXP_DATE_REQ_FLAG);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "EXPIRE_DAYS", aData.EXPIRE_DAYS.ToString());

            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "MINIMUM", aData.MINIMUM.ToString());
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "RESTOCK_QTY", aData.RESTOCK_QTY.ToString());
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "MAXIMUM", aData.MAXIMUM.ToString());
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "LEADTIME", aData.LEADTIME.ToString());
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "INHOUSE_TIME", aData.INHOUSE_TIME.ToString());


            if (fUseVMI)  //(Service1.Parse.Globals.myConfig.iniGNVMI.boolValue)  //changed 08-11-15 (JXG) for Allen Dist
            {
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "VMI_CUSTID", vmiCustId);
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "VMI_VENDORID", vendorId);

                string vmiRespId = aData.VMI_RESPID;
                if (String.IsNullOrEmpty(vmiRespId))
                {
                    if (!recExists)  //added (!recExists) 10-12-16 (JXG) for Driscoll's
                    {
                        Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "VMI_RESPID", vendorId);
                        Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "VMI_RESPFLAG", "V");
                    }
                }
                else
                {
                    Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "VMI_RESPID", vmiRespId);
                    Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "VMI_RESPFLAG", "C");
                }

                if (String.IsNullOrEmpty(vmiCustId))
                {
                    if (!recExists)  //added (!recExists) 10-12-16 (JXG) for Driscoll's
                    {
                        Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "VMI_OWNERID", vendorId);
                        Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "VMI_OWNERFLAG", "V");
                    }
                }
                else
                {
                    Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "VMI_OWNERID", vmiCustId);
                    Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "VMI_OWNERFLAG", "C");
                }
            }
            else
            {
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "VENDOR1", vendorId);
            }

            SaveCustomFields(ref updstr, aData.CustomList, currImportConfig.GWTranslation);

            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "ORGANIC_FLAG", aData.ORGANIC_FLAG);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "PKG_MATERIAL_FLAG", aData.PKG_MATERIAL_FLAG);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "MFG_ID", aData.MFG_ID);
            Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "VENDOR1ITEMNUM", aData.VENDOR1ITEMNUM);

            if (!recExists)
                myClass.myParse.Globals.mydmupdate.InsertRecord("ITEMMSTR", updstr);
            else
                myClass.myParse.Globals.mydmupdate.UpdateFields("ITEMMSTR", updstr, "ASCITEMID='" + ascItemId + "'");
            myClass.ImportCustomData(myClass.myLogRecord.FunctionID, "ITEMMSTR", "ASCITEMID='" + ascItemId + "'", itemId);  //added 10-17-17 (JXG)
            SaveExtData(ascItemId, aData.ExtDataList);
            myClass.myParse.Globals.dmMiscItem.CalcItemSubUOMConv(ascItemId);
            //if (!recExists)
                myClass.myParse.Globals.dmMiscItem.SetItemDefaultsFromCategory(ascItemId, aData.CATEGORY);

            UpdateMissingItemQtyRecords();

            double hostQty = aData.HOST_QTY;
            updstr = "LAST_MF_QTY=" + hostQty.ToString() + ", LAST_MF_QTY_STATIC = " + hostQty.ToString();
            myClass.myParse.Globals.mydmupdate.UpdateFields("ITEMQTY", updstr, "ASCITEMID='" + ascItemId + "'");

            if (myClass.myParse.Globals.myConfig.vmProduction.boolValue && !String.IsNullOrEmpty(aData.REF_NOTES))
            {
                sqlStr = "SELECT ASCITEMID FROM BOMHDR WHERE ASCITEMID='" + ascItemId + "'";
                recExists = myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sqlStr, "", ref tmpStr);
                updstr = "";
                Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "REF_NOTES", aData.REF_NOTES);
                if (recExists)
                    myClass.myParse.Globals.mydmupdate.UpdateFields("BOMHDR", updstr, "ASCITEMID='" + ascItemId + "'");
                else
                {
                    Utils.ASCUtils.CheckAndAppend(ref updstr, "ITEMMSTR", "ASCITEMID", ascItemId);
                    myClass.myParse.Globals.mydmupdate.InsertRecord("BOMHDR", updstr);
                }
            }
            //////////////////////
            int seq = 1;
            foreach (var rec in aData.NotesList)
                ImportNotes.SaveNotes("I", ascItemId, rec.NOTE, false, 0, seq++, myClass.myParse.Globals);

            if (retval == HttpStatusCode.OK)
                myClass.myParse.Globals.mydmupdate.ProcessUpdates();

            return (retval);
        }

        private void SaveCustomFields(ref string updStr, List<ASCTracInterfaceModel.Model.ModelCustomData> CustomList, Dictionary<string, List<string>> TranslationList)
        {
            foreach (var rec in CustomList)
            {
                if (TranslationList.ContainsKey(rec.FieldName.ToUpper()))
                {
                    var asclist = TranslationList[rec.FieldName.ToUpper()];
                    foreach (var ascfield in asclist)
                    {

                        ascLibrary.ascStrUtils.ascAppendSetStr(ref updStr, ascfield, rec.Value);
                    }
                }
            }
        }
        private void SaveExtData(string ascitemid, Dictionary<string, string> ExtDataList)
        {
            if (ExtDataList.Count > 0)
            {
                myClass.myParse.Globals.mydmupdate.DeleteRecord("EXTDATA", "RECID='" + ascitemid + "'");
                foreach (var rec in ExtDataList)
                {
                    if (!String.IsNullOrEmpty(rec.Value))
                    {
                        string promptID = string.Empty;
                        if (myClass.myParse.Globals.myDBUtils.ReadFieldFromDB("SELECT PROMPT_NUM FROM EXTDATA_SETUP WHERE TBLNAME='ITEMMSTR' AND DISPLAY_STRING='" + rec.Key + "'", "", ref promptID))
                        {
                            string updstr = "";
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "TBLNAME", "ITEMMSTR");
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "PROMPT_NUM", promptID);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "RECID", ascitemid);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "VALUE", rec.Value);
                            myClass.myParse.Globals.mydmupdate.InsertRecord("EXTDATA", updstr);
                        }
                    }
                }
            }
        }

        private void UpdateMissingItemQtyRecords()
        {
            string sql = "INSERT INTO ITEMQTY (ASCITEMID,QTYTOTAL,QTYALLOC,QTYONHOLD,QTYSUBITEMS,QTYREQUIRED,QTYSCHEDULED) " +
                "(SELECT ASCITEMID,0,0,0,0,0,0 FROM ITEMMSTR WHERE ASCITEMID NOT IN (SELECT ASCITEMID FROM ITEMQTY))";
            myClass.myParse.Globals.mydmupdate.AddToUpdate(sql);
        }

        private bool SetItemReqFields(string aCustID, string aASCItemID)
        {
            if ((!String.IsNullOrEmpty(aCustID)) && (!String.IsNullOrEmpty(aASCItemID)))
            {
                string updStr = "INSERT INTO RECV_REQ_FIELDS" +
                    " (RECTYPE, RECID, FIELDNAME, DESCRIPTION, REQUIRED, CREATEUSERID, CREATEDATE)" +
                    " SELECT 'I', '" + aASCItemID + "', FIELDNAME, DESCRIPTION, REQUIRED, 'IMPORT', GETDATE()" +
                    " FROM RECV_REQ_FIELDS" +
                    " WHERE RECTYPE='C'" +
                    " AND RECID='" + aCustID + "'" +
                    " AND (SELECT TOP 1 RECID FROM RECV_REQ_FIELDS R2" +
                    " WHERE R2.RECTYPE='I' AND R2.RECID='" + aASCItemID + "'" +
                    " AND R2.FIELDNAME=RECV_REQ_FIELDS.FIELDNAME) IS NULL";
                myClass.myParse.Globals.mydmupdate.AddToUpdate(updStr);
            }
            return true;
        }

    }

}