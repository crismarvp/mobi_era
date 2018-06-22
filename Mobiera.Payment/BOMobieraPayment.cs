using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMCommon;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Otsol.Support.Json;
using System.Data;
using Otsol.Support.Data;
using System.Data.SqlClient;
using VM.Common;
using System.Configuration;
using System.Collections.Specialized;
using System.IO;


namespace Mobiera.Payment
{
    class BOMobieraPayment : VMBaseNew
    {
        public BOMobieraPayment(): base()
        {
            //publisher = new Publisher("dispatchQueue");
        }
        //string queueDestination = "dispatchQueue";
        string transactionUUID;
        string msisdn;
        string fullDateFormat;
        string shortDateFormat;
        JObject joAmountTransaction;
        string endUserId;
        Int64 mandate_id;
        Int64 identifier_id;
        string identifier_label;
        string identifier_class;
        int service_identifier_id;
        JObject joPaymentAmount ;
        JObject joChargingInformation ;
        string currency;
        string charging_description;
        JObject joCchargingMetaData;
        string channel;
        Double total_amount_charged;
        Double tax_amount;
        Double amount;
        Decimal price;
        string charging_event;
        Int64 server_reference_code;
        string resource_url;
        string transaction_operation_status;
        Int64 mobiera_debit_id;
        Int64 suscription_id;
        Int64 operator_subscription_id;
        Byte status;
        Int32 service_id;
        Int32 subservice_id;
        Int32 operator_service_id;
        Int64 mobiera_payment_notification_id = 0;
        Int64 mobiera_notification_id;
        DateTime created_on;
        String url;
        Int32 price_id;
        Int32 operator_id;
        Int32 created_by;
        Int32 updated_by;
        Int64 mobiera_out_id;
        Int64 mobiera_dispatch_id;
        Int32 operator_product_id;
        Int64 msg_content_id;
        String msg_text;
        String renew_context;
        String priority;
        Int32 product_id;
        Int32 package_id;
        string short_number;
        string high;
        string low;
        DateTime plannedOn;

        public override void InitValues()
        {
            base.InitValues();
            errorProcessName = "Mobiera.Payment";
            ApplicationName = "MOBIERA";
        }

        public void RegisterPayment(JObject data)
        {
            Exception innerEx = null;

            try
            {
         
                RegisterNotification(data);

                DataSet dsData1 = get_price_data();
                service_id = DataUtil.AsInteger(dsData1.Tables[0].Rows[0], "service_id", false);
                subservice_id = DataUtil.AsInteger(dsData1.Tables[0].Rows[0], "subservice_id", false);

                DataSet dsDispatch = get_dispatch_data();
          
                created_on = DataUtil.AsDateTime(dsDispatch.Tables[0].Rows[0], "created_on");
                short_number = DataUtil.AsString(dsDispatch.Tables[0].Rows[0], "short_number");
                msg_content_id = DataUtil.AsInteger(dsDispatch.Tables[0].Rows[0], "msg_content_id", false);
                msg_text = DataUtil.AsString(dsDispatch.Tables[0].Rows[0], "msg_text");
                package_id = DataUtil.AsInteger(dsDispatch.Tables[0].Rows[0], "package_id", false);
                suscription_id = DataUtil.AsInteger64(dsDispatch.Tables[0].Rows[0], "suscription_id", false);
                operator_product_id = DataUtil.AsInteger(dsDispatch.Tables[0].Rows[0], "operator_product_id", false);

                Int16 status = -1;
                Registerdebit(data);
                PriorityTime();
                RegisterMobiera_Dispatch(data, short_number);
                RegisterMobiera_Out(status, plannedOn);
            }
            catch (Exception ex)
            {
                innerEx = ex;

                if (errorCode == ERROR_CODE.OK)
                    ErrorAssignation(ERROR_CODE.GENR_Unidentified, ex.Message, ex.StackTrace, data.ToString());
            }
            finally
            {
                if (errorCode != ERROR_CODE.OK)
                {
                    Insert_Error_log();
                }
            }
        }

