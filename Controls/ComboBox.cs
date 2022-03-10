using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;


[assembly: System.Web.UI.WebResource("Controls.ComboBox.js", "text/javascript")]
namespace Controls
{
    public class ComboBox : TextBox, IScriptControl, ICallbackEventHandler, IPostBackEventHandler, IKeyControl
    {
        private class CallBackArguments
        {
            private const char _split = '|';

            public string Identifier { get; set; }
            public bool RaiseMore { get; set; }
            public string SourceId { get; set; }
            public string Key { get; set; }
            public string SearchText { get; set; }

            public CallBackArguments(string callBackString)
            {
                var arguments = callBackString.Split(_split);

                Identifier = arguments[0];
                RaiseMore = arguments[1] == "1";
                SourceId = arguments[2];
                Key = arguments[3];
                SearchText = arguments[4];
            }

            public CallBackArguments(string identifier, bool raiseMore, string sourceId, string key, string searchText)
            {
                Identifier = identifier;
                RaiseMore = raiseMore;
                SourceId = sourceId;
                Key = key;
                SearchText = searchText;
            }

            public override string ToString()
            {
                return Identifier + _split
                       + (RaiseMore ? "1" : "0") + _split
                       + SourceId + _split
                       + Key + _split
                       + "' + " + SearchText + " + '";
            }
        }

        public event EventHandler<EventArgs<string>> SelectedItemChanged;

        protected virtual void OnSelectedItemChanged(EventArgs<string> e)
        {
            if (SelectedItemChanged != null)
            {
                SelectedItemChanged(this, e);
            }
        }

        public event EventHandler OtherItemSelected;

        protected virtual void OnOtherItemSelected(EventArgs e)
        {
            if (OtherItemSelected != null)
            {
                OtherItemSelected(this, e);
            }
        }

        // marker used in class of an gridrow element, so javascript can detect its a row
        private const string GridRowMarker = " _grMarker_ ";
        private const string MoreRowMarker = " _mrMarker_ ";

        // marker used in class of an gridcell element, so javascript can detect its the 'resultcolumn'
        private const string ResultColumnMarker = " _rcMarker_ ";

        private const string TextParameter = "_textParam";

        private const string HeadersVisibleKey = "_hv";
        private const string CssPanelKey = "_cp";
        private const string CssGridKey = "_cg";
        private const string CssButtonKey = "_cb";
        private const string CssGridHKey = "_ch";
        private const string CssGridSKey = "_cs";
        private const string DsObjectKey = "_ds";
        private const string ShowNewKey = "_sn";
        private const string ResultColumnKey = "_rc";
        private const string PhKey = "_ph";
        private const string AutoExpandKey = "_ae";
        private const string AutoMoreKey = "_am";
        private const string AutoFilterKey = "_af";
        public const string RealTextKey = "_rt";

        public const string ConversionEnabledKey = "_ce";
        public const string ConverterKey = "_cv";

        // CSS Constants (are appended to 'main'-CssClass)
        private const string CssSpecialItem = "SpecialItem";
        private const string CssSelectedRow = "Selected";
        private const string CssHeaderRow = "Header";
        private const string CssItem = "Item";

        private ScriptManager sm;
        private const string btnId = "_btn";
        private const string pnlId = "_pnl";
        private const string grvId = "_grv";

        private Panel popupPanel;
        private HtmlGenericControl popupDiv;
        private HtmlGenericControl popupDiv2;
        private GridView grid;
        private Button popupButton;


        private IKeyedObject _cnv;
        /// <summary>
        /// Set converter used for converting Key to Text
        /// </summary>
        /// <param name="converter"></param>
        public void SetConverter(IKeyedObject converter)
        {
            _cnv = converter;
        }

        public IKeyedObject GetConverter()
        {
            return _cnv;
        }

