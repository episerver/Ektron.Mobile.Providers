using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using SharpZipLib2.SharpZipLib.GZip;
using Ektron.Cms;
using SharpZipLib2.SharpZipLib.Zip;
using Ektron.Cms.Mobile;

public partial class Workarea_devicemodelselection : Ektron.Cms.Workarea.Page
{
    private bool use51
    {
        get
        {
            return !string.IsNullOrEmpty(ConfigurationManager.AppSettings["51DegreesLocation"]);
        }
    }

    protected ContentAPI _ContentApi;
    protected Ektron.Cms.Common.EkMessageHelper m_MsgHelper;
    public class ListItemComparer : EqualityComparer<DeviceInfo>
    {
        public override bool Equals(DeviceInfo x, DeviceInfo y)
        {
            return x.ModelName.Equals(y.ModelName);
        }

        public override int GetHashCode(DeviceInfo obj)
        {
            return obj.ModelName.GetHashCode();
        }
    }
    public class DeviceInfo
    {
        public string ModelName {get; set;}
        public string UserAgent { get; set;}
    }

    protected void Page_Init(object sender, System.EventArgs e)
    {
        _ContentApi = new ContentAPI();
        m_MsgHelper = _ContentApi.EkMsgRef;
        this.RegisterCSS();
        this.RegisterJS();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
	   Utilities.ValidateUserLogin();
       if(!this.Page.IsPostBack)
       {
           BindGrid(string.Empty,0);
       }
       setLocalizedString();
    }


    public void BindGrid(string searchText,int index)
    {
        if (use51)
        {
            var mobilelib = ObjectFactory.Get<Ektron.Cms.Mobile.IWURFLProvider>();

            var provider = mobilelib.GetType();

            if (Request.QueryString["os"] == "false")
            {
                //search for a model
                var searchForDevices = provider.GetMethod("SearchForDevices", (System.Reflection.BindingFlags)0xFFFFFFF);
                var retval = (List<EkDeviceInfo>)searchForDevices.Invoke(mobilelib, new object[] { searchText.ToLower() });
                DeviceModelView.DataSource = retval
                    .Select(t => new DeviceInfo { ModelName = t.DeviceModel, UserAgent = t.DeviceUserAgent })
                    .GroupBy(t => t.ModelName)
                    .Select(t => t.First())
                    .ToList();
            }
            else
            {
                //search for an os
                var searchForDevices = provider.GetMethod("GetAvaliableOs", (System.Reflection.BindingFlags)0xFFFFFFF);
                var retval = (List<string>)searchForDevices.Invoke(mobilelib, null);
                if (searchText != "") retval = retval.Where(t => t.ToLower().Contains(searchText.ToLower())).ToList();

                DeviceModelView.DataSource = retval.Select(t => new DeviceInfo { ModelName = t }).ToList();
            }

        }
        else
        {
            XDocument xmDoc = (Context.Cache["EktronWurflXML"] != null) ? (XDocument)Context.Cache["EktronWurflXML"] : null;
            if (xmDoc == null)
            {
                string filePath = Server.MapPath("~/App_Data/wurfl.zip");
                var importFile = new FileInfo(filePath);
                using (var zipStream = new ZipInputStream(importFile.OpenRead()))
                {
                    ZipEntry entry;
                    while ((entry = zipStream.GetNextEntry()) != null)
                    {
                        if (entry.Name.ToLower().EndsWith(".xml") && !entry.Name.ToLower().StartsWith("__macosx"))
                        {
                            xmDoc = XDocument.Load(zipStream);
                        }
                    }
                }
                Context.Cache.Add("EktronWurflXML", xmDoc, new System.Web.Caching.CacheDependency(filePath), System.DateTime.MaxValue, TimeSpan.Zero, System.Web.Caching.CacheItemPriority.Default, null);
            }

            if (Request.QueryString["os"] == "false")
            {
                if (searchText != "")
                {

                    var modelList = (from devices in xmDoc.Element("wurfl").Element("devices").Elements("device")
                                     from groups in devices.Elements("group")
                                     where (string)groups.Attribute("id") == "product_info"
                                     from cap1 in groups.Elements("capability")
                                     where (string)cap1.Attribute("name") == "model_name" && cap1.Attribute("value").Value.ToLower().Contains(searchText.ToLower())
                                     orderby cap1.Attribute("value").Value

                                     select new DeviceInfo
                                     {
                                         ModelName = cap1.Attribute("value").Value,
                                         UserAgent = devices.Attribute("user_agent").Value
                                     })
                                     .Distinct(new ListItemComparer())
                                     .ToList();
                    DeviceModelView.DataSource = modelList;


                }
                else
                {
                    var modelList = (from devices in xmDoc.Element("wurfl").Element("devices").Elements("device")
                                     from groups in devices.Elements("group")
                                     where (string)groups.Attribute("id") == "product_info"
                                     from cap1 in groups.Elements("capability")
                                     where (string)cap1.Attribute("name") == "model_name" && cap1.Attribute("value").Value != ""
                                     orderby cap1.Attribute("value").Value

                                     select new DeviceInfo
                                     {
                                         ModelName = cap1.Attribute("value").Value,
                                         UserAgent = devices.Attribute("user_agent").Value
                                     })
                                         .Distinct(new ListItemComparer())
                                         .ToList();
                    DeviceModelView.DataSource = modelList;
                }
            }
            else
            {
                if (searchText != "")
                {

                    var modelList = (from devices in xmDoc.Element("wurfl").Element("devices").Elements("device")
                                     from groups in devices.Elements("group")
                                     where (string)groups.Attribute("id") == "product_info"
                                     from cap1 in groups.Elements("capability")
                                     where (string)cap1.Attribute("name") == "device_os" && cap1.Attribute("value").Value.ToLower().Contains(searchText.ToLower())
                                     orderby cap1.Attribute("value").Value

                                     select new DeviceInfo
                                     {
                                         ModelName = cap1.Attribute("value").Value,
                                         UserAgent = devices.Attribute("user_agent").Value
                                     })
                                     .Distinct(new ListItemComparer())
                                     .ToList();
                    DeviceModelView.DataSource = modelList;


                }
                else
                {
                    var modelList = (from devices in xmDoc.Element("wurfl").Element("devices").Elements("device")
                                     from groups in devices.Elements("group")
                                     where (string)groups.Attribute("id") == "product_info"
                                     from cap1 in groups.Elements("capability")
                                     where (string)cap1.Attribute("name") == "device_os" && cap1.Attribute("value").Value != ""
                                     orderby cap1.Attribute("value").Value

                                     select new DeviceInfo
                                     {
                                         ModelName = cap1.Attribute("value").Value,
                                         UserAgent = devices.Attribute("user_agent").Value
                                     })
                                         .Distinct(new ListItemComparer())
                                         .ToList();
                    DeviceModelView.DataSource = modelList;
                }
            }
        }

        DeviceModelView.Columns[0].HeaderText = m_MsgHelper.GetMessage("lbl Device Model");
        DeviceModelView.PageIndex = index;
        DeviceModelView.DataBind();
    }


