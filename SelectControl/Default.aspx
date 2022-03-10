<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SelectControl._Default" %>

<%@ Register TagPrefix="cc1" Namespace="Controls" Assembly="Controls" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>Eurocalculator</h1>
        <p class="lead">hark maar binnen!</p>
    </div>

    <div class="row">
        <div class="col-md-4">
            <asp:UpdatePanel ID="upEmail" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
                <ContentTemplate>
                    <cc1:ComboBox ID="cbEmail" runat="server" DsObject="SelectControl.BusinessObjects.EmailAddressRepo"
                        Converter="SelectControl.BusinessObjects.EmailAddress" CssClass="select-context"
                        CssClassPanel="select-context-panel" OnSelectedItemChanged="cbEmail_SelectedItemChanged"
                        CssClassGridView="select-table" PanelWidth="500px" AutoExpand="true" AutoPostBack="false" Placeholder="enter an e-mail address here"
                        AutoFilter="true" MaxLength="150" ResultColumn="Name" NoResultsText="no e-mail address found" EmptyFilterNotAllowedText="start entering e-mail address for suggestions">
                    </cc1:ComboBox>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
    <br />
    <br />
    <asp:Button ID="Button1" runat="server" Text="Click here to see which one you selected" OnClick="Button1_Click" />
</asp:Content>

<div class="extra-groot">Welkom!</div>
<asp:Label ID="lbInstructie" runat="server" CssClass="extra-groot" Text="Welkom!" />