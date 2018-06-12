
using VM.Common;
using VMCommon;
using System;
using System.Data;
using Otsol.Support.Data;
using Newtonsoft.Json.Linq;
using System.Configuration;
using Apache.NMS;

namespace Mobiera.verifyContent
{
    class BOVerifyContent : VMBaseNew
    {
        string PROC;
                  

        public override void InitValues()
        {
            base.InitValues();
            errorProcessName = "Entel.VerifyContent";
            ApplicationName = "MOBIERA";

        }

        public void verifyContentToSend()
       
        {
            try
            {

                string PROC = "spu_insert_incoming";
                ExecuteNonQuery(InstanceName, CommandType.StoredProcedure, PROC);
            }
            catch (Exception ex)
            {
                ErrorAssignation(ERROR_CODE.Mobiera_NoDataInTable, ex.Message, ex.StackTrace);
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