        [Bindable(true), Category("Behavior"), Description("Class which should be used for textconversion"), DefaultValue(""), Localizable(true)]
        public string Converter
        {
            get { return ViewState.Get(ConverterKey, string.Empty); }
            set
            {
                if (!DesignMode)
                {
                    ViewState[ConverterKey] = value;
                    KeyControlHelper.LoadConverter(this);
                }
            }
        }

        [Bindable(true), Category("Behavior"), Description("Determines if the text should be converted before being displayed"), DefaultValue(true), Localizable(true)]
        public bool ConversionEnabled
        {
            get { return ViewState.Get(ConversionEnabledKey, true); }
            set { ViewState[ConversionEnabledKey] = value; }
        }


        private IKeyedObjectContainer __dso = null;

        private IKeyedObjectContainer _dso
        {
            get
            {
                if (__dso == null)
                {
                    Type t = BuildManager.GetType(DsObject, true);
                    __dso = ((IKeyedObjectContainer)t.InvokeMember(string.Empty, BindingFlags.CreateInstance, null, null, null, CultureInfo.InvariantCulture));
                }
                return __dso;
            }
        }


        [Bindable(true), Category("Behavior"), Description("Class which should be used to retrieve data"), DefaultValue(""), Localizable(true)]
        public string DsObject
        {
            get { return ViewState.Get(DsObjectKey, string.Empty); }
            set { ViewState[DsObjectKey] = value; }
        }

        [Bindable(true), Category("Behavior"), Description("Text which should be added to the list indicating 'all items'"), DefaultValue(""), Localizable(true)]
        public string AllItemText
        {
            get { return ViewState.Get("_aik", string.Empty); }
            set { ViewState["_aik"] = value; }
        }

        [Bindable(true), Category("Behavior"), Description("Message which should be shown to the user if he wants to expand the list without entering a searchstring"), DefaultValue(""), Localizable(true)]
        public string EmptyFilterNotAllowedText
        {
            get { return ViewState.Get("_eik", string.Empty); }
            set { ViewState["_eik"] = value; }
        }

        [Bindable(true), Category("Behavior"), Description("Message which should be shown to the user if there are no searchresults"), DefaultValue(""), Localizable(true)]
        public string NoResultsText
        {
            get { return ViewState.Get("_nrk", string.Empty); }
            set { ViewState["_nrk"] = value; }
        }

        [Bindable(true), Category("Behavior"), Description("Indicates if combobox should expand if user uses textbox"), DefaultValue(false), Localizable(true)]
        public bool AutoExpand
        {
            get { return ViewState.Get(AutoExpandKey, false); }
            set { ViewState[AutoExpandKey] = value; }
        }

        [Bindable(true), Category("Behavior"), Description("Indicates that a 'more' option shouldn't be rendered, but if reached by scrolling it should be automatically called"), DefaultValue(false), Localizable(true)]
        public bool AutoMore
        {
            get { return ViewState.Get(AutoMoreKey, false); }
            set { ViewState[AutoMoreKey] = value; }
        }

        [Bindable(true), Category("Behavior"), Description("Indicates that list should be filtered based on the characters typed by user"), DefaultValue(false), Localizable(true)]
        public bool AutoFilter
        {
            get { return ViewState.Get(AutoFilterKey, false); }
            set { ViewState[AutoFilterKey] = value; }
        }

        [Bindable(true), Category("Behavior"), Description("Property of DsObject which should be presented in textbox"), DefaultValue(""), Localizable(true)]
        public string ResultColumn
        {
            get { return ViewState.Get(ResultColumnKey, string.Empty); }
            set { ViewState[ResultColumnKey] = value; }
        }

        [Bindable(true), Category("Behavior"), Description("Which placeholder text should be visible if no search term is entered yet"), DefaultValue(""), Localizable(true)]
        public string Placeholder
        {
            get { return ViewState.Get(PhKey, string.Empty); }
            set { ViewState[PhKey] = value; }
        }

