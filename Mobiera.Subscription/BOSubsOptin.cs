using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMCommon;
using VM.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Otsol.Support.Json;
using System.Data;
using Otsol.Support.Data;
using Otsol.DataAccess;
using System.Data.SqlClient;
using System.Configuration;


namespace Mobiera
{
    class BOSubsOptin : VMBaseNew
    {
        public BOSubsOptin() : base()
        {
            //publisher = new Publisher("dispatchQueue");
            
        }
        //string queueDestination = "dispatchQueue";
        string PROC_NOTIFY;
        string fullDateFormat;
        string shortDateFormat;
        string operator_service_id;
        string service_identifier_id;
        string msisdn;
        string endUserId;
        string identifier_id;
        string identifier_label;
        string identifier_class;
        string landing_id;
        string reference_code;
        string tracking_param;
        string periodicity;
        public long mobiera_subscription_notification_id;
        Int64 suscription_id;
        Int32 package_id;
        DateTime start_on;
        DateTime expire_on;
        string operator_subscription_id;
        string resource_url;
        DateTime created_on;
        string subscription_operation_status;
        string url;
        string campaign_id;

        public override void InitValues()
        {
            base.InitValues();
            errorProcessName = "Mobiera.SubsOptin";
            ApplicationName = "MOBIERA";
        }

        public void RegisterOptin(JObject data)
        {
            Exception innerEx = null;
            try
            {
                RegisterSubscriptionNotification(data);

                if (subscription_operation_status.ToUpper() == "SUBSCRIBED")
                {
                    DataSet dsData = get_subscription_data();
           
                    if (dsData.Tables[0].Rows.Count == 0)
                    {
                        ErrorAssignation(ERROR_CODE.Mobiera_OperatorProduct_Not_Registered,
                        "Mobiera Operator_product not registered",
                        string.Format("WRONG INFORMATION [operator_product_id={0}]",
                        JsonUtil.AsInteger(data, "subscriptionId")), data.ToString());
                        
                        throw new Exception();
                    }

                    else
                    {
                        package_id = DataUtil.AsInteger(dsData.Tables[0].Rows[0], "package_id", false);

                        
                        if (dsData.Tables[1].Rows.Count == 0)
                        {
                            RegisterSuscription();
                        }
                    }
                }

                else
                {
                    UpdateSubscription();
                }
            }
            catch (Exception ex)
            {
                innerEx = ex;

                if (errorCode == ERROR_CODE.OK)
                    //ErrorAssignation(ERROR_CODE.GENR_Unidentified, ex.Message, ex.StackTrace, data.ToString());
                    ErrorAssignation(ERROR_CODE.Mobiera_NoDataInTable, " Faltan datos en la tabla  ", ex.StackTrace,data.ToString());

            }
            finally
            {
                if (errorCode != ERROR_CODE.OK)
                {
                    Insert_Error_log();

                }
            }
        }

