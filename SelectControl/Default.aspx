<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SelectControl._Default" %>


<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>Eurocalculator</h1>
        <p class="lead">hark maar binnen!</p>
    </div>

    <div class="row">
        <div class="col-md-4">
            <asp:UpdatePanel ID="upEmail" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
                <ContentTemplate>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
    <br />
    <br />
    <asp:Button ID="Button1" runat="server" Text="Click here to see which one you selected" OnClick="Button1_Click" />
</asp:Content>