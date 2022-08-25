using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ASCTracInterfaceDll.Imports
{
    public class ImportItemMaster
    {
        private static string funcType = "IM_ITEM";
        private static string siteid = string.Empty;
        private static Class1 myClass;
        private static Model.Item.ItemImportConfig currImportConfig;
        public static HttpStatusCode doImportItem(ASCTracInterfaceModel.Model.Item.ItemMasterImport aData, ref string errmsg)
        {
            myClass = Class1.InitParse(funcType, ref errmsg);
            HttpStatusCode retval = HttpStatusCode.OK;
            string ItemID = aData.PRODUCT_CODE;
            string updstr = string.Empty;
            try
            {
                if (myClass != null)
                {
                    if (!myClass.FunctionAuthorized(funcType))
                        retval = HttpStatusCode.NonAuthoritativeInformation;
                    else
                    {
                        siteid = myClass.GetSiteIdFromHostId(aData.FACILITY);
                        currImportConfig = Configs.ItemConfig.getImportSite(siteid, myClass.myParse.Globals);
                        if (String.IsNullOrEmpty(siteid))
                        {
                            errmsg = "No Facility or Site defined for record.";
                            retval = HttpStatusCode.BadRequest;
                        }
                        myClass.myParse.Globals.mydmupdate.InitUpdate();

                        retval = ImportItemRecord(aData, ref errmsg);
                        if( retval == HttpStatusCode.OK)
                            myClass.myParse.Globals.mydmupdate.ProcessUpdates();

                    }
                }
                else
                    retval = HttpStatusCode.InternalServerError;
            }
            catch (Exception ex)
            {
                Class1.WriteException(funcType, Newtonsoft.Json.JsonConvert.SerializeObject(aData), ItemID, ex.ToString(), updstr);
                retval = HttpStatusCode.BadRequest;
                errmsg = ex.Message;

            }
            return (retval);
        }

        private static HttpStatusCode ImportItemRecord(ASCTracInterfaceModel.Model.Item.ItemMasterImport aData, ref string errmsg)
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

            itemId = aData.PRODUCT_CODE.ToUpper().Trim();
            vmiCustId = aData.VMI_CUSTID.ToUpper().Trim();


            bool fUseVMI = myClass.myParse.Globals.myConfig.iniGNVMI.boolValue;
            if (fUseVMI)  //(Service1.Parse.Globals.myConfig.iniGNVMI.boolValue)  //changed 08-11-15 (JXG) for Allen Dist
                ascItemId = siteid + "&" + itemId + "&" + vmiCustId;
            else
                ascItemId = siteid + "&" + itemId + "&";

            sqlStr = "SELECT ITEM_STATUS, FREIGHT_CLASS_CODE FROM ITEMMSTR WHERE ASCITEMID='" + ascItemId + "'";
            recExists = myClass.myParse.Globals.myDBUtils.ReadFieldFromDB(sqlStr, "", ref fcCode);

            string itemstatus = ascLibrary.ascStrUtils.GetNextWord(ref fcCode);
            itemDesc = aData.DESCRIPTION.Trim();
            itemDesc2 = aData.PROD_ALTDESC.Trim();
            vendorId = aData.VENDORID.Trim();
            if (itemDesc.Length > 60)  //added 03-11-16 (JXG)
                itemDesc = itemDesc.Substring(0, 60);
            if (itemDesc2.Length > 60)  //added 03-11-16 (JXG)
                itemDesc2 = itemDesc2.Substring(0, 60);

            zoneId = aData.ZONEID;
            if (string.IsNullOrEmpty(zoneId))
                zoneId = myClass.GetZone(siteid);

            stockUom = aData.STOCK_UOM.Trim();
            cwUom = aData.CW_UOM.Trim();
            labelUom = aData.LABEL_UOM.Trim();

            dualUnitItem = false;
            if (String.IsNullOrEmpty(cwUom) == false)
                dualUnitItem = true;

            if (dualUnitItem)
                buyUom = cwUom;
            else
                buyUom = aData.RECEIVING_UOM.Trim();


            abcZone = aData.ABC_ZONE.Trim();
            if (String.IsNullOrEmpty(abcZone))
                abcZone = "B";

            string updstr = string.Empty;

            string status = aData.STATUS_FLAG.Trim();
            if (status == "D" || status == "O")
            {
                // If item not already obsolete, update status and set obsoleted date
                if (itemstatus != "O")
                {
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ITEM_STATUS", "O");
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "OBSOLETED_DATE", "GETDATE()");
                }
            }
            else if (status == "I")
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ITEM_STATUS", "I");
            else if ((status == "A") || (!recExists))  //added 10-12-16 (JXG) for Driscoll's
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ITEM_STATUS", "A"); // Default to 'A' in case invalid flag used


            if (!recExists)
            {
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ASCITEMID", ascItemId);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ITEMID", itemId);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SITE_ID", siteid);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ZONEID", zoneId);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ABCZONE", abcZone);

                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CREATE_DATE", DateTime.Now.ToString());
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CREATE_USERID", currImportConfig.GatewayUserID);

                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ITEMHEIGHT", "1");
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ITEMLENGTH", "1");
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ITEMWIDTH", "1");
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "MAXSKIDSHIGH", "1");
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SKIDWIDTH", "1");
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SKIDLENGTH", "1");
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SKID_HEIGHT", "1");

                conv12 = "1";

                if (!String.IsNullOrEmpty(labelUom))
                {
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "LABEL_UOM", labelUom);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "LABEL_UOM", labelUom);
                    uom1 = labelUom;
                }
                else if (stockUom == "EA")
                {
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "LABEL_UOM", "EA");
                    uom1 = "EA";
                }
                else
                {
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "LABEL_UOM", currImportConfig.defLabelUOM);
                    uom1 = currImportConfig.defLabelUOM;
                }

                SetItemReqFields(vmiCustId, ascItemId);  //added 02-02-15 (JXG) for BMS
            }

            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "LAST_IMPORT_DATE", DateTime.Now.ToString());
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "LAST_UPDATE", DateTime.Now.ToString());
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "LAST_UPDATE_USERID", "GATEWAY");

            //moved from out of !recEists 06-10-16 (JXG)
            //changed 06-10-16 (JXG)
            string serTrack = aData.SERIAL_TRACKED.Trim();
            if (!string.IsNullOrEmpty(serTrack))
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SER_NUM_FLAG", serTrack);

            //moved from out of !recEists 06-10-16 (JXG)
            string skidTrack = aData.SKID_TRACKED.Trim();
            if (!recExists && String.IsNullOrEmpty(skidTrack))  //added !recExists 06-10-16 (JXG)
                skidTrack = currImportConfig.defTrackBy;
            if (!string.IsNullOrEmpty(skidTrack) && !myClass.myParse.Globals.myDBUtils.ifRecExists("SELECT TOP 1 SKIDID FROM LOCITEMS WHERE ASCITEMID='" + ascItemId + "'"))
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "TRACKBYSKID", skidTrack);
            ////////////////////////////////////////////

            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BUYER", aData.BUYER.Trim());

            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CATID", aData.CATEGORY.Trim());

            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CAT2ID", aData.CATEGORY_2.Trim());

            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "DESCRIPTION", itemDesc.Trim());
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "DESCRIPTION2", itemDesc2.Trim());
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "STANDARDCOST", aData.STD_COST.ToString());

            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "STOCK_UOM", stockUom);
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "RETAILPRICE", aData.RETAIL_PRICE.ToString());

            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SCC14", aData.SCC14.Trim());

            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "SHELFLIFE", aData.SHELF_LIFE.ToString());

            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "HOLD_DATA", aData.AUTO_QC_REASON.Trim());

            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BUNDLE_SIZE", aData.BUNDLE_SIZE.Trim());

            ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "ORDERMINIMUM", aData.ORDERMINIMUM.ToString());
            ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "MRP_SAFETY_STOCK", aData.MRP_SAFETY_STOCK.ToString());

            //ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty( ref updstr,"THUMBNAIL_FILENAME", thumbnailFilename);  //taken out 10-02-17 (JXG)
            if (String.IsNullOrEmpty(buyUom))
                buyUom = stockUom;
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BUY_UOM", buyUom);

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

            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BOL_UNITWEIGHT", aData.PRODUCT_WEIGHT.ToString());

            if (String.IsNullOrEmpty(fcCode))
            {
                tmpStr = aData.FREIGHT_CLASS_CODE;
                if (!String.IsNullOrEmpty(tmpStr))
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHT_CLASS_CODE", tmpStr);
                else if (!String.IsNullOrEmpty(currImportConfig.defFreightClass))
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "FREIGHT_CLASS_CODE", currImportConfig.defFreightClass);
            }

            if (dualUnitItem)
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "DUAL_UNIT_ITEM", "T");

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
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "UPCCODE", aData.UPC_CODE.Trim());

            tmpStr = aData.ITEM_TYPE.Trim();
            if (string.IsNullOrEmpty(tmpStr) && !recExists)
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "PURORMFG", currImportConfig.defItemType);
            else
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "PURORMFG", tmpStr);

            if (String.IsNullOrEmpty(aData.UNIT1_UOM.Trim()) == false)

                uom1 = aData.UNIT1_UOM.Trim();
            else if (recExists && currImportConfig.doNotUpdateUOMValues)
                uom1 = string.Empty;
            else if (stockUom == "EA")
                uom1 = "EA";

            if (String.IsNullOrEmpty(aData.UNIT2_UOM.Trim()) == false)

                uom2 = aData.UNIT2_UOM.Trim();
            else if (recExists && currImportConfig.doNotUpdateUOMValues)
                uom2 = string.Empty;

            if (String.IsNullOrEmpty(aData.UNIT3_UOM.Trim()) == false)

                uom3 = aData.UNIT3_UOM.Trim();
            else if (recExists && currImportConfig.doNotUpdateUOMValues)
                uom3 = string.Empty;

            if (String.IsNullOrEmpty(aData.UNIT4_UOM.Trim()) == false)
                uom4 = aData.UNIT4_UOM.Trim();
            else if (recExists && currImportConfig.doNotUpdateUOMValues)
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
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "LABEL_UOM", labelUom);
                else if (stockUom == "EA")
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "LABEL_UOM", "EA");
            }

            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "UNIT_MEAS1", uom1);
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "UNIT_MEAS2", uom2);
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "UNIT_MEAS3", uom3);
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "UNIT_MEAS4", uom4);
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CONV_FACT_12", conv12);
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CONV_FACT_23", conv23);
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CONV_FACT_34", conv34);

            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "GTIN_UNIT1_ID", aData.GTIN_CODE_1.Trim());

            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "GTIN_UNIT2_ID", aData.GTIN_CODE_2.Trim());

            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "GTIN_UNIT3_ID", aData.GTIN_CODE_3.Trim());

            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "GTIN_UNIT4_ID", aData.GTIN_CODE_4.Trim());


            unitWidth = aData.UNITWIDTH;
            unitLength = aData.UNITLENGTH;
            unitHeight = aData.UNITHEIGHT;
            unitWeight = aData.UNITWEIGHT;


            if (unitWidth > 0)
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "UNITWIDTH", unitWidth.ToString());
            if (unitLength > 0)
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "UNITLENGTH", unitLength.ToString());
            if (unitHeight > 0)
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "UNITHEIGHT", unitHeight.ToString());
            if (unitWeight > 0)
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "UNITWEIGHT", unitWeight.ToString());

            itemCubic = aData.CUBIC_PER_EACH;
            if (itemCubic <= 0)
                itemCubic = unitWidth * unitLength * unitHeight;
            if (itemCubic > 0)
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ITEMCUBIC", itemCubic.ToString());

            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "TARE_WEIGHT", aData.TARE_WEIGHT.ToString());
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BULK_TARE_WEIGHT", aData.BULK_TARE_WEIGHT.ToString());

            if (!String.IsNullOrEmpty(aData.BILL_UOM))
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILL_UOM", aData.BILL_UOM);
            else
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "BILL_UOM", cwUom);

            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "COUNTRY_OF_ORIGIN", aData.COUNTRY_OF_ORIGIN);



            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "HAZMAT_FLAG", aData.HAZMAT_FLAG.Trim());
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "LOT_FLAG", aData.LOT_FLAG);
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "LOT_PROD_FLAG", aData.LOT_PROD_FLAG);
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "POST_LOT_TO_HOST_FLAG", aData.POST_LOT_TO_HOST_FLAG);
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "EXP_DATE_REQ_FLAG", aData.EXP_DATE_REQ_FLAG);
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "EXPIRE_DAYS", aData.EXPIRE_DAYS.ToString());

            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "MINIMUM", aData.MINIMUM.ToString());
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "RESTOCK_QTY", aData.RESTOCK_QTY.ToString());
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "MAXIMUM", aData.MAXIMUM.ToString());
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "LEADTIME", aData.LEADTIME.ToString());
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "INHOUSE_TIME", aData.INHOUSE_TIME.ToString());


            if (fUseVMI)  //(Service1.Parse.Globals.myConfig.iniGNVMI.boolValue)  //changed 08-11-15 (JXG) for Allen Dist
            {
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "VMI_CUSTID", vmiCustId);
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "VMI_VENDORID", vendorId);

                string vmiRespId = aData.VMI_RESPID;
                if (String.IsNullOrEmpty(vmiRespId))
                {
                    if (!recExists)  //added (!recExists) 10-12-16 (JXG) for Driscoll's
                    {
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "VMI_RESPID", vendorId);
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "VMI_RESPFLAG", "V");
                    }
                }
                else
                {
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "VMI_RESPID", vmiRespId);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "VMI_RESPFLAG", "C");
                }

                if (String.IsNullOrEmpty(vmiCustId))
                {
                    if (!recExists)  //added (!recExists) 10-12-16 (JXG) for Driscoll's
                    {
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "VMI_OWNERID", vendorId);
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "VMI_OWNERFLAG", "V");
                    }
                }
                else
                {
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "VMI_OWNERID", vmiCustId);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "VMI_OWNERFLAG", "C");
                }
            }
            else
            {
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "VENDOR1", vendorId);
            }

            SaveCustomFields(ref updstr, aData.CustomList, currImportConfig.GWTranslation);

            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ORGANIC_FLAG", aData.ORGANIC_FLAG);
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "PKG_MATERIAL_FLAG", aData.PKG_MATERIAL_FLAG);
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "MFG_ID", aData.MFG_ID);
            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "VENDOR1ITEMNUM", aData.VENDOR1ITEMNUM);

            if (!recExists)
                myClass.myParse.Globals.mydmupdate.InsertRecord("ITEMMSTR", updstr);
            else
                myClass.myParse.Globals.mydmupdate.UpdateFields("ITEMMSTR", updstr, "ASCITEMID='" + ascItemId + "'");
            myClass.ImportCustomData(funcType, "ITEMMSTR", "ASCITEMID='" + ascItemId + "'", itemId);  //added 10-17-17 (JXG)
            SaveExtData(ascItemId, aData.ExtDataList);
            myClass.myParse.Globals.dmMiscItem.CalcItemSubUOMConv(ascItemId);
            if (!recExists)
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
                ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "REF_NOTES", aData.REF_NOTES);
                if (recExists)
                    myClass.myParse.Globals.mydmupdate.UpdateFields("BOMHDR", updstr, "ASCITEMID='" + ascItemId + "'");
                else
                {
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "ASCITEMID", ascItemId);
                    myClass.myParse.Globals.mydmupdate.InsertRecord("BOMHDR", updstr);
                }
            }
            //////////////////////
            int seq = 1;
            foreach (var rec in aData.NotesList)
                ImportNotes.SaveNotes("I", ascItemId, rec.NOTE, false, 0, seq++, myClass.myParse.Globals);
            return (retval);
        }

        private static void SaveCustomFields(ref string updStr, List<ASCTracInterfaceModel.Model.ModelCustomData> CustomList, Dictionary<string, List<string>> TranslationList)
        {
            foreach (var rec in CustomList)
            {
                if (TranslationList.ContainsKey(rec.FieldName))
                {
                    var asclist = TranslationList[rec.FieldName];
                    foreach (var ascfield in asclist)
                    {

                        ascLibrary.ascStrUtils.ascAppendSetStr(ref updStr, ascfield, rec.Value);
                    }
                }
            }
        }
        private static void SaveExtData(string ascitemid, Dictionary<string, string> ExtDataList)
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

        private static void UpdateMissingItemQtyRecords()
        {
            string sql = "INSERT INTO ITEMQTY (ASCITEMID,QTYTOTAL,QTYALLOC,QTYONHOLD,QTYSUBITEMS,QTYREQUIRED,QTYSCHEDULED) " +
                "(SELECT ASCITEMID,0,0,0,0,0,0 FROM ITEMMSTR WHERE ASCITEMID NOT IN (SELECT ASCITEMID FROM ITEMQTY))";
            myClass.myParse.Globals.mydmupdate.AddToUpdate(sql);
        }

        private static bool SetItemReqFields(string aCustID, string aASCItemID)
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