        public void RegisterNotification(JObject data)
        {
            string PROC_NOTIFY = "spu_insert_mobiera_payment_notification";
            fullDateFormat = "yyyy-MM-dd HH:mm:ss";
            shortDateFormat = "yyyy-MM-dd HH:mm";

            JObject joAmountTransaction = JsonUtil.AsJObject(data, "amountTransaction");
            msisdn = JsonUtil.AsString(joAmountTransaction, "endUserId", "");
            msisdn = msisdn.Substring(5, msisdn.Length - 5);
            endUserId = JsonUtil.AsString(joAmountTransaction, "endUserId", "");
            mandate_id = JsonUtil.AsInteger64(joAmountTransaction, "mandateId");
            identifier_id = JsonUtil.AsInteger64(joAmountTransaction, "identifierId");
            identifier_label = JsonUtil.AsString(joAmountTransaction, "identifierLabel", "");
            identifier_class = JsonUtil.AsString(joAmountTransaction, "identifierClass", "");
            service_identifier_id = JsonUtil.AsInteger(joAmountTransaction, "serviceIdentifierId");
            JObject joPaymentAmount = JsonUtil.AsJObject(joAmountTransaction, "paymentAmount");

                JObject joChargingInformation = JsonUtil.AsJObject(joPaymentAmount, "chargingInformation");
                    amount = JsonUtil.AsDouble(joChargingInformation, "amount");
                    currency = JsonUtil.AsString(joChargingInformation, "currency", "");
                    charging_description = JsonUtil.AsString(joChargingInformation, "description", "");

                total_amount_charged = JsonUtil.AsDouble(joPaymentAmount, "totalAmountCharged");
                JObject joCchargingMetaData = JsonUtil.AsJObject(joPaymentAmount, "chargingMetaData");
                   channel = JsonUtil.AsString(joCchargingMetaData, "channel", "");
                   tax_amount = JsonUtil.AsDouble(joCchargingMetaData, "taxAmount");
                   charging_event = JsonUtil.AsString(joCchargingMetaData, "chargingEvent", "");

            server_reference_code = JsonUtil.AsInteger64(joAmountTransaction, "serverReferenceCode");
            resource_url = JsonUtil.AsString(joAmountTransaction, "resourceURL", "");
            transaction_operation_status = JsonUtil.AsString(joAmountTransaction, "transactionOperationStatus", "");


            try
            {
                SqlParameter[] parameters = new SqlParameter[19];

                parameters[0] = new SqlParameter("@mobiera_payment_notification_id", DbType.Int64);
                parameters[0].Direction = ParameterDirection.Output;
                parameters[1] = new SqlParameter("@endUserId", DbType.String);
                parameters[1].Value = endUserId;
                parameters[2] = new SqlParameter("@msisdn", DbType.String);
                parameters[2].Value = msisdn;
                parameters[3] = new SqlParameter("@mandate_id", DbType.Int64);
                parameters[3].Value = mandate_id;
                parameters[4] = new SqlParameter("@identifier_id", DbType.Int64);
                parameters[4].Value = identifier_id;
                parameters[5] = new SqlParameter("@identifier_label", DbType.String);
                parameters[5].Value = identifier_label;
                parameters[6] = new SqlParameter("@identifier_class", DbType.String);
                parameters[6].Value = identifier_class;
                parameters[7] = new SqlParameter("@service_identifier_id", DbType.Int32);
                parameters[7].Value = service_identifier_id;
                parameters[8] = new SqlParameter("@amount", DbType.Decimal);
                parameters[8].Value = amount;
                parameters[9] = new SqlParameter("@currency", DbType.String);
                parameters[9].Value = currency;
                parameters[10] = new SqlParameter("@charging_description", DbType.String);
                parameters[10].Value = charging_description;
                parameters[11] = new SqlParameter("@total_amount_charged", DbType.Decimal);
                parameters[11].Value = total_amount_charged;
                parameters[12] = new SqlParameter("@channel", DbType.String);
                parameters[12].Value = channel;
                parameters[13] = new SqlParameter("@tax_amount", DbType.Decimal);
                parameters[13].Value = tax_amount;
                parameters[14] = new SqlParameter("@charging_event", DbType.String);
                parameters[14].Value = charging_event;
                parameters[15] = new SqlParameter("@server_reference_code", DbType.Int64);
                parameters[15].Value = server_reference_code;
                parameters[16] = new SqlParameter("@resource_url", DbType.String);
                parameters[16].Value = resource_url;
                parameters[17] = new SqlParameter("@transaction_operation_status", DbType.String);
                parameters[17].Value = transaction_operation_status;
                parameters[18] = new SqlParameter("@created_by", DbType.Int32);
                parameters[18].Value = 1;


                ExecuteNonQuery(InstanceName, CommandType.StoredProcedure, PROC_NOTIFY, parameters);
                mobiera_payment_notification_id = DataUtil.AsInteger64(parameters[0].Value, false);
            }
            catch (Exception ex)
            {

                ErrorAssignation(ERROR_CODE.GENR_ServerConnectionError, ex.Message, ex.StackTrace, data.ToString());
                ErrorAssignation(ERROR_CODE.GENR_NewIDNotGenerated, ex.Message, ex.StackTrace, url);
                
                throw ex;
            }

            finally
            {
                if (errorCode != ERROR_CODE.OK)
                    Insert_Error_log();
            }

            //return mobiera_payment_notification_id;
        }

        
        public DataSet get_price_data()
        {
            DataSet dsPrice;
            try
            {
                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@price", DbType.Decimal);
                parameters[0].Value = total_amount_charged;
                parameters[1] = new SqlParameter("@operator_id", DbType.Int32);
                parameters[1].Value = 5;

                dsPrice = GetDataSet(InstanceName, CommandType.StoredProcedure, "spu_get_price_data", parameters);
            }
            catch (Exception ex)
            {
                if (errorCode != ERROR_CODE.Mobiera_NoDataInTable)
                    ErrorAssignation(ERROR_CODE.GENR_ServerConnectionError, ex.Message, ex.StackTrace, dataMessage);
                throw ex;
            }

            finally
            {
                if (errorCode != ERROR_CODE.OK)
                    Insert_Error_log();
            }
            return dsPrice;
        }

