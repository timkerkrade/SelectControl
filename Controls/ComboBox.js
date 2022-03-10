/// <reference name="MicrosoftAjax.js"/>

Type.registerNamespace("Controls");

_textParam = '';

function ResetGlobals() {
    _moreClicked = false;
    _doNotCollapse = false;
    _ddButtonClicked = false;
    _notCollapseId = '';
}

/* function will be called before callback, so we know that more data should be retrieved */
function MoreClicked(element) {
    _moreClicked = true;
    _scrollTopBeforeMoreCallback = element.parentNode.parentNode.parentNode.scrollTop;
}

/* function will be called before callback, so we know a row whas clicked */
function ItemClicked(confirmButtonId, unselectButtonId, textboxId, result) {
    if (confirmButtonId !== null) {
        $(confirmButtonId).prop('disabled', false);
    }
    if (unselectButtonId !== null && $(unselectButtonId).length) {
        $(unselectButtonId).show();
    }
    $('.org-select-notification').css('visibility', 'visible');
    $('.org-select-error').css('visibility', 'hidden');
    $(textboxId).css('color', 'black');
    $(textboxId).val(result);
}

/* marker which indicates the button from the combobox was clicked */
function DdButtonClicked() {
    _ddButtonClicked = true;
    DoNotCollapse();
}

/* function can be called before callback, we won't collapse the panel then 
   this can be seen as a marker that the button is clicked, this button calls 
   this method before executing its code */
function DoNotCollapse(id) {
    _doNotCollapse = true;
    _notCollapseId = id;
}

function GetTextParam() {
    return _textParam;
}
function SetTextParam(id, value) {
    if (id == null)
        _textParam = value;
    else if (document.getElementById(id))
        _textParam = document.getElementById(id).value;
}

/* helper function checking if input is numeric */
function IsNumeric(input) {
    return (input - 0) == input && input.length > 0;
}

/* helper function getting the closest parent with the given tag name. */
function getParentByTagName(obj, tag) {
    var obj_parent = obj.parentNode;
    if (!obj_parent) return false;
    if (!obj_parent.tagName) return false;
    if (obj_parent.tagName.toLowerCase() == tag) return obj_parent;
    else return getParentByTagName(obj_parent, tag);
}

/* helper function getting id out of argument */
function GetSourceId(arg) {
    var returnValue = "";

    if (arg.length > 0) {
        // parse string received from server
        var ci = arg.indexOf("|", 0);
        var ci2 = arg.indexOf("|", ci + 1);
        var ci3 = arg.indexOf("|", ci2 + 1);
        returnValue = arg.substring(ci2 + 1, ci3);
    }

    return returnValue;
}

function OnErr(arg, ctx) {

}

/* callback function, receives html of gridview from server and displays this on screen  */
function ReceiveServerData(arg, ctx) {

    if (arg.length > 0) {
        // parse string received from server
        var ci = arg.indexOf("|", 0);

        // parameter 'showPanel' not used at this moment
        var showPanel = arg.substring(0, ci);
        var ci2 = arg.indexOf("|", ci + 1);

        // selectedindex points at the currently selected row(number). 
        // we should try to scroll the panel in a way this row is 
        // at the top of the panel. -1 means we shouldn't try to scroll
        var selectedIndex = arg.substring(ci + 1, ci2);

        // parent is the div in which the panel is contained. Should be made visible
        var parent = document.getElementById(ctx).parentNode
        parent.style.display = '';
        parent.className = 'ind_Visible';

        // the panel itself should contain the grid received from server.
        // panel has a child div, which should be preserved (it contains an eventhandler for scrolling)
        var panel = document.getElementById(ctx);
        var targetDiv = panel.getElementsByTagName('div')[0];
        targetDiv.innerHTML = arg.substring(ci2 + 1);

        // do we have to scroll to the currently selected row?
        if (selectedIndex >= 0) {
            // try to determine row height
            var cellHeightHeader;
            var cellHeight;
            try {
                cellHeightHeader = targetDiv.getElementsByTagName('th')[0].clientHeight;
                cellHeight = targetDiv.getElementsByTagName('tr')[0].clientHeight;
            }
            catch (fout) {
                cellHeight = 22;
                cellHeightHeader = 0;
            }

            targetDiv.scrollTop = (selectedIndex * cellHeight) + cellHeightHeader;
        }
        else if (selectedIndex == -3 && typeof (_scrollTopBeforeMoreCallback) != 'undefined') {
            // we should restore the last known scroll position (because more was clicked)
            targetDiv.scrollTop = _scrollTopBeforeMoreCallback;
        }

        // now, we should check if automore is enabled, and also the bottom of the grid is reached/visible.
        // In that case we should callback for more already.
        var panelHeight = panel.clientHeight;
        var divHeight = targetDiv.scrollHeight;
        var scrollPos = targetDiv.scrollTop;

        if (scrollPos + panelHeight >= divHeight) {
            // bottom reached, check if there's a more row and activate it..
            var rows = panel.getElementsByTagName('tr');

            for (var i = rows.length - 1; i >= 0; i--) {
                // start at the end of the grid, highest chance more-row is there
                if (rows[i].className.indexOf('_mrMarker_') != -1 && rows[i].style.display != 'none') {
                    // apparantly auto-more is not active (there's a visible more row).
                    break;
                }
                if (rows[i].className.indexOf('_mrMarker_') != -1) {
                    // there's a non-visible auto-more row.
                    rows[i].onclick();
                    break;
                }
            }
        }

        ResetGlobals()
    }
}

