using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using Ektron.Cms.Common;
using Ektron.Cms;
using Ektron.Cms.Device;
using System.Data;

public partial class ViewAllDeviceConfigurations : System.Web.UI.UserControl
{
    protected EkMessageHelper _MessageHelper;
    protected StyleHelper _StyleHelper;
    protected ContentAPI _ContentApi = new ContentAPI();
    protected int _ItemCount = 0;
    protected void Page_Init(object sender, System.EventArgs e)
    {
        this.RegisterCSS();
        this.RegisterJS();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        imgCloseAddItemModal.ImageUrl = _ContentApi.ApplicationPath + "images/ui/icons/cancel.png";
        //updateWurfl.Text = "WURFL FILE MANAGER";
        //iframeUpdateWurflFile.Attributes.Add("src", _ContentApi.ApplicationPath + "updatewurflfile.aspx?action=view");
        _MessageHelper = _ContentApi.EkMsgRef;
        _StyleHelper = new StyleHelper();

        if (!_ContentApi.RequestInformationRef.IsDeviceDetectionEnabled)
        {
            dvMessage.Visible = true;
            lblEnableDeviceDetection.Text = _MessageHelper.GetMessage("lbl Enable Device Detection");


        }
        else
        {
            //Adaptive image cache
            btnClearCache.Text = _MessageHelper.GetMessage("lbl adaptive img btn clear text");
            pnlAdpImg.Visible = true;
            ltrLV1Cnt.Text = Ektron.ASM.EkHttpDavHandler.AdaptiveImageProcessor.Instance.Level1CacheCount.ToString();
            ltrLV2Cnt.Text = Ektron.ASM.EkHttpDavHandler.AdaptiveImageProcessor.Instance.Level2CacheCount.ToString();
        }
        
        BindData();
        ViewDevicesToolBar();
    }
    protected void clearCache_click(object sender, EventArgs e)
    {
        Ektron.ASM.EkHttpDavHandler.AdaptiveImageProcessor.Instance.ClearCache();
        ltrLV1Cnt.Text = Ektron.ASM.EkHttpDavHandler.AdaptiveImageProcessor.Instance.Level1CacheCount.ToString();
        ltrLV2Cnt.Text = Ektron.ASM.EkHttpDavHandler.AdaptiveImageProcessor.Instance.Level2CacheCount.ToString();
    }
    #region Private Methods

    private void ViewDevicesToolBar()
    {
        StringBuilder sb = new StringBuilder();

        divTitleBar.InnerHtml = _StyleHelper.GetTitleBar(_MessageHelper.GetMessage("view all device configurations"));

        sb.Append("<table><tbody><tr>");
        if (_ContentApi.RequestInformationRef.IsDeviceDetectionEnabled == true)
        {
            sb.Append(_StyleHelper.GetButtonEventsWCaption(_ContentApi.AppImgPath + "../UI/Icons/add.png", "settings.aspx?action=adddeviceconfiguration&os=false", _MessageHelper.GetMessage("alt add button text (device configuration)"), _MessageHelper.GetMessage("btn add device configuration"), "", StyleHelper.AddButtonCssClass, true));
            sb.Append(_StyleHelper.GetButtonEventsWCaption(_ContentApi.AppImgPath + "../UI/Icons/add.png", "settings.aspx?action=adddeviceconfiguration&os=true", _MessageHelper.GetMessage("alt add button text (device configuration)"), _MessageHelper.GetMessage("btn add device configuration by os"), "", StyleHelper.AddButtonCssClass, true));
            //sb.Append(_StyleHelper.GetButtonEventsWCaption(_ContentApi.AppImgPath + "../UI/Icons/add.png", "#", _MessageHelper.GetMessage("alt add button text (update wurfl file)"), "WURFL File Manager", "", "ektronModal " + StyleHelper.AddButtonCssClass, true));
            if (_ItemCount > 1)
            {
                sb.Append(_StyleHelper.GetButtonEventsWCaption(_ContentApi.AppImgPath + "../UI/Icons/arrowUpDown.png", "settings.aspx?action=reorderdeviceconfigurations", _MessageHelper.GetMessage("alt: reorder devices text"), _MessageHelper.GetMessage("btn reorder device configurations"), "", StyleHelper.ReOrderButtonCssClass));
			    sb.Append(_StyleHelper.GetButtonEventsWCaption(_ContentApi.AppImgPath + "../UI/Icons/delete.png", "settings.aspx?action=deletedeviceconfiguration", _MessageHelper.GetMessage("alt remove button text (device configuration)"), _MessageHelper.GetMessage("btn remove device configurations"), "", StyleHelper.DeleteButtonCssClass));
            } 
			sb.Append(StyleHelper.ActionBarDivider);
        }
        sb.Append("<td>");
        sb.Append(_StyleHelper.GetHelpButton("viewalldeviceconfigurations", ""));
        sb.Append("</td>");
        sb.Append("</tr></tbody></table>");

        divToolBar.InnerHtml = sb.ToString();
    }

