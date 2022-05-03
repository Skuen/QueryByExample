using DevExpress.XtraReports.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DevExpress.XtraReports.Web;

namespace TestProject.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            Dictionary<String, List<String>> TableDict = new Dictionary<String, List<String>>();
            List<String> TablesName = new List<String>();
            List<List<String>> Relations = new List<List<String>>();

            try
            {
                //
                string connectionString = "Data Source=DESKTOP-JTFS021;Initial Catalog=QLVT_DATHANG;User ID=sa;Password=123";
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand("select name FROM SYS.tables where name not in ('sysdiagrams')", connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            String temp = reader.GetString(0);
                            List<String> templist = getColFromTableName(temp);
                            TablesName.Add(temp);
                            TableDict.Add(temp, templist);
                        }
                    }
                }
                //
                Relations = getRelations();
                List<String> Cols = new List<string>();
                ViewBag.Tables = TablesName;
                ViewBag.TableDict = TableDict;
                ViewBag.Relations = Relations;
                //ViewBag.TableList = Tables;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi khi mo  ket noi:" + ex.Message);
            }
            return View();
        }
        public List<String> getColFromTableName(String name)
        {
            name = name.Trim();
            List<String> Cols = new List<string>();
            try
            {
                string connectionString = "Data Source=DESKTOP-JTFS021;Initial Catalog=QLVT_DATHANG;User ID=sa;Password=123";
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand("SELECT COLUMN_NAME FROM information_schema.columns WHERE table_name = '" + name + "'", connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            String temp = reader.GetString(0);
                            Cols.Add(temp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi khi mo  ket noi:" + ex.Message);
            }
            return Cols;
        }

        public List<List<String>> getRelations()
        {
            List<List<String>> Relations = new List<List<String>>();
            try
            {
                //
                string sql = @"SELECT  obj.name AS FK_NAME,
    tab1.name AS [table],
    col1.name AS [column],
    tab2.name AS [referenced_table],
    col2.name AS [referenced_column]
FROM sys.foreign_key_columns fkc
INNER JOIN sys.objects obj
    ON obj.object_id = fkc.constraint_object_id 
INNER JOIN sys.tables tab1
    ON tab1.object_id = fkc.parent_object_id
INNER JOIN sys.schemas sch
    ON tab1.schema_id = sch.schema_id
INNER JOIN sys.columns col1
    ON col1.column_id = parent_column_id AND col1.object_id = tab1.object_id
INNER JOIN sys.tables tab2
    ON tab2.object_id = fkc.referenced_object_id
INNER JOIN sys.columns col2
    ON col2.column_id = referenced_column_id AND col2.object_id = tab2.object_id";

                string connectionString = "Data Source=DESKTOP-JTFS021;Initial Catalog=QLVT_DATHANG;User ID=sa;Password=123";
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            List<String> items = new List<String>();
                            items.Add(reader.GetString(0));
                            items.Add(reader.GetString(1));
                            items.Add(reader.GetString(2));
                            items.Add(reader.GetString(3));
                            items.Add(reader.GetString(4));
                            Relations.Add(items);

                        }
                    }
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi khi mo  ket noi:" + ex.Message);
            }
            return Relations;
        }

        [HttpPost]
        public ActionResult Report(String AreaQuery, String RPTitle)
        {
            String qr = AreaQuery;
            String tit = RPTitle;
            XtraReport xtraRP = new XtraReport();
            SqlConnection cnn = new SqlConnection();
            SqlCommand cmd = new SqlCommand();

            try
            {


                String connect = "Data Source=DESKTOP-JTFS021;Initial Catalog=QLVT_DATHANG;User ID=sa;Password=123";
                cnn.ConnectionString = connect;
                cnn.Open();
                DataSet dt = new DataSet();

                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = new SqlCommand(qr, cnn);
                da.Fill(dt);
                xtraRP.DataSource = dt;
                InitBands(xtraRP);
                InitDetailsBaseXRTable(xtraRP, dt, tit);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi khi mo  ket noi:" + ex.Message);
            }
            return View(xtraRP);
        }
        
        public void InitBands(XtraReport rep)
        {
            DetailBand detail = new DetailBand();
            PageHeaderBand pageHeader = new PageHeaderBand();
            ReportHeaderBand reportHeader = new ReportHeaderBand();
            ReportFooterBand reportFooter = new ReportFooterBand();

            reportHeader.HeightF = 40;
            detail.HeightF = 20;
            reportFooter.HeightF = 380;
            pageHeader.HeightF = 20;
            rep.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] { reportHeader, detail, pageHeader, reportFooter });
        }
        public void InitDetailsBaseXRTable(XtraReport rep, DataSet ds, String tit)
        {
            ds = ((DataSet)rep.DataSource);
            int colCount = ds.Tables[0].Columns.Count;
            int colWidth = (rep.PageWidth - (rep.Margins.Left + rep.Margins.Right)) / colCount;
            rep.Margins = new System.Drawing.Printing.Margins(20, 20, 20, 20);
            XRLabel title = new XRLabel();
            title.Text = tit;
            title.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
            title.ForeColor = Color.Red;
            title.Font = new Font("Tahoma", 20, FontStyle.Bold, GraphicsUnit.Pixel);
            title.Width = Convert.ToInt32(rep.PageWidth - 50);


            // Create a table to represent headers
            XRTable tableHeader = new XRTable();
            tableHeader.Height = 40;
            tableHeader.BackColor = Color.PeachPuff;
            tableHeader.ForeColor = Color.Black;
            tableHeader.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            tableHeader.Font = new Font("Tahoma", 12, FontStyle.Bold, GraphicsUnit.Pixel);
            tableHeader.Width = (rep.PageWidth - (rep.Margins.Left + rep.Margins.Right));
            tableHeader.Padding = new DevExpress.XtraPrinting.PaddingInfo(5, 5, 5, 5, 100.0F);
            XRTableRow headerRow = new XRTableRow();
            headerRow.Width = tableHeader.Width;
            tableHeader.Rows.Add(headerRow);
            tableHeader.BeginInit();

            /*Create a table to display data*/
            XRTable tableDetail = new XRTable();
            tableDetail.Height = 20;
            tableDetail.Width = (rep.PageWidth - (rep.Margins.Left + rep.Margins.Right));
            tableDetail.Font = new Font("Tahoma", 12, FontStyle.Regular, GraphicsUnit.Pixel);
            XRTableRow detailRow = new XRTableRow();
            detailRow.Width = tableDetail.Width;
            tableDetail.Rows.Add(detailRow);
            tableDetail.Padding = new DevExpress.XtraPrinting.PaddingInfo(5, 5, 5, 5, 100.0F);
            tableDetail.BeginInit();
            tableDetail.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            tableHeader.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            /*Create table cells, fill the header cells with text, bind the cells to data*/
            for (int i = 0; i < colCount; i++)
            {
                XRTableCell headerCell = new XRTableCell();
                headerCell.Text = ds.Tables[0].Columns[i].Caption;
                XRTableCell detailCell = new XRTableCell();
                detailCell.DataBindings.Add("Text", null, ds.Tables[0].Columns[i].Caption);

                detailCell.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
                headerCell.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
                /*
                if (i == 0)
                {
                    headerCell.Borders = DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top | DevExpress.XtraPrinting.BorderSide.Bottom;
                    detailCell.Borders = DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top | DevExpress.XtraPrinting.BorderSide.Bottom;
                }
                else
                {
                    headerCell.Borders = DevExpress.XtraPrinting.BorderSide.All;
                    detailCell.Borders = DevExpress.XtraPrinting.BorderSide.All;
                }*/

                headerCell.Borders = DevExpress.XtraPrinting.BorderSide.All;
                detailCell.Borders = DevExpress.XtraPrinting.BorderSide.All;

                headerCell.Width = colWidth / colCount * (i + 1);
                detailCell.Width = colWidth / colCount * (i + 1);
                detailCell.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;

                detailCell.Borders = DevExpress.XtraPrinting.BorderSide.Bottom | DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Right;

                /*Place the cells into the corresponding tables*/
                headerRow.Cells.Add(headerCell);
                detailRow.Cells.Add(detailCell);
            }

            tableHeader.EndInit();
            tableDetail.EndInit();
            /*Place the table onto a report's Detail band*/
            rep.Bands[BandKind.ReportHeader].Controls.Add(title);
            rep.Bands[BandKind.PageHeader].Controls.Add(tableHeader);
            rep.Bands[BandKind.Detail].Controls.Add(tableDetail);
        }

    }
}