Controls.ComboBox = function (element) {
    Controls.ComboBox.initializeBase(this, [element]);
    this._targetControl = null;
    this._confirmButton = null;
    this._button = null;
    this._panel = null;
    this._autoExpand = false;
    this._autoMore = false;
    this._autoFilter = false;
    this._cssClassGridView = '';
    this.PreviousValueTextBox = '';
    this.GridRowMarker = '_grMarker_';
    this.ResultCellMarker = '_rcMarker_';
    this.MoreRowMarker = '_mrMarker_';
    this._selectedText = '';
    this._lastScrollPos = 0;
    this._functionInQueue = null;
}

Controls.ComboBox.prototype = {
    initialize: function () {
        Controls.ComboBox.callBaseMethod(this, 'initialize');

        // handling click event for whole document, so panel will be hidden if user
        // clicks anywhere         
        this._clickDelegate = Function.createDelegate(this, this._click);
        $addHandler(document, 'click', this._clickDelegate);

        this._clickPanelDelegate = Function.createDelegate(this, this._ifNotVisibleMakeVisible);
        $addHandler(this.get_TargetControl(), 'click', this._clickPanelDelegate);

        // handling keydown event, will capture navigation keys (up, down, enter, esc, ...)
        // only active if cursor is on targetcontrol (button or textbox), the the panel or the
        // 'arrow button' in case of a combobox.
        this._pressDelegate = Function.createDelegate(this, this._keyPressed);
        $addHandler(this.get_TargetControl(), 'keydown', this._pressDelegate);

        $addHandler(this.get_Panel(), 'keydown', this._pressDelegate);
        if (this.get_TargetControl() != this.get_Button()) {
            $addHandler(this.get_Button(), 'keydown', this._pressDelegate);
        }

        // special handler on same controls for ENTER key (on key down is too early)
        this._enterDelegate = Function.createDelegate(this, this._enterPressed);
        $addHandler(this.get_TargetControl(), 'keypress', this._enterDelegate);
        $addHandler(this.get_Panel(), 'keypress', this._enterDelegate);
        if (this.get_TargetControl() != this.get_Button()) {
            $addHandler(this.get_Button(), 'keypress', this._enterDelegate);
        }

        // if text is entered in the textbox, we will process that
        this._keyUpDelegate = Function.createDelegate(this, this._keyUp);
        $addHandler(this.get_TargetControl(), 'keyup', this._keyUpDelegate);

        // if user hovers over items in the panel, they should get selected
        this._mouseDelegate = Function.createDelegate(this, this._mouseOver);
        $addHandler(this.get_Panel(), 'mouseover', this._mouseDelegate);

        // detect scrolling of panel (if 'AutoMore' is set), so we can load
        // new data if user reaches bottom of panel
        if (this.get_AutoMore()) {
            this._scrollDelegate = Function.createDelegate(this, this._panelScroll);
            var div = this.get_Panel().getElementsByTagName('div')[0];
            $addHandler(div, 'scroll', this._scrollDelegate);
        }

        // saving actual value of textbox, so we can check later if content changed
        this.PreviousValueTextBox = this.get_TargetControl().value;

        // textparam: this is a variabele which will be passed to the server function.
        // if empty, this means user hasn't changed the content of the textbox
        // if not empty, it contains the user-entered text in the textbox
        // SetTextParam('');

        ResetGlobals()
    },
    get_TargetControl: function () {
        return this._targetControl;
    },
    set_TargetControl: function (value) {
        this._targetControl = value;
    },
    get_ConfirmButton: function () {
        return this._confirmButton;
    },
    set_ConfirmButton: function (value) {
        this._confirmButton = value;
    },
    get_Button: function () {
        return this._button;
    },
    set_Button: function (value) {
        this._button = value;
    },
    get_Panel: function () {
        return this._panel;
    },
    set_Panel: function (value) {
        this._panel = value;
    },
    get_AutoExpand: function () {
        return this._autoExpand;
    },
    set_AutoExpand: function (value) {
        this._autoExpand = value;
    },
    get_CssClassGridViewSel: function () {
        return this.get_CssClassGridView() + "Selected";
    },
    get_CssClassGridView: function () {
        return this._cssClassGridView;
    },
    set_CssClassGridView: function (value) {
        this._cssClassGridView = value;
    },
    get_AutoMore: function () {
        return this._autoMore;
    },
    set_AutoMore: function (value) {
        this._autoMore = value;
    },
    get_AutoFilter: function () {
        return this._autoFilter;
    },
    set_AutoFilter: function (value) {
        this._autoFilter = value;
    },
    // a mouse click by the user causes the panel to disappear.
    // there are only two exceptions: if the user clicks/reaches the 'more data' entry in the grid,
    // or if the user clicks the button or textbox 'again'.
    // in that cases the panel should stay on screen, and if needed, a callback is done to retrieve more entries
    _click: function (e) {
        if (this._panel.parentNode.className == 'ind_Visible' && !_moreClicked && _notCollapseId != this.get_TargetControl().id) {
            this._panel.parentNode.style.display = 'none';
            this._panel.parentNode.className = '';
        }
        if (_notCollapseId == this.get_TargetControl().id) {
            _notCollapseId = '';
        }

        if (_ddButtonClicked) {
            // if the button from the combobox is clicked, we want the text in textbox selected
            //this.get_TargetControl().focus();
            //this.get_TargetControl().select();
            _ddButtonClicked = false;
        }
    },
    // if user hovers over the panel, the row he points at should be selected
    _mouseOver: function (e) {
        var newRow = getParentByTagName(e.target, 'tr');
        if (newRow
            && newRow.className.indexOf(this.GridRowMarker) != -1
            && newRow.className.indexOf(this.get_CssClassGridViewSel()) == -1) {
            // deselect old row
            this._determineSelectedRow();

            // select new row
            this._activateRow(newRow);
        }
    },
    _queue: function (func) {
        clearTimeout(this._functionInQueue);
        this._functionInQueue = setTimeout(func, 350);
    },
    // function is called is user presses (key up) some key (non-navigational) in textbox.
    // check if we should scroll to a new entry in list
    _keyUp: function (e) {
        if (e &&
            e.keyCode != 40 &&
            e.keyCode != 38 &&
            e.keyCode != 34 &&
            e.keyCode != 33 &&
            e.keyCode != 34 &&
            e.keyCode != 13 &&
            e.keyCode != 27 &&
            e.keyCode != 9) {
            if (this.PreviousValueTextBox != this.get_TargetControl().value) {
                this.PreviousValueTextBox = this.get_TargetControl().value;

                if (this.get_AutoFilter()) {
                    // only autofilter for non-numeric values OR if panel is already visible

                    var userInput = this.get_TargetControl().value;
                    SetTextParam(null, userInput);
                    if (this.get_Panel().parentNode.className != '' || !IsNumeric(userInput)) {
                        // query server with new filter!
                        this._queue(this.get_Button().onclick);
                        return;
                    }
                    else {
                        return;
                    }
                }

                if (this.get_TargetControl().value != '' && this.get_AutoExpand()) {
                    this._ifNotVisibleMakeVisible();
                }

                // if panel is visible at this point, we should select and scroll an entry
                if (this.get_Panel().parentNode.className != '') {

                    // scroll through all rows to find entry which should be selected
                    var rows = this.get_Panel().getElementsByTagName('tr');

                    if (rows.length < 1) return;
                    var rowToSelect = 0;
                    var matchingCharacters = 0;
                    var textToFind = this.get_TargetControl().value.toLowerCase();

                    for (var i = 0; i < rows.length; i++) {
                        var rowText = this._getResultTextFromRow(rows[i]);
                        var currentMatch = this._getNrOfMatchingChars(rowText, textToFind);

                        if (currentMatch > matchingCharacters) {
                            rowToSelect = i;
                            matchingCharacters = currentMatch;
                        }
                        if (currentMatch < matchingCharacters) {
                            // break;
                        }
                    }

                    // check if this new row isn't already selected
                    var newRow = rows[rowToSelect];
                    if (newRow.className.indexOf(this.get_CssClassGridViewSel()) == -1) {
                        // deselect old row
                        this._determineSelectedRow();

                        // select new row
                        this._activateRow(newRow);
                    }
                }
            }
        }
    },
    // helper function to determine how many characters in currentText exist in textToMatch
    _getNrOfMatchingChars: function (currentText, textToMatch) {
        var maxLength = currentText.length;
        if (textToMatch.length < maxLength) {
            maxLength = textToMatch.length;
        }
        var i;
        for (i = 0; i < maxLength; i++) {
            if (currentText[i] != textToMatch[i]) break;
        }
        return i;
    },
    // gets a text from column markes as 'resultcolumn' from a specific row
    _getResultTextFromRow: function (row) {
        var result = '';

        var cells = row.getElementsByTagName("td");

        for (var i = 0; i < cells.length; i++) {
            if (cells[i].className.indexOf(this.ResultCellMarker) != -1) {
                result = cells[i].innerText;
                break;
            }
        }

        return result;
    },
    _navigateBy: function (noOfRows) {
        this._ifNotVisibleMakeVisible();
        var row = this._determineSelectedRow();

        if (row == -1) {
            // no row selected, select first row
            row = this._determineFirstVisibleRow();
            if (row == -1) return;
        }

        while (!this._setSelectedRow(row + noOfRows)) {
            // apparently not possible to select this row, decrease noOfRows
            noOfRows = this._decrease(noOfRows);
        }
    },
    _decrease: function (nr) {
        if (nr > 0) {
            nr--;
        }
        else {
            nr++;
        }
        return nr;
    },
    _enterPressed: function (e) {
        if (e) {
            var code = e.keyCode || e.charCode;
            // enter
            if (code == 13) {
                if (this.get_Panel().parentNode.className != '') {
                    // in previous method '_selectedText' is set if a row with data is selected
                    // (more row has no data). This is the data from 'resultColumn'

                    var row = this._determineSelectedRow();
                    if (row >= 0) {
                        this.get_Panel().getElementsByTagName('tr')[row].onclick();
                    }
                    // only one 'onclick' should be executed. so if no 'row' is entered, we propagate this enter to the confirmbutton
                    else if (this.get_ConfirmButton() !== null) {
                        this.get_ConfirmButton().onclick();
                    }
                    // hide panel
                    this._click();
                }
                // panel is not expanded. User 'enters' value: submit.
                else if (this.get_ConfirmButton() !== null) {
                    this.get_ConfirmButton().onclick();
                }


                // enter key should not be propagated, because it will cause some other postback
                e.stopPropagation();
                e.preventDefault();
                return false;
            }
        }
    },
    // function is called if user presses (key down) some specific key in textbox
    // the combobox listens to down and up arrow, esc, tab, page up and pagedown.
    _keyPressed: function (e) {
        if (e) {
            // down arrow
            if (e.keyCode == 40) {
                this._navigateBy(1);
                return false;
            }
            // up arrow
            if (e.keyCode == 38) {
                this._navigateBy(-1);
                return false;
            }
            // page down
            if (e.keyCode == 34) {
                this._navigateBy(5);
                return false;
            }
            // page up
            if (e.keyCode == 33) {
                this._navigateBy(-5);
                return false;
            }
            // enter -> remove eventhandler from textbox (this is causing a canceled postback)
            if (e.keyCode == 13) {
                if (this.get_TargetControl() != null) {
                    this.get_TargetControl().onkeypress = null;
                    this.get_TargetControl().onchange = null;
                }
                return false;
            }

            // esc and tab
            if (e.keyCode == 27 || e.keyCode == 9) {
                // hide panel
                this._click(e);
                // return false;
            }
            else {
                // enable confirm button if some text is entered
                if (this.get_ConfirmButton() !== null) {
                    this.get_ConfirmButton().disabled = false;
                }
                $('.org-select-notification').css('visibility', 'visible');
                $('.org-select-error').css('visibility', 'hidden');
                $(this.get_TargetControl()).css('color', 'black');
            }
        }
    },
    // make panel visible, if it wasn't already
    _ifNotVisibleMakeVisible: function () {
        if (this.get_Panel().parentNode.className == '') {
            SetTextParam(null, this.get_TargetControl().value);
            this.get_Button().onclick();
        }
    },
    _determineFirstVisibleRow: function () {
        var rows = this.get_Panel().getElementsByTagName('tr');
        for (var i = 0; i < rows.length; i++) {
            if (rows[i].style.display != 'none') return i;
        }
        return -1;
    },
    // check all rows in the gridview, to see which one has selected status
    _determineSelectedRow: function () {
        var ret = -1;
        var rows = this.get_Panel().getElementsByTagName('tr');

        for (var i = 0; i < rows.length; i++) {
            if (rows[i].className.indexOf(this.get_CssClassGridViewSel()) != -1) {
                rows[i].className = rows[i].className.replace(this.get_CssClassGridViewSel(), '');
                ret = i;
                this._selectedText = this._getResultTextFromRow(rows[i]);
                break;
            }
        }
        return ret;
    },
    // sets selected status on a row (by rownumber) returns true if succesful,
    // false if newly selected row isn't visible..
    _setSelectedRow: function (rowNumber) {
        var rows = this.get_Panel().getElementsByTagName('tr');
        if (rows.length <= 0) return false;

        if (rowNumber < 0) rowNumber = 0;
        if (rowNumber >= rows.length) rowNumber = rows.length - 1;

        if (rows[rowNumber].style.display == 'none') return false;

        this._activateRow(rows[rowNumber]);
        return true;
    },
    // sets selected status on a row
    _activateRow: function (row) {
        if (row.className.indexOf(this.get_CssClassGridViewSel()) == -1) {
            row.className += ' ' + this.get_CssClassGridViewSel();

            // check if this row is visible
            this._scrollTo(row);
        }
    },
    // makes sure the given row is visible (scrolls panel as little as possible to make row visible)
    _scrollTo: function (row) {
        var div = this.get_Panel().getElementsByTagName('div')[0];
        var scrollPos = div.scrollTop;
        var panelHeight = this.get_Panel().clientHeight;
        var rowHeight = row.clientHeight;
        var rowTop = row.offsetTop;
        var rowBottom = rowTop + rowHeight;

        var visibleAreaTop = scrollPos;
        var visibleAreaBottom = scrollPos + panelHeight;

        if (rowBottom > visibleAreaBottom) {
            div.scrollTop += rowBottom - visibleAreaBottom;
        }
        else if (rowTop < visibleAreaTop) {
            div.scrollTop += rowTop - visibleAreaTop;
        }
    },
    // handles panel scroll event: if we reach the bottom of the panel, and there's a 'more' entry
    // we should request more items from server
    _panelScroll: function () {
        var div = this.get_Panel().getElementsByTagName('div')[0];
        var scrollPos = div.scrollTop;

        if (scrollPos != this._lastScrollPos) {
            this._lastScrollPos = scrollPos;

            var panelHeight = this.get_Panel().clientHeight;
            var divHeight = div.scrollHeight;

            if (scrollPos + panelHeight >= divHeight) {
                // bottom reached, check if there's a more row and activate it..
                var rows = this.get_Panel().getElementsByTagName('tr');

                for (var i = rows.length - 1; i >= 0; i--) {
                    // start at the end of the grid, highest chance more-row is there
                    if (rows[i].className.indexOf(this.MoreRowMarker) != -1) {
                        rows[i].onclick();
                        break;
                    }
                }
            }
        }
    },
    dispose: function () {
        //Add custom dispose actions here
        $removeHandler(document, 'click', this._clickDelegate);
        $removeHandler(this.get_TargetControl(), 'keydown', this._pressDelegate);
        $removeHandler(this.get_TargetControl(), 'click', this._clickPanelDelegate);
        $removeHandler(this.get_Panel(), 'keydown', this._pressDelegate);
        $removeHandler(this.get_Panel(), 'mouseover', this._mouseDelegate);
        if (this.get_TargetControl() != this.get_Button()) {
            $removeHandler(this.get_Button(), 'keydown', this._pressDelegate);
        }
        $removeHandler(this.get_TargetControl(), 'keyup', this._keyUpDelegate);
        if (this.get_AutoMore()) {
            var div = this.get_Panel().getElementsByTagName('div')[0];
            $removeHandler(div, 'scroll', this._scrollDelegate);
        }
        Controls.ComboBox.callBaseMethod(this, 'dispose');
    }
}
Controls.ComboBox.registerClass('Controls.ComboBox', Sys.UI.Control);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