        public long RegisterSubscriptionNotification(JObject data)
        {
            PROC_NOTIFY = "spu_insert_mobiera_subscription_notification";
            fullDateFormat = "yyyy-MM-dd HH:mm:ss";
            shortDateFormat = "yyyy-MM-dd HH:mm";

            JObject subscription = JsonUtil.AsJObject(data, "subscription");
            operator_service_id = JsonUtil.AsString(subscription, "serviceId", "");
            service_identifier_id = JsonUtil.AsString(subscription, "serviceIdentifierId", "");
            msisdn = JsonUtil.AsString(subscription, "endUserId", "");
            msisdn = msisdn.Substring(8, msisdn.Length - 8);

            endUserId = JsonUtil.AsString(subscription, "endUserId", "");
            identifier_id = JsonUtil.AsString(subscription, "identifierId", "");
            identifier_label = JsonUtil.AsString(subscription, "identifierLabel", "");
            identifier_class = JsonUtil.AsString(subscription, "identifierClass", "");
            
            landing_id = JsonUtil.AsString(subscription, "landingId", "");
            reference_code = JsonUtil.AsString(subscription, "referenceCode", "");
            tracking_param = JsonUtil.AsString(subscription, "trackingParam", "");
            periodicity = JsonUtil.AsString(subscription, "subscriptionPeriodicity", "");

            string str_start_on = JsonUtil.AsString(subscription, "subscriptionStartDateTime");
            str_start_on = str_start_on.Substring(0, 19);
            str_start_on = str_start_on.Replace("T", " ");
            start_on = DataUtil.AsDateTime(str_start_on, fullDateFormat, 
            DataUtil.AsDateTime(str_start_on, shortDateFormat, DateTime.MinValue, false), false);

            string str_expire_on = JsonUtil.AsString(subscription, "subscriptionValidUntilDateTime");
            str_expire_on = str_expire_on.Substring(0, 19);
            str_expire_on = str_expire_on.Replace("T", " ");
            expire_on = DataUtil.AsDateTime(str_expire_on, fullDateFormat, 
            DataUtil.AsDateTime(str_expire_on, shortDateFormat, DateTime.MinValue, false), false);

            operator_subscription_id = JsonUtil.AsString(subscription, "subscriptionId", "");
            resource_url = JsonUtil.AsString(subscription, "resourceURL", "");

            string str_created_on = JsonUtil.AsString(subscription, "subscriptionStartDateTime");
            str_created_on = str_created_on.Substring(0, 19);
            str_created_on = str_created_on.Replace("T", " ");
            created_on = DataUtil.AsDateTime(str_created_on, fullDateFormat, 
            DataUtil.AsDateTime(str_created_on, shortDateFormat, DateTime.MinValue, false), false);

            subscription_operation_status = JsonUtil.AsString(subscription, "subscriptionOperationStatus", "");

            campaign_id = JsonUtil.AsString(subscription, "campaignId", "");


            try
            {
                SqlParameter[] parameters = new SqlParameter[19];

                parameters[0] = new SqlParameter("@mobiera_subscription_notification_id", DbType.Int64);
                parameters[0].Value = mobiera_subscription_notification_id;
                parameters[1] = new SqlParameter("@operator_service_id", DbType.String);
                parameters[1].Value = operator_service_id;
                parameters[2] = new SqlParameter("@service_identifier_id", DbType.String);
                parameters[2].Value = service_identifier_id;
                parameters[3] = new SqlParameter("@msisdn", DbType.String);
                parameters[3].Value = msisdn;
                parameters[4] = new SqlParameter("@endUserId", DbType.String);
                parameters[4].Value = endUserId;
                parameters[5] = new SqlParameter("@identifier_id", DbType.String);
                parameters[5].Value = identifier_id;
                parameters[6] = new SqlParameter("@identifier_label", DbType.String);
                parameters[6].Value = identifier_label;
                parameters[7] = new SqlParameter("@identifier_class", DbType.String);
                parameters[7].Value = identifier_class;
                parameters[8] = new SqlParameter("@landing_id", DbType.String);
                parameters[8].Value = landing_id;
                parameters[9] = new SqlParameter("@reference_code", DbType.String);
                parameters[9].Value = reference_code;
                parameters[10] = new SqlParameter("@tracking_param", DbType.String);
                parameters[10].Value = tracking_param;
                parameters[11] = new SqlParameter("@periodicity", DbType.String);
                parameters[11].Value = periodicity;
                parameters[12] = new SqlParameter("@start_on", DbType.DateTime);
                parameters[12].Value = start_on;
                parameters[13] = new SqlParameter("@expire_on", DbType.DateTime);
                parameters[13].Value = expire_on;
                parameters[14] = new SqlParameter("@operator_subscription_id", DbType.Int64);
                parameters[14].Value = operator_subscription_id;
                parameters[15] = new SqlParameter("@resource_url", DbType.String);
                parameters[15].Value = resource_url;
                parameters[16] = new SqlParameter("@created_by", DbType.Int32);
                parameters[16].Value = 1;
                parameters[17] = new SqlParameter("@created_on", DbType.DateTime);
                parameters[17].Value = created_on;
                parameters[18] = new SqlParameter("@campaign_id", DbType.String);
                parameters[18].Value = campaign_id;

                ExecuteNonQuery(InstanceName, CommandType.StoredProcedure, PROC_NOTIFY, parameters);
            }
            catch (Exception ex)
            {
                ErrorAssignation(ERROR_CODE.GENR_ServerConnectionError, ex.Message, ex.StackTrace, data.ToString());

                throw ex;
            }
            return mobiera_subscription_notification_id;
        }
        
