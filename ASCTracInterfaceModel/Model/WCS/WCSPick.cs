using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceModel.Model.WCS
{
    public class WCSPick
    {

        public string SITE_ID { get; set; }

        public double ASSIGNMENT_NUMBER { get; set; }

        public string TYPE_OF_PICK { get; set; }

        public string ORDERTYPE { get; set; }

        public string ORDERNUMBER { get; set; }

        public double PRIORITY { get; set; }

        public string DELIVERY_LOCATION { get; set; }

        public DateTime DELIVERY_DATE { get; set; }

        public string ROUTE { get; set; }

        public double GOAL_TIME { get; set; }

        public double PICK_SEQUENCE_NO { get; set; }

        public string ITEMID { get; set; }

        public string ZONEID { get; set; }

        public string AISLE { get; set; }

        public string SLOT { get; set; }

        public string LOCATIONID { get; set; }

        public string LOCATION_IDENTIFIER { get; set; }

        public string LOTID { get; set; }

        public string SKIDID { get; set; }

        public string FULL_CASE { get; set; }

        public string PCE_TYPE { get; set; }

        public double NUM_EACHES_CASE { get; set; }

        public double UNIT_WIDTH { get; set; }

        public double UNIT_LENGTH { get; set; }

        public double UNIT_HEIGHT { get; set; }

        public double UNIT_WEIGHT { get; set; }

        public double WCS_WIDTH { get; set; }

        public double WCS_LENGTH { get; set; }

        public double WCS_HEIGHT { get; set; }

        public double WCS_WEIGHT { get; set; }

        public double QTY_TO_PICK { get; set; }

        public double QTY_PICKED { get; set; }

        public string CONTAINER_ID { get; set; }

       // public string ORDER_CANCEL_FLAG { get; set; }

       // public string ORDER_CANCEL_CODE { get; set; }

        public string TRIGGER_A_REPLEN { get; set; }

        public DateTime DATETIME_PICKED { get; set; }

        public string USERID { get; set; }

        public string INTERFACE_RACK { get; set; }

        public string INTERFACE_LEVEL { get; set; }

        public string INTERFACE_POSITION { get; set; }

        public double COORD_X { get; set; }

        public double COORD_Y { get; set; }

        public string SER_NUM { get; set; }

        public string ITEM_DESCRIPTION { get; set; }

        //public double EXPIRE_DAYS { get; set; }

        public DateTime EXPDATE_FIRST { get; set; }

        public string PICK_UNIT { get; set; }

        public string ITEM_UPC { get; set; }

        public string DIRECTED_CONTAINER_ID { get; set; }

    }

}