    private void BindData()
    {
        CmsDeviceConfiguration cDevice = new CmsDeviceConfiguration(_ContentApi.RequestInformationRef);
        CmsDeviceConfigurationCriteria criteria = new CmsDeviceConfigurationCriteria();
        List<CmsDeviceConfigurationData> cDeviceList;
        StringBuilder sBuilder = new StringBuilder();

        criteria.OrderByField = Ektron.Cms.Device.CmsDeviceConfigurationProperty.Order;
        criteria.OrderByDirection = EkEnumeration.OrderByDirection.Ascending;
        cDeviceList = cDevice.GetList(criteria);
        _ItemCount = cDeviceList.Count;

        System.Web.UI.WebControls.BoundColumn colBound = new System.Web.UI.WebControls.BoundColumn();
        colBound.DataField = "Device";
        colBound.HeaderStyle.CssClass = "left";
        colBound.ItemStyle.CssClass = "left";
        colBound.HeaderText = _MessageHelper.GetMessage("lbl Device");
        DeviceListGrid.Columns.Add(colBound);

        colBound = new System.Web.UI.WebControls.BoundColumn();
        colBound.DataField = "Models";
        //colBound.HeaderStyle.CssClass = "center";
        //colBound.ItemStyle.CssClass = "center";
        colBound.HeaderText = _MessageHelper.GetMessage("lbl Device Models");
        DeviceListGrid.Columns.Add(colBound);

        DataTable dt = new DataTable();
        DataRow dr;

        dt.Columns.Add(new DataColumn("Device", typeof(string)));
        dt.Columns.Add(new DataColumn("Models", typeof(string)));
 
        if (cDeviceList.Count > 2)
        {
            for (int i = 2; i <= cDeviceList.Count - 1; i++)
            {
                sBuilder = new StringBuilder();
                dr = dt.NewRow();
                if (_ContentApi.RequestInformationRef.IsDeviceDetectionEnabled)
                    dr[0] = "<a href=\'settings.aspx?action=viewdeviceconfiguration&id=" + cDeviceList[i].Id + "\' title=\'" + EkFunctions.HtmlEncode(cDeviceList[i].Name) + "\'>" + EkFunctions.HtmlEncode(cDeviceList[i].Name) + "</a>";
                else
                    dr[0] = EkFunctions.HtmlEncode(cDeviceList[i].Name);

                foreach (string cModel in cDeviceList[i].Models)
                {
                    sBuilder.Append(cModel).Append(",");
                    dr[1] = sBuilder.ToString().TrimEnd(new char[] { ',' });
                }

                dt.Rows.Add(dr);
            }
        }

        // Always display Generic Mobile and Generic last... 

        for (int i = 1; i >= 0; i--)
        {
            sBuilder = new StringBuilder();
            dr = dt.NewRow();
            if (_ContentApi.RequestInformationRef.IsDeviceDetectionEnabled)
                dr[0] = "<a href=\'settings.aspx?action=viewdeviceconfiguration&id=" + cDeviceList[i].Id + "\' title=\'" + EkFunctions.HtmlEncode(cDeviceList[i].Name) + "\'>" + EkFunctions.HtmlEncode(cDeviceList[i].Name) + "</a>";
            else
                dr[0] = EkFunctions.HtmlEncode(cDeviceList[i].Name);

            if(cDeviceList[i].Id == 0)
                dr[1] = "Generic Mobile Devices";
            else if (cDeviceList[i].Id == 1)
                dr[1] = "Generic";

            dt.Rows.Add(dr);
        }

        DataView dv = new DataView(dt);
        DeviceListGrid.DataSource = dv;
        DeviceListGrid.DataBind();
    }

    #endregion

    #region CSS, JS

    private void RegisterCSS()
    {
        Ektron.Cms.API.Css.RegisterCss(this, Ektron.Cms.API.Css.ManagedStyleSheet.EktronWorkareaCss);
        Ektron.Cms.API.Css.RegisterCss(this, Ektron.Cms.API.Css.ManagedStyleSheet.EktronWorkareaIeCss, Ektron.Cms.API.Css.BrowserTarget.LessThanEqualToIE7);
        Ektron.Cms.API.Css.RegisterCss(this, Ektron.Cms.API.Css.ManagedStyleSheet.EktronModalCss);
    }

    private void RegisterJS()
    {
        Ektron.Cms.API.JS.RegisterJS(this, Ektron.Cms.API.JS.ManagedScript.EktronJS);
        Ektron.Cms.API.JS.RegisterJS(this, Ektron.Cms.API.JS.ManagedScript.EktronWorkareaJS);
        Ektron.Cms.API.JS.RegisterJS(this, Ektron.Cms.API.JS.ManagedScript.EktronWorkareaHelperJS);
        Ektron.Cms.API.JS.RegisterJS(this, Ektron.Cms.API.JS.ManagedScript.EktronJFunctJS);
        Ektron.Cms.API.JS.RegisterJS(this, Ektron.Cms.API.JS.ManagedScript.EktronModalJS);
    }

    #endregion

}