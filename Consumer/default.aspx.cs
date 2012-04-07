using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Configuration;
using RabbitMQ.Client;
using System.Data.SqlClient;

namespace RabbitConsumer
{
    public partial class _default : System.Web.UI.Page
    {
        
        protected string connString = WebConfigurationManager.AppSettings["dbConnectionString"];
        protected SqlConnection conn;
        private Consumer consumer = new Consumer("uhuruExchange", ExchangeType.Fanout);

        protected void Page_Load(object sender, EventArgs e)
        {
            conn = new SqlConnection(connString);
            consumer.ConnectToRabbitMQ();
            conn.Open();

            lblExgName.Text = "Exchange Name :  " + consumer.Exchange;
            lblExgType.Text = "Exchange Type :  " + consumer.ExchangeTypeName;
            lblQName.Text = "Queue Name :  " + consumer.QueueName;
        }

        protected void Page_UnLoad(object sender, EventArgs e)
        {
            consumer.Dispose();
            conn.Close();
        }

        protected void btnRec_Click(object sender, EventArgs e)
        {
            //consumer.StartConsuming();
            consumer.ReceiveMessage();
            try
            {
                SqlCommand command = conn.CreateCommand();
                command.CommandText = "select * from dbo.tblRabbitQueueMsg";
                SqlDataReader reader = command.ExecuteReader();
                grdView.DataSource = reader;
                grdView.DataBind();
            }
            catch (Exception ex)
            {
                Response.Write("oops, something went terribly wrong:" + ex.ToString());
            }
        }

        protected void btnTrunc_Click(object sender, EventArgs e)
        {
            try
            {
                SqlCommand command = conn.CreateCommand();
                command.CommandText = "Truncate Table dbo.tblRabbitQueueMsg";
                SqlDataReader reader = command.ExecuteReader();
                grdView.DataSource = reader;
                grdView.DataBind();
            }
            catch (Exception ex)
            {
                Response.Write("oops, something went terribly wrong:" + ex.ToString());
            }
        }
    }
}