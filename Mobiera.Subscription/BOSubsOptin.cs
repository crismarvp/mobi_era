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
//using Mobiera.schemas;

namespace Mobiera
{
    class BOSubsOptin : VMBaseNew
    {
        public BOSubsOptin() : base()
        {
            //publisher = new Publisher("dispatchQueue");
            
        }
        string queueDestination = "dispatchQueue";
        string PROC_NOTIFY;
        string fullDateFormat;
        string shortDateFormat;
        string operator_service_id;
        string service_identifier_id;
        string transactionUUID;
        string msisdn;
        string endUserId;
        string identifier_id;
        string identifier_label;
        string identifier_class;
        string landing_id;
        string reference_code;
        string tracking_param;
        string periodicity;
        public long mobiera_notification_id;
        public long mobiera_subscription_notification_id;
        Int64 suscription_id;
        Int32 package_id;
        DateTime start_on;
        DateTime expire_on;
        string operator_subscription_id;
        string INACTIVE_REASON;
        string resource_url;

        DateTime created_on;
        String url;
      
        string subscription_operation_status;
        
   
        string is_new;  
        string active;  
        DateTime registered_on ;  
        DateTime last_attempt_date ;
        string entryChannel ;
        string subscriptionOperationStatus;
        DateTime unregistered_on;
        Int32 operator_id;


        public override void InitValues()
        {
            base.InitValues();
            errorProcessName = "Mobiera.SubsOptin";
            ApplicationName = "MOBIERA";
            //priority = ConfigurationManager.AppSettings["priority"];
            //renew_context = ConfigurationManager.AppSettings["context"];

        }


