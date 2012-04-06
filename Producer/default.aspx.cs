using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Configuration;
using RabbitMQ.Client;
using Newtonsoft.Json;
using System.Text;
using System.IO;

namespace AsyncRabbitWeb
{
    public partial class _default : System.Web.UI.Page
    {
        private Producer producer = new Producer("uhuruExchange", ExchangeType.Fanout);

        protected void Page_Load(object sender, EventArgs e)
        {
            producer.ConnectToRabbitMQ();
            lblExgName.Text = "Exchange Name :  " + producer.Exchange;
            lblExgType.Text = "Exchange Name :  " + producer.ExchangeTypeName;
        }

        protected void Page_UnLoad(object sender, EventArgs e)
        {
            producer.Dispose();
        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            JsonWriter jsonWriter = new JsonTextWriter(sw);

            jsonWriter.Formatting = Formatting.Indented;
            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("name");
            jsonWriter.WriteValue(txtName.Text);

            jsonWriter.WritePropertyName("description");
            jsonWriter.WriteValue(txtDesc.Text);

            jsonWriter.WritePropertyName("priority");
            jsonWriter.WriteValue((dpdPri.Text).ToUpper());

            jsonWriter.WriteEndObject();
            string output = sw.ToString();
            jsonWriter.Close();
            sw.Close();

            //string[] args = new string[3];
            //args[0] = txtName.Text;
            //args[1] = txtDesc.Text;
            //args[2] = txtPri.Text;
            //string message = string.Join(" ", args);
            byte[] body = System.Text.Encoding.UTF8.GetBytes(output);
            producer.SendMessage(body);
        }
    }
}