        public DataSet get_dispatch_data()
        {
            DataSet dsDispatch;
            try
            {
                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0] = new SqlParameter("@operator_subscription_id", DbType.Int64);
                parameters[0].Value = mandate_id;

                dsDispatch = GetDataSet(InstanceName, CommandType.StoredProcedure, "spu_get_dispatch_data", parameters);
            }
            catch (Exception ex)
            {
                if (errorCode != ERROR_CODE.Mobiera_NoDataInTable)
                    ErrorAssignation(ERROR_CODE.GENR_ServerConnectionError, ex.Message, ex.StackTrace, dataMessage);
                throw ex;
            }
            finally
            {
                if (errorCode != ERROR_CODE.OK)
                    Insert_Error_log();
            }
            return dsDispatch;
        }

        public void Registerdebit(JObject data)
        {
            string spName = "spu_insert_mobiera_debit";

            Exception innerEx = null;

            try
            {
                mobiera_debit_id = GetNewId("mobiera_debit", 1);
            }
            catch (Exception ex)
            {
                ErrorAssignation(ERROR_CODE.GENR_NewIDNotGenerated, ex.Message, ex.StackTrace, url);
                throw ex;
            }
            try
            {

                SqlParameter[] parameters = new SqlParameter[9];
                parameters[0] = new SqlParameter("@mobiera_debit_id", DbType.Int64);
                parameters[0].Value = mobiera_debit_id;
                parameters[1] = new SqlParameter("@suscription_id", DbType.Int64);
                parameters[1].Value = suscription_id;
                parameters[2] = new SqlParameter("@status", DbType.Byte);
                parameters[2].Value = 1;
                parameters[3] = new SqlParameter("@service_id", DbType.Int32);
                parameters[3].Value = service_id;
                parameters[4] = new SqlParameter("@subservice_id", DbType.Int32);
                parameters[4].Value = subservice_id;
                parameters[5] = new SqlParameter("@mobiera_payment_notification_id", DbType.Int64);
                parameters[5].Value = mobiera_payment_notification_id;
                parameters[6] = new SqlParameter("@price", DbType.Decimal);
                parameters[6].Value = total_amount_charged;
                parameters[7] = new SqlParameter("@created_by", DbType.Int32);
                parameters[7].Value = 1;
                parameters[8] = new SqlParameter("@created_on", DbType.DateTime);
                parameters[8].Value = DateTime.UtcNow;

                ExecuteNonQuery(InstanceName, CommandType.StoredProcedure, spName, parameters);
            
            }
            catch (Exception ex)
            {
                ErrorAssignation(ERROR_CODE.GENR_ServerConnectionError, ex.Message, ex.StackTrace, data.ToString());
                throw ex;
            }

            finally
            {
                if (errorCode != ERROR_CODE.OK)
                    Insert_Error_log();
            }
        }

        public void RegisterMobiera_Out(Int16 status, DateTime plannedOn)
        {

            string PROC = "spu_insert_mobiera_out";

            try
            {
                mobiera_out_id = GetNewId("mobiera_out", 1);
            }
            catch (Exception ex)
            {
                ErrorAssignation(ERROR_CODE.GENR_NewIDNotGenerated, ex.Message, ex.StackTrace, "spu_insert_mobiera_out");
                throw ex;
            }

            try
            {
                SqlParameter[] parameters = new SqlParameter[6];
                parameters[0] = new SqlParameter("@mobiera_out_id", DbType.Int64);
                parameters[0].Value = mobiera_out_id;
                parameters[1] = new SqlParameter("@mobiera_dispatch_id", DbType.Int64);
                parameters[1].Value = mobiera_dispatch_id;
                parameters[2] = new SqlParameter("@planned_on", DbType.DateTime);
                parameters[2].Value = plannedOn;
                parameters[3] = new SqlParameter("@status", DbType.Int16);
                parameters[3].Value = status;
                parameters[4] = new SqlParameter("@created_by", DbType.Int32);
                parameters[4].Value = 1;
                parameters[5] = new SqlParameter("@created_on", DbType.DateTime);
                parameters[5].Value = DateTime.UtcNow;

                ExecuteNonQuery(InstanceName, CommandType.StoredProcedure, PROC, parameters);

            }
            catch (Exception ex)
            {
                ErrorAssignation(ERROR_CODE.GENR_ServerConnectionError, ex.Message, ex.StackTrace, "spu_insert_mobiera_out");
                throw ex;
            }
            finally
            {
                if (errorCode != ERROR_CODE.OK)
                    Insert_Error_log();
            }
        }