        public void UpdateSubscription()
        {
            string spName3 = "spu_update_subscription_status";

            try
            {
                SqlParameter[] parameters = new SqlParameter[5];

                parameters[0] = new SqlParameter("@active", DbType.String);
                parameters[0].Value = "0";
                parameters[1] = new SqlParameter("@unregistered_on", DbType.DateTime);
                parameters[1].Value = DateTime.UtcNow;
                parameters[2] = new SqlParameter("@INACTIVE_REASON", DbType.String);
                parameters[2].Value = "U";
                parameters[3] = new SqlParameter("@operator_subscription_id", DbType.Int64);
                parameters[3].Value = operator_subscription_id;
                parameters[4] = new SqlParameter("@operator_id", DbType.Int32);
                parameters[4].Value = 5;

                ExecuteNonQuery(InstanceName, CommandType.StoredProcedure, spName3, parameters);

            }
            catch (Exception ex)
            {
                ErrorAssignation(ERROR_CODE.Mobiera_NoDataInTable, ex.Message, ex.StackTrace, dataMessage);
                throw ex;
            }
        }

        public DataSet get_subscription_data()
        {
            DataSet dsOptIn;

                SqlParameter[] parameters = new SqlParameter[4];

                parameters[0] = new SqlParameter("@operator_service_id", DbType.Int32);
                parameters[0].Value = operator_service_id;
                parameters[1] = new SqlParameter("@service_identifier_id", DbType.Int32);
                parameters[1].Value = service_identifier_id;
                parameters[2] = new SqlParameter("@operator_subscription_id", DbType.Int64);
                parameters[2].Value = operator_subscription_id;
                parameters[3] = new SqlParameter("@operator_id", DbType.Int32);
                parameters[3].Value = 5;
     
                dsOptIn = GetDataSet(InstanceName, CommandType.StoredProcedure, "spu_get_data_mobiera", parameters);
    
            return dsOptIn;
        }

        public void RegisterSuscription()
        {
            string spName = "spu_insert_suscription";
            string spName2 = "spu_insert_operator_subscription";
            
            try
            {
                 suscription_id = GetNewId("suscription", 1);
            }
            catch (Exception ex)
            {
                ErrorAssignation(ERROR_CODE.GENR_NewIDNotGenerated, ex.Message, ex.StackTrace, dataMessage);
                throw ex;
            }

            try
            {
                SqlParameter[] parameters = new SqlParameter[8];

                parameters[0] = new SqlParameter("@suscription_id", DbType.Int64);
                parameters[0].Value = suscription_id;
                parameters[1] = new SqlParameter("@package_id", DbType.Int32);
                parameters[1].Value = package_id;
                parameters[2] = new SqlParameter("@msisdn", DbType.String);
                parameters[2].Value = msisdn;
                parameters[3] = new SqlParameter("@is_new", DbType.String);
                parameters[3].Value = "1";
                parameters[4] = new SqlParameter("@active", DbType.String);
                parameters[4].Value = "1";
                parameters[5] = new SqlParameter("@registered_on", DbType.DateTime);
                parameters[5].Value = DateTime.UtcNow;
                parameters[6] = new SqlParameter("@last_attempt_date", DbType.DateTime);
                parameters[6].Value = DateTime.UtcNow;
                parameters[7] = new SqlParameter("@entryChannel", DbType.String);
                parameters[7].Value = "";

                ExecuteNonQuery(InstanceName, CommandType.StoredProcedure, spName, parameters);

                SqlParameter[] parameters2 = new SqlParameter[6];

                parameters2[0] = new SqlParameter("@operator_subscription_id", DbType.Int64);
                parameters2[0].Value = operator_subscription_id;
                parameters2[1] = new SqlParameter("@suscription_id", DbType.Int64);
                parameters2[1].Value = suscription_id;
                parameters2[2] = new SqlParameter("@operator_id", DbType.Int32);
                parameters2[2].Value = 5;
                parameters2[3] = new SqlParameter("@start_on", DbType.DateTime);
                parameters2[3].Value = start_on;
                parameters2[4] = new SqlParameter("@expire_on", DbType.DateTime);
                parameters2[4].Value = expire_on;
                parameters2[5] = new SqlParameter("@created_on", DbType.DateTime);
                parameters2[5].Value = DateTime.UtcNow;

                ExecuteNonQuery(InstanceName, CommandType.StoredProcedure, spName2, parameters2);
            }

            catch (Exception ex)
            {
                ErrorAssignation(ERROR_CODE.GENR_ServerConnectionError, ex.Message, ex.StackTrace, dataMessage);
                throw ex;
            }
        }
    }
}