        /// <summary>
        /// optional: client ID of a button which should be shown as soon as a selection is made
        /// </summary>
        public string UnselectButtonID { get; set; }

        private Control UnselectButton
        {
            get
            {
                Control nc = this.NamingContainer;
                Control c = null;

                while (UnselectButtonID != null && c == null && nc != null)
                {
                    c = nc.FindControl(UnselectButtonID);
                    nc = nc.NamingContainer;
                }
                return c;
            }
        }


        [Bindable(true)]
        [IDReferenceProperty()]
        [TypeConverter(typeof(Control))]
        [Category("Misc")]
        [DefaultValue("")]
        [Localizable(true)]
        [Description("Optional: ID of a button which should be activated as soon as a selection is made")]
        public string ConfirmButtonID { get; set; }

        private Control ConfirmButton
        {
            get
            {
                Control nc = this.NamingContainer;
                Control c = null;

                while (ConfirmButtonID != null && c == null && nc != null)
                {
                    c = nc.FindControl(ConfirmButtonID);
                    nc = nc.NamingContainer;
                }
                return c;
            }
        }

        [Bindable(true), Category("Appearance"), Description("Text of textbox before conversion"), DefaultValue(""), Localizable(true)]
        public string Key
        {
            get { return ViewState.Get(RealTextKey, string.Empty); }
            set
            {
                if (!DesignMode)
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        ViewState[RealTextKey] = null;
                        Text = string.Empty;
                    }
                    else
                    {
                        // two 'types' of keys are allowed here: a 'real' xmlserialized-keystring, or just a simple string.
                        // in this last case we are assuming it's a Key<string>.
                        string keyString = KeyHelper.IsKey(value) ? value : new Key<string>(value).ToString();
                        ViewState[RealTextKey] = keyString;
                        Text = KeyControlHelper.ConvertToText(this, keyString);
                    }
                }
            }
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();


            popupButton = new Button();

            // invisible button javascript is tied to.
            popupButton.Style.Add("display", "none");
            popupButton.ID = this.ID + btnId;
            popupButton.TabIndex = -1;
            popupButton.CausesValidation = false;
            this.Controls.Add(popupButton);

            var cb = new CallBackArguments("sh", false, this.UniqueID, string.Empty, TextParameter);

            this.Attributes.Add("onclick", "DoNotCollapse();");
            this.Attributes.Add("onfocus", "this.select();");
            if (!string.IsNullOrWhiteSpace(Placeholder))
            {
                this.Attributes.Add("placeholder", Placeholder);
            }
            this.Attributes.Add("onchange", "if (_ddButtonClicked) return;");
            this.Attributes.Add("autocomplete", "off");
            popupButton.Attributes.Add("onmousedown", "DdButtonClicked();");
            // bij click van de button naast de combobox, een callback uitvoeren. Daarnaast de huidige
            // inhoud van de textbox meegeven in de vorm van 'textparam'
            popupButton.OnClientClick = "SetTextParam('" + this.ClientID + "', null); return DoCbCallBack('" + cb.ToString() + "', '" + this.ClientID + pnlId + "');";
            popupDiv = new HtmlGenericControl("div");
            popupDiv2 = new HtmlGenericControl("div");
            popupPanel = new Panel();
            popupDiv.Controls.Add(popupPanel);
            popupDiv.Style.Add("clear", "both");
            popupPanel.Controls.Add(popupDiv2);

            this.Controls.Add(popupDiv);

            popupPanel.ID = this.ID + pnlId;
            popupPanel.Width = PanelWidth;