        public void PriorityTime ()
        {
           string diffGmt = ConfigurationManager.AppSettings.Get("diffGmt");
           int IdiffGmt = int.Parse(diffGmt);
           Double DdiffGmt = Double.Parse(diffGmt);
           string minHour = ConfigurationManager.AppSettings.Get("minHour");
           int IminHour = int.Parse(minHour);
           Double DminHour = Double.Parse(minHour);
           DateTime horaMinUtc = DateTime.UtcNow.Date.AddHours(DminHour);
           int IlocalTime = IminHour + IdiffGmt;
           string localTime = IlocalTime.ToString();
           Double DlocalTime = Double.Parse(localTime);
           DateTime horaMinLocal = DateTime.Now.Date.AddHours(DlocalTime);

        try
        {
             if (created_on.AddHours(DdiffGmt) > DateTime.Now.AddMinutes(-60))
             {
                   priority = "high";
                   plannedOn = DateTime.UtcNow;
             }
             else
             {
                   priority = "low";

                   if (created_on.AddHours(DdiffGmt) < horaMinLocal) 
                   {
                        plannedOn = horaMinUtc;
                   }
                   else if (created_on.AddHours(DdiffGmt) > horaMinLocal & 
                            created_on.AddHours(DdiffGmt) < horaMinLocal.AddHours(12))
                   {
                        plannedOn = DateTime.UtcNow;
                   }
                   else
                   {
                        DateTime today = DateTime.Now;
                        DateTime nextDay = today.AddDays(1);
                        TimeSpan ts = new TimeSpan((IminHour), 0, 0);
                        plannedOn = nextDay.Date + ts;
                   }
                }
            }
            catch(Exception ex)
            {
                ErrorAssignation(ERROR_CODE.GENR_Unidentified, ex.Message, ex.StackTrace, "spu_insert_mobiera_out");
                //Insert_Error_log(); 
                throw ex;
            }
         }

        public void RegisterMobiera_Dispatch(JObject data, string short_number)
        {
            string PROC = "spu_insert_mobiera_dispatch";

            try
            {
                mobiera_dispatch_id = GetNewId("mobiera_dispatch", 1);
            }
            catch (Exception ex)
            {
                ErrorAssignation(ERROR_CODE.GENR_NewIDNotGenerated, ex.Message, ex.StackTrace, "mobiera_dispatch_id");
                throw ex;
            }

            try
            {
                SqlParameter[] parameters = new SqlParameter[12];
                parameters[0] = new SqlParameter("@mobiera_dispatch_id", DbType.Int64);
                parameters[0].Value = mobiera_dispatch_id;
                parameters[1] = new SqlParameter("@suscription_id", DbType.Int64);
                parameters[1].Value = suscription_id;
                parameters[2] = new SqlParameter("@msisdn", DbType.String);
                parameters[2].Value = msisdn;
                parameters[3] = new SqlParameter("@operator_product_id", DbType.Int32);
                parameters[3].Value = operator_product_id;
                parameters[4] = new SqlParameter("@short_number", DbType.String);
                parameters[4].Value = short_number;
                parameters[5] = new SqlParameter("@msg_content_id", DbType.Int64);
                parameters[5].Value = msg_content_id;
                parameters[6] = new SqlParameter("@msg_text", DbType.String);
                parameters[6].Value = msg_text;
                parameters[7] = new SqlParameter("@priority", DbType.String);
                parameters[7].Value = priority;
                parameters[8] = new SqlParameter("@service_id", DbType.Int32);
                parameters[8].Value = service_id;
                parameters[9] = new SqlParameter("@subservice_id", DbType.Int32);
                parameters[9].Value = subservice_id;
                parameters[10] = new SqlParameter("@created_by", DbType.Int32);
                parameters[10].Value = 1;
                parameters[11] = new SqlParameter("@created_on", DbType.DateTime);
                parameters[11].Value = DateTime.UtcNow;

                ExecuteNonQuery(InstanceName, CommandType.StoredProcedure, PROC, parameters);
            }
            catch (Exception ex)
            {
                ErrorAssignation(ERROR_CODE.GENR_ServerConnectionError, ex.Message, ex.StackTrace, data.ToString());
                throw ex;
            }

            finally
            {
                if (errorCode != ERROR_CODE.OK)
                    Insert_Error_log();
            }
        }
    }
}



 