        public void RegisterOptin(JObject data)
        {
            Exception innerEx = null;
            try
            {
                //JObject subscription = JsonUtil.AsJObject(data, "subscription");
                //msisdn = JsonUtil.AsString(subscription, "endUserId", "");
                
                //1. Insertas notificación.
                RegisterSubscriptionNotification(data);

                //2. SI subscriptionOperationStatus == "subscribed" haces lo que está abajo.
               //  public void UpdateSubscription()
                //subscription_operation_status
                if (subscription_operation_status == "Subscribed")
                {
                    //Aquí obtienes el dataset desde la función get_subscription_data (la segunda tabla es operator_suscription)
                    DataSet dsData = get_subscription_data();
                    if (dsData.Tables[0].Rows.Count == 0)
                    {
                        //ErrorAssignation(ERROR_CODE.Entel_OperatorProduct_Not_Registered, "Entel Operator_product not registered", string.Format("WRONG INFORMATION [operator_product_id={0}]", JsonUtil.AsInteger(data, "productId")), data.ToString());
                        //throw new Exception();
                        ErrorAssignation(ERROR_CODE.Mobiera_OperatorProduct_Not_Registered, "Mobiera Operator_product not registered", string.Format("WRONG INFORMATION [operator_product_id={0}]", JsonUtil.AsInteger(data, "subscriptionId")), data.ToString());
                        
                        throw new Exception();
                    }
                    else
                    {
                        //En la primera tabla del dataset dsData está el campo package_id
                        //Lo mismo tienes que hacer para el suscription_id, sólo que está en la segunda tabla
                        // Primera tabla: Tables[0]    ... segunda tabla Tables[1]
                        

                        package_id = DataUtil.AsInteger(dsData.Tables[0].Rows[0], "package_id", false);
                        
                        //no puedes asignar un valor que no tienes.
                        //si es una nueva suscripción, eso quiere decir que aún no has registrado 
                        //en la tabla suscription, por lo tanto, no tienes el suscription_id
                        //la siguiente línea de código no sé que hace aquí. 

                        //suscription_id = DataUtil.AsInteger(dsData.Tables[1].Rows[0], "suscription_id", false);
                       
                        if (dsData.Tables[1].Rows.Count == 0)
                        {
                            //llamas a la rutina RegisterSuscription
                            RegisterSuscription();
                            //RegisterContentMessage(data);
                            //SendToQueue();
                        }
                    }

                }
                
                else
                {
                    UpdateSubscription();
                }
                //3 Else al if en el punto 2 (si subscriptionOperationStatus != "subscribed")
                // llamas a la rutina UpdateSubscription 
                
                //Send_Notification();


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
        public long RegisterSubscriptionNotification(JObject data)
        {
            PROC_NOTIFY = "spu_insert_mobiera_subscription_notification";
            fullDateFormat = "yyyy-MM-dd HH:mm:ss";
            shortDateFormat = "yyyy-MM-dd HH:mm";

            JObject subscription = JsonUtil.AsJObject(data, "subscription");
            operator_service_id = JsonUtil.AsString(subscription, "serviceId", "");
            service_identifier_id = JsonUtil.AsString(subscription, "serviceIdentifierId", "");
            msisdn = JsonUtil.AsString(subscription, "endUserId", "");
            msisdn = msisdn.Substring(5, msisdn.Length - 5);

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
            start_on = DataUtil.AsDateTime(str_start_on, fullDateFormat, DataUtil.AsDateTime(str_start_on, shortDateFormat, DateTime.MinValue, false), false);

            string str_expire_on = JsonUtil.AsString(subscription, "subscriptionValidUntilDateTime");
            str_expire_on = str_expire_on.Substring(0, 19);
            str_expire_on = str_expire_on.Replace("T", " ");
            expire_on = DataUtil.AsDateTime(str_expire_on, fullDateFormat, DataUtil.AsDateTime(str_expire_on, shortDateFormat, DateTime.MinValue, false), false);

            //DateTime expire_on = JsonUtil.AsDateTime(subscription, "subscriptionValidUntilDateTime");

            operator_subscription_id = JsonUtil.AsString(subscription, "subscriptionId", "");
            resource_url = JsonUtil.AsString(subscription, "resourceURL", "");

            string str_created_on = JsonUtil.AsString(subscription, "subscriptionStartDateTime");
            str_created_on = str_created_on.Substring(0, 19);
            str_created_on = str_created_on.Replace("T", " ");
            created_on = DataUtil.AsDateTime(str_created_on, fullDateFormat, DataUtil.AsDateTime(str_created_on, shortDateFormat, DateTime.MinValue, false), false);

            subscription_operation_status = JsonUtil.AsString(subscription, "subscriptionOperationStatus", "");


           
            try
            {
                SqlParameter[] parameters = new SqlParameter[18];

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
                ErrorAssignation(ERROR_CODE.Mobiera_NoDataInTable , ex.Message, ex.StackTrace, url);
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
                //en la tabla suscription el campo suscription_id no es identity, por lo que lo 
                //tienes que obtener con el siguiente código, obtener el máximo id de la tabla suscription
                //reservar 1 ID.
                suscription_id = GetNewId("suscription", 1);
             
            }
            catch (Exception ex)
            {
                ErrorAssignation(ERROR_CODE.GENR_NewIDNotGenerated, ex.Message, ex.StackTrace, url);
                throw ex;
            }

            try
            {
                SqlParameter[] parameters = new SqlParameter[7];

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
                ErrorAssignation(ERROR_CODE.GENR_ServerConnectionError, ex.Message, ex.StackTrace, url);
                throw ex;
            }
          

            //return vm_notification_id;
        }

         

        public DataSet SaveSubscription()
        {

            DataSet dsSubscription = new Mobiera.schemas.SubscriptionDS();
            DataTable dtSuscription = dsSubscription.Tables["suscription"];
            DataTable dtOperator_Subscription = dsSubscription.Tables["operator_subscription"];

            //SetSuscription(drRenew, expire_date, dtSuscription);
            //SetDavidSuscription(drRenew, dtDavid_Suscription);
            SetSubscription(dtSuscription, dtOperator_Subscription);
            RegisterCharges(dsSubscription);
            return dsSubscription;
        }



        //private void SetSuscription(DataRow drRenew, DateTime expire_date, DataTable dtSuscription)
        //{
        //    DataRow drSuscription = dtSuscription.NewRow();

        //    drSuscription["suscription_id"] = drRenew["suscription_id"];

        //    dtSuscription.Rows.Add(drSuscription);
        //    drSuscription.AcceptChanges();

        //    if (DataUtil.AsString(drRenew, "is_new") == "1")  // tabla suscription
        //    {
        //        if (DataUtil.AsString(drRenew, "suscription_status") == "1") // tabla david_suscription
        //        {
        //            drSuscription["is_new"] = "0";
        //        }
        //        else
        //        {
        //            DataSet dsCount;
        //            SqlParameter[] parameters = new SqlParameter[1];
        //            parameters[0] = new SqlParameter("@operator_subscription_id", DbType.Int64);
        //            parameters[0].Value = DataUtil.AsString(drRenew, "operator_subscription_id");
        //            dsCount = GetDataSet(InstanceName, CommandType.StoredProcedure, "SPU_GET_DAVID_DEBIT", parameters);

        //            if (DataUtil.AsInteger(dsCount.Tables[0].Rows[0], "CUENTA", false) > 1)
        //                drSuscription["is_new"] = "0";
        //        }
        //    }
        //    drSuscription["last_attempt_date"] = this.DateToUTC(expire_date);
        //}



        private void SetSubscription(DataTable dtSuscription, DataTable dtOperator_Subscription)
        {
            DataRow drSuscription = dtSuscription.NewRow();
            DataRow drOperator_S = dtOperator_Subscription.NewRow();


            //Suscription
            drSuscription["suscription_id"] = --suscription_id;
            drSuscription["package_id"] = package_id;
            drSuscription["msisdn"] = msisdn;
            //drSuscription["last_charge_date"] = DateTime.UtcNow;
            drSuscription["is_new"] = 1;
            drSuscription["active"] = 1;
            drSuscription["registered_on"] = DateTime.UtcNow;
            drSuscription["last_attempt_date"] = DateTime.UtcNow;

            //operator_subscription
            drOperator_S["operator_subscription_id"] = operator_subscription_id;
            drOperator_S["suscription_id"] = drSuscription["suscription_id"];
            drOperator_S["operator_id"] = 5;
            drOperator_S["start_on"] = start_on;
            drOperator_S["expire_on"] = expire_on;
            drOperator_S["created_on"] = DateTime.UtcNow;



            dtSuscription.Rows.Add(drSuscription);
            dtOperator_Subscription.Rows.Add(drOperator_S);

        }
        public void RegisterCharges(DataSet dsSubscription)
        {
            try
            {
                CreateDataRelation("suscription_operator_subscription", dsSubscription.Tables["suscription"].PrimaryKey, GetDataColumn(dsSubscription.Tables["operator_subscription"], "suscription_id"), Rule.None, Rule.Cascade);

                DataSet dsChanges = dsSubscription.GetChanges();
                ApplyChangesResult result = ApplyChanges(dsChanges);

            }
            catch (Exception ex)
            {
                ErrorAssignation(ERROR_CODE.GENR_ServerConnectionError, ex.Message, ex.StackTrace, "");
                //registered = false;
                throw ex;
            }
        }

        public override DBUpdater CreateDBUpdater(DataSet DS)
        {
            DataSet dsSchema = new Mobiera.schemas.SubscriptionDS();
            DBUpdater updater = new DBUpdater(dsSchema);
            return updater;
        }

        protected override void SetDataTableMappings(DataSet data, DBUpdater dbUpdater)
        {
            string[] auditFields = new string[] { "created_by", "created_on", "updated_by", "updated_on" };

            if (data.Tables.Contains("suscription"))
                dbUpdater.SetDataTableMapping(data.Tables["suscription"], "suscription", ConcurrencyMode.None, null, auditFields, null);

            if (data.Tables.Contains("operator_subscription"))
                dbUpdater.SetDataTableMapping(data.Tables["operator_subscription"], "operator_subscription", ConcurrencyMode.None, null, auditFields, null);

        }
    



    }

}