    protected void DeviceModelView_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
      BindGrid(txtSearch.Text, e.NewPageIndex);
    }

    protected void DeviceModelView_RowDataBound(object sender, GridViewRowEventArgs e)
    {
         if (e.Row.RowType == DataControlRowType.DataRow)
         {
             string modelName = DataBinder.Eval(e.Row.DataItem, "modelName").ToString();
             if (modelName != "")
                 e.Row.Attributes.Add("onclick", "SetDeviceModel('" + modelName.Replace("'","") + "');");

             e.Row.Attributes.Add("OnMouseOver", "this.style.backgroundColor = '#f7eda1';");
             if (e.Row.RowIndex % 2 == 0)
             {
                 e.Row.Attributes.Add("OnMouseOut", "this.style.backgroundColor = '#FFFFFF';");
             }
             else
             {
                 e.Row.Attributes.Add("OnMouseOut", "this.style.backgroundColor = '#e7f0f7';");
             }
         }  
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        BindGrid(txtSearch.Text,0);
    }
    protected void btnClear_Click(object sender, EventArgs e)
    {
        txtSearch.Text = string.Empty;
        BindGrid(string.Empty,0);

    }

    private void setLocalizedString()
    {
        txtSearch.ToolTip = m_MsgHelper.GetMessage("tooltip:device model search");

        btnSearch.Text = m_MsgHelper.GetMessage("generic search");
        btnSearch.ToolTip = m_MsgHelper.GetMessage("tooltip:search device");
        
        btnClear.Text = m_MsgHelper.GetMessage("lbl clear");
        btnClear.ToolTip = m_MsgHelper.GetMessage("tooltip:clear device search");

        DeviceModelView.Columns[0].HeaderText = m_MsgHelper.GetMessage("lbl Device Model");
    }

    #region CSS, JS

    private void RegisterCSS()
    {
        Ektron.Cms.API.Css.RegisterCss(this, Ektron.Cms.API.Css.ManagedStyleSheet.EktronWorkareaCss);
        Ektron.Cms.API.Css.RegisterCss(this, Ektron.Cms.API.Css.ManagedStyleSheet.EktronWorkareaIeCss, Ektron.Cms.API.Css.BrowserTarget.LessThanEqualToIE7);
        Ektron.Cms.API.Css.RegisterCss(this, Ektron.Cms.API.Css.ManagedStyleSheet.EktronModalCss);
        Ektron.Cms.API.Css.RegisterCss(this, Ektron.Cms.API.Css.ManagedStyleSheet.EktronTreeviewCss);
        Ektron.Cms.API.Css.RegisterCss(this, Ektron.Cms.API.Css.ManagedStyleSheet.EktronClueTipCss);
        Ektron.Cms.API.Css.RegisterCss(this, _ContentApi.ApplicationPath + "controls/DeviceConfiguration/deviceTree.css", "EktronDeviceTreeCSS");
    }

    private void RegisterJS()
    {
        Ektron.Cms.API.JS.RegisterJS(this, Ektron.Cms.API.JS.ManagedScript.EktronJS);
        Ektron.Cms.API.JS.RegisterJS(this, Ektron.Cms.API.JS.ManagedScript.EktronWorkareaJS);
        Ektron.Cms.API.JS.RegisterJS(this, Ektron.Cms.API.JS.ManagedScript.EktronWorkareaHelperJS);
        Ektron.Cms.API.JS.RegisterJS(this, Ektron.Cms.API.JS.ManagedScript.EktronJFunctJS);
        Ektron.Cms.API.JS.RegisterJS(this, Ektron.Cms.API.JS.ManagedScript.EktronModalJS);
        Ektron.Cms.API.JS.RegisterJS(this, Ektron.Cms.API.JS.ManagedScript.EktronTreeviewJS);
        Ektron.Cms.API.JS.RegisterJS(this, Ektron.Cms.API.JS.ManagedScript.EktronClueTipJS);
    }

    #endregion
}
