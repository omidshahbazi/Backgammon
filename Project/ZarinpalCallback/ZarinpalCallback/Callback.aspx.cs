using System;

namespace ZarinpalCallback
{
	public partial class Callback : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			string result = Request.QueryString["result"];

			if (string.IsNullOrEmpty(result))
				return;

			Response.Redirect(result);
		}
	}
}