            popupPanel.CssClass = CssClassPanel;
            popupDiv.Style.Add("display", "none");
        }

        protected override void OnLoad(EventArgs e)
        {
            string js = Page.ClientScript.GetCallbackEventReference("GetSourceId(arg)", "arg", "ReceiveServerData", "ctx", "ReceiveServerData", true);
            string script = "function DoCbCallBack(arg, ctx) { " + js + "; return false; }";
            Page.ClientScript.RegisterClientScriptBlock(Page.GetType(), "callbackkey", script, true);
            base.OnLoad(e);
        }

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            ScriptControlDescriptor descriptor = new ScriptControlDescriptor("Controls.ComboBox", this.ClientID);
            descriptor.AddElementProperty("Panel", this.ClientID + pnlId);
            descriptor.AddElementProperty("TargetControl", this.ClientID);
            descriptor.AddElementProperty("Button", popupButton.ClientID);
            descriptor.AddProperty("CssClassGridView", CssClassGridView);
            descriptor.AddProperty("AutoExpand", AutoExpand);
            descriptor.AddProperty("AutoMore", AutoMore);
            descriptor.AddProperty("AutoFilter", AutoFilter);
            if (ConfirmButton != null)
            {
                descriptor.AddElementProperty("ConfirmButton", ConfirmButton.ClientID);
            }
            yield return descriptor;
        }

        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("Controls.ComboBox.js", this.GetType().Assembly.FullName);
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (!DesignMode)
            {
                sm = ScriptManager.GetCurrent(Page);

                if (sm == null)
                {
                    throw new HttpException("ScriptManager must be on page");
                }
                sm.RegisterScriptControl<ComboBox>(this);
            }
            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (!base.DesignMode)
            {
                sm.RegisterScriptDescriptors(this);
            }

            base.Render(writer);

            if (!DesignMode)
            {
                popupButton.RenderControl(writer);
                writer.WriteBreak();
                popupDiv.RenderControl(writer);
            }
        }

        protected string callbackResult;

        public string GetCallbackResult()
        {
            return callbackResult;
        }

        [Bindable(true), Category("Behavior"), Description("Determines if headers are visible in popup"), DefaultValue(false), Localizable(true)]
        public bool ShowHeader
        {
            get { return ViewState.Get(HeadersVisibleKey, false); }
            set { ViewState[HeadersVisibleKey] = value; }
        }

        [Bindable(true), Category("Appearance"), Description("CssClass of popup panel"), DefaultValue(""), Localizable(true)]
        public string CssClassPanel
        {
            get { return ViewState.Get(CssPanelKey, string.Empty); }
            set
            {
                ViewState[CssPanelKey] = value;
            }
        }

        [Bindable(true), Category("Appearance"), Description("CssClass of gridview"), DefaultValue(""), Localizable(true)]
        public string CssClassGridView
        {
            get { return ViewState.Get(CssGridKey, string.Empty); }
            set { ViewState[CssGridKey] = value; }
        }

        [Bindable(true), Category("Appearance"), Description("Should a 'new item' entry appear in list"), DefaultValue(false), Localizable(true)]
        public bool ShowNew
        {
            get { return ViewState.Get(ShowNewKey, false); }
            set { ViewState[ShowNewKey] = value; }
        }

        private Unit _panelWidth = new Unit(400, UnitType.Pixel);

        public Unit PanelWidth
        {
            get { return _panelWidth; }
            set { _panelWidth = value; }
        }

        private Type gridDataType = typeof(object);
        private KeyInfo ki;
        private string otherKey = string.Empty;
        private int otherPosition = -1;

        public void RaiseCallbackEvent(string eventArgument)
        {
            if (eventArgument.Length > 0)
            {
                CallBackArguments cb = new CallBackArguments(eventArgument);
                if (cb.Identifier == "sh")
                {
                    if (AutoFilter)
                    {
                        _dso.FilterBy(cb.SearchText);
                    }

                    int noOfAddedRows = 0;

                    _dso.AllItemText = AllItemText;

                    if (cb.RaiseMore)
                    {
                        noOfAddedRows = _dso.GetMore();
                    }

                    var sw = new StringWriter(CultureInfo.InvariantCulture);
                    var hw = new HtmlTextWriter(sw);

                    if (!string.IsNullOrWhiteSpace(EmptyFilterNotAllowedText) && string.IsNullOrWhiteSpace(cb.SearchText))
                    {
                        // combobox can only be used for filtered datasets. 
                        var div = new HtmlGenericControl("span");
                        div.Attributes.Add("class", CssClassGridView + CssSpecialItem);
                        div.InnerText = EmptyFilterNotAllowedText;
                        div.RenderControl(hw);
                    }
                    else
                    {
                        grid = new GridView();
                        var data = _dso.GetData().ToList<IKeyedObject>();
                        if (data.Count > 0)
                        {
                            // other item uit de lijst halen

                            for (int i = data.Count - 1; i >= 0; i--)
                            {
                                if (data[i].GetKeyInfo().Other)
                                {
                                    ki = data[i].GetKeyInfo();
                                    otherKey = data[i].GetKey();
                                    otherPosition = i;
                                    data.RemoveAt(i);
                                    break;
                                }
                            }
                            if (data.Count > 0)
                            {
                                gridDataType = data[0].GetType();
                            }
                        }

                        grid.DataSource = data.ToList<object>();

                        grid.ID = ID + grvId;
                        grid.UseAccessibleHeader = ShowHeader;
                        grid.RowDataBound += new GridViewRowEventHandler(grid_RowDataBound);
                        grid.CssClass = CssClassGridView;
                        grid.RowStyle.CssClass = CssClassGridView + CssItem;
                        grid.HeaderStyle.CssClass = CssClassGridView + CssHeaderRow;
                        grid.SelectedRowStyle.CssClass = CssClassGridView + CssSelectedRow;
                        grid.DataBind();

                        if (cb.RaiseMore)
                        {
                            // if 'more' is selected, the last 'old' element is the new selected row
                            int newIndex = grid.Rows.Count - 2 - noOfAddedRows;

                            if (newIndex > 0)
                            {
                                DeselectRow(grid.Rows);

                                // no scrolling at client side -> scroll to last known position.
                                SelectedRowIndex = -2;
                                grid.Rows[newIndex].CssClass += CssClassGridView + CssSelectedRow;
                            }
                        }

                        // zet rijen in tabel en voeg 'other item' toe
                        Table tabel = new Table();
                        tabel.CssClass = CssClassGridView;
                        tabel.ID = ID + grvId;

                        int j = 0;
                        TableCell addedTd = null;
                        TableCell moreTd = null;
                        TableRow addedTr = null;
                        int maxSpan = 1;

                        if (grid.HeaderRow != null)
                        {
                            tabel.Rows.Add(grid.HeaderRow);
                        }
                        foreach (GridViewRow row in grid.Rows)
                        {
                            // search more row
                            if (row.CssClass.Contains(MoreRowMarker))
                            {
                                moreTd = row.Cells[0];
                            }

                            if (row.Cells.Count > maxSpan)
                            {
                                // count visible cells
                                int noOfCells = 0;
                                foreach (TableCell cel in row.Cells)
                                {
                                    if (cel.Visible)
                                    {
                                        noOfCells++;
                                    }
                                }
                                if (noOfCells > maxSpan)
                                {
                                    maxSpan = noOfCells;
                                }
                            }
                            if (j == otherPosition)
                            {
                                if (!string.IsNullOrWhiteSpace(ki.Text))
                                {
                                    addedTr = new TableRow();
                                    tabel.Rows.Add(addedTr);

                                    addedTd = new TableCell();
                                    addedTd.Text = ki.Text;

                                    addedTr.Cells.Add(addedTd);
                                }
                            }
                            tabel.Rows.Add(row);
                            j++;
                        }
                        if (j == otherPosition)
                        {
                            if (!string.IsNullOrWhiteSpace(ki.Text))
                            {
                                addedTr = new TableRow();
                                tabel.Rows.Add(addedTr);

                                addedTd = new TableCell();
                                addedTd.Text = ki.Text;

                                addedTr.Cells.Add(addedTd);
                            }
                        }

                        if (addedTd != null && addedTr != null)
                        {
                            addedTd.ColumnSpan = maxSpan;
                            addedTr.Attributes.Add("onclick", Page.ClientScript.GetPostBackEventReference(this, otherKey));
                            addedTr.CssClass = CssClassGridView + CssSpecialItem + GridRowMarker;
                        }
                        if (moreTd != null)
                        {
                            moreTd.ColumnSpan = maxSpan;
                        }

                        if (tabel.Rows.Count > 0)
                        {
                            // render tabel
                            tabel.RenderControl(hw);
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(NoResultsText))
                            {
                                var div = new HtmlGenericControl("span");
                                div.Attributes.Add("class", CssClassGridView + CssSpecialItem);
                                div.InnerText = NoResultsText;
                                div.RenderControl(hw);
                            }
                        }
                    }

                    callbackResult = "1|" + (SelectedRowIndex - HiddenRowsBeforeSelectedRow).ToString(CultureInfo.InvariantCulture) + "|" + sw.ToString();
                }
            }
        }

        private void DeselectRow(GridViewRowCollection rows)
        {
            foreach (GridViewRow row in rows)
            {
                // unselect any potentially selected row
                row.CssClass = row.CssClass.Replace(CssClassGridView + CssSelectedRow, string.Empty);
            }
        }

        private int SelectedRowIndex = -1;
        private int HiddenRowsBeforeSelectedRow = 0;

        private void grid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                ApplyAttributes(e.Row.Cells, e.Row.RowType);
                if (!ShowHeader)
                {
                    foreach (TableCell cel in e.Row.Cells)
                    {
                        cel.Text = string.Empty;
                        cel.Height = new Unit(0, UnitType.Pixel);
                        cel.Style.Add(HtmlTextWriterStyle.Margin, "0px");
                        cel.Style.Add(HtmlTextWriterStyle.Padding, "0px");
                    }
                }
            }
            else if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var dataItem = (IKeyedObject)e.Row.DataItem;

                // selecteer rij welke overeenkomt met key.
                if (dataItem.GetKey() == Key)
                {
                    e.Row.CssClass = CssClassGridView + CssSelectedRow;
                    SelectedRowIndex = e.Row.RowIndex;
                }

                ApplyAttributes(e.Row.Cells, e.Row.RowType);

                KeyInfo ki = dataItem.GetKeyInfo();

                if (ki.More)
                {
                    CallBackArguments cb = new CallBackArguments("sh", true, UniqueID, dataItem.GetKey(), TextParameter);

                    e.Row.Attributes.Add("onclick", "MoreClicked(this); return DoCbCallBack('" + cb.ToString() + "', '" + ClientID + pnlId + "');");
                    e.Row.CssClass = CssClassGridView + CssSpecialItem;
                    e.Row.CssClass += MoreRowMarker;
                    var moreCell = e.Row.Cells[e.Row.Cells.Count - 2];
                    moreCell.ColumnSpan = e.Row.Cells.Count - 1;
                    // mogelijk is deze cel onterecht onzichtbaar gemaakt in in ApplyAttributes
                    moreCell.Visible = true;
                    e.Row.Cells.Clear();
                    e.Row.Cells.Add(moreCell);

                    if (AutoMore)
                    {
                        if (SelectedRowIndex == -1)
                        {
                            HiddenRowsBeforeSelectedRow++;
                        }
                        e.Row.Style.Add("display", "none");
                    }
                }
                else if (ki.Other)
                {
                    e.Row.Visible = false;
                }
                else if (ki.New)
                {
                    if (ShowNew)
                    {
                        e.Row.Attributes.Add("onclick", Page.ClientScript.GetPostBackEventReference(this, dataItem.GetKey()));
                        e.Row.CssClass = CssClassGridView + CssSpecialItem;
                        var otherCell = e.Row.Cells[e.Row.Cells.Count - 2];
                        otherCell.ColumnSpan = e.Row.Cells.Count - 1;
                        e.Row.Cells.Clear();
                        e.Row.Cells.Add(otherCell);
                    }
                    else
                    {
                        if (SelectedRowIndex == -1)
                        {
                            HiddenRowsBeforeSelectedRow++;
                        }
                        e.Row.Visible = false;
                    }
                }
                else
                {
                    var clientIdConfirmButton = (ConfirmButton != null) ? ConfirmButton.ClientID : "null";
                    var clientIdUnselectButton = (UnselectButton != null) ? UnselectButton.ClientID : "null";

                    var script = $"ItemClicked({clientIdConfirmButton}, {clientIdUnselectButton},{this.ClientID}, '{dataItem.ToString()}');";
                    script += Page.ClientScript.GetPostBackEventReference(this, dataItem.GetKey());
                    e.Row.Attributes.Add("onclick", script);
                }

                // mark this as a gridview datarow for usage in javascript
                e.Row.CssClass += GridRowMarker;
            }
        }

        private void ApplyAttributes(TableCellCollection cells, DataControlRowType rowType)
        {
            int i = 0;

            foreach (var prp in gridDataType.GetProperties())
            {
                if (!IsBindableType(prp.PropertyType))
                {
                    continue;
                }

                if (prp.Name == ResultColumn)
                {
                    cells[i].CssClass += ResultColumnMarker;
                }

                var attributes = prp.GetCustomAttributes(true);
                foreach (var attribute in attributes)
                {
                    if (cells.Count > i)
                    {
                        VisibleAttribute va = attribute as VisibleAttribute;

                        if (va != null)
                        {
                            cells[i].Visible = va.Visible;
                        }

                        WidthAttribute wa = attribute as WidthAttribute;

                        if (wa != null)
                        {
                            cells[i].Width = wa.Width;
                        }

                        if (rowType == DataControlRowType.Header)
                        {
                            DisplayTextAttribute da = attribute as DisplayTextAttribute;

                            if (da != null)
                            {
                                cells[i].Text = da.Text;
                            }
                        }
                    }
                }
                i++;
            }
        }

        public void RaisePostBackEvent(string eventArgument)
        {
            // indien het eventargument een 'other key' bevat, zijn er twee mogelijkheden.
            // 1) we willen het 'onOtherItemSelected' event doorgeven aan de pagina
            // 2) indien er een 'allitem' is toegevoegd, willen we deze gewoon selecteren.

            if (String.IsNullOrEmpty(eventArgument))
            {
                // user has entered an non existing value or hasn't validated it through the list!

                var result = _dso.GetSingleItem(Text);

                if (result != null)
                {
                    // via 'achterdeur' gegevens zetten, zodat deze niet nog een keer opgehaald hoeven te worden
                    ViewState[RealTextKey] = result.GetKey();
                    Text = result.ToString();

                    OnSelectedItemChanged(new EventArgs<string>(result.GetKey()));
                    return;
                }

                // no match found
                OnTextChanged(EventArgs.Empty);
                return;
            }

            var keyInfo = KeyInfo.CreateKeyInfo(eventArgument);
            if (keyInfo.Other)
            {
                if (string.IsNullOrWhiteSpace(AllItemText))
                {
                    OnOtherItemSelected(new EventArgs());
                    return;
                }
            }

            Key = eventArgument;
            OnSelectedItemChanged(new EventArgs<string>(eventArgument));
        }

        private static bool IsBindableType(Type type)
        {
            if (type == null)
            {
                return false;
            }
            Type underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                type = underlyingType;
            }
            return type.IsPrimitive || type == typeof(string) || type == typeof(DateTime) || type == typeof(decimal) || type == typeof(Guid) || type == typeof(DateTimeOffset) || type == typeof(TimeSpan);
        }
    }
}