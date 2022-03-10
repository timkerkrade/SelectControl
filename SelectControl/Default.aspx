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
    Tarief per uur: <asp:TextBox ID="tbUurtarief" runat="server"></asp:TextBox>
    <asp:Button ID="Button1" runat="server" Text="GO!" OnClick="Button1_Click" />
    <br /><br />
    <b><asp:Label ID="lbResult" runat="server"></asp:Label></b>
</asp:Content>