﻿using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;

using System.Web.Security;
using System.Web.UI.HtmlControls;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

using System.Data.SqlClient;

namespace suidaozhaoming
{
    public partial class Draw4 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            this.lbTt.Text = null;
            this.SetBind();
        }

        private void SetBind()
        {
            int countconst = 30;
            string day1 = null;
            string day2 = null;
            switch (Session["suidaocode"].ToString())
            {
                case "0100000000000":
                    if (Session["IntoCode"].ToString() == "Come from Button EnergyH")
                    {
                        String sqlconnection1 = ConfigurationManager.ConnectionStrings["suidaozhaomingConnectionString3"].ToString();
                        String Sqlempinfo = "select top(1) starttime,endtime from LightParam where ( cuid = 'CC6012070168')and (code = 'Energy') order by id desc";
                        SqlConnection conn1 = new SqlConnection(sqlconnection1);
                        SqlCommand lightinfo = new SqlCommand(Sqlempinfo, conn1);
                        conn1.Open();
                        SqlDataReader light_info = lightinfo.ExecuteReader();
                        if (light_info.Read())
                        {
                            day1 = light_info["starttime"].ToString();  // DateTime.Now.ToString("yyyy-MM-dd") + " 05:00:00";
                            day2 = light_info["endtime"].ToString();    // DateTime.Now.ToString("yyyy-MM-dd") + " 20:00:00";
                        }
                        conn1.Close();
                    }
                    else if (Session["IntoCode"].ToString() == "Come from Button EnergyDay")
                    {
                        String sqlconnection1 = ConfigurationManager.ConnectionStrings["suidaozhaomingConnectionString3"].ToString();
                        String Sqlempinfo = "select top(1) starttime,endtime from LightParam where ( cuid = 'CC6012070168')and (code = 'EnergyDay') order by id desc";
                        SqlConnection conn1 = new SqlConnection(sqlconnection1);
                        SqlCommand lightinfo = new SqlCommand(Sqlempinfo, conn1);
                        conn1.Open();
                        SqlDataReader light_info = lightinfo.ExecuteReader();
                        if (light_info.Read())
                        {
                            day1 = light_info["starttime"].ToString();  // DateTime.Now.ToString("yyyy-MM-dd") + " 05:00:00";
                            day2 = light_info["endtime"].ToString();    // DateTime.Now.ToString("yyyy-MM-dd") + " 20:00:00";
                        }
                        conn1.Close();
                    }
                    break;
                case "0200000000000":
                    break;
                case "0300000000000":
                    break;
                default:
                    break;
            }
            switch (Session["suidaocode"].ToString())
            {
                case "0100000000000":
                    String sqlconnection = ConfigurationManager.ConnectionStrings["suidaozhaomingConnectionString3"].ToString();
                    String SqlLightDisData = null;
                    if (Session["IntoCode"].ToString() == "Come from Button EnergyH")
                    {
                        SqlLightDisData = "select Energy_Hour,dbTime from Energy_Hour where((UID = 'CC6012070168') and (dbTime between '" + day1 + "' and '" + day2 + "')) order by dbTime desc";
                        this.lbTt.Text = "每小时";
                    }
                    else if (Session["IntoCode"].ToString() == "Come from Button EnergyDay")
                    {
                        SqlLightDisData = "select Energy_Hour,dbTime from Energy_Hour where((UID = 'CC6012070168') and (dbTime between '" + day1 + "' and '" + day2 + "'))and (CONVERT(varchar(2), dbTime, 108) >= 23 ) order by dbTime desc";
                        this.lbTt.Text = "每天";
                    }
                    SqlConnection conn = new SqlConnection(sqlconnection);
                    SqlDataAdapter MyAdapter = new SqlDataAdapter(SqlLightDisData, conn);
                    try
                    {
                        //初始化所建表的SQL语句,属于新类别
                        //this.lbTt.Text = null;
                        DataSet ds = new DataSet();
                        MyAdapter.Fill(ds, "EnergyH");
                        if (ds.Tables[0].Rows.Count > 1)
                        {
                            draw(this, ds, countconst, this.lbTt.Text);
                        }
                        else
                        {
                            this.lbTt.Text = "用电量";
                            drawtext(this, countconst, this.lbTt.Text);
                        }
                        conn.Close();
                    }
                    catch (Exception exc)
                    {
                        conn.Close();
                        string s = exc.ToString();
                    }
                    
                    break;
                case "0200000000000":
                    this.lbTt.Text = "用电量";
                    drawtext(this, countconst, this.lbTt.Text);
                    break;
                case "0300000000000":
                    this.lbTt.Text = "用电量";
                    drawtext(this, countconst, this.lbTt.Text);
                    break;
                default:
                    break;
            }




        }

        public static void draw(Page page, DataSet ds, int Tnum, string strName)
        {
            try
            {
                //取得记录数量
                int count = ds.Tables[0].Rows.Count;
                //Double i_num = 1;
                int countMax = count;
                if (count > Tnum)
                {
                    //i_num = Convert.ToDouble(count) / Tnum;
                    count = Tnum;

                }
                //记算图表宽度
                int wd = 80 + 20 * (count - 1);
                //设置最小宽度为800
                if (wd < 800) wd = 800;
                //设置高度为400
                int hd = 400;
                //生成Bitmap对像
                Bitmap img = new Bitmap(wd, hd);
                //生成绘图对像
                Graphics g = Graphics.FromImage(img);
                //定义黑色画笔
                Pen Bp = new Pen(Color.Black);
                //定义红色画笔
                Pen Rp = new Pen(Color.Red);
                //定义银灰色画笔
                Pen Sp = new Pen(Color.Silver);
                //定义大标题字体
                Font Bfont = new Font("Arial", 12, FontStyle.Bold);
                //定义一般字体
                Font font = new Font("Arial", 6);
                //定义大点的字体
                Font Tfont = new Font("Arial", 9);
                //定义横坐标间隔，(最佳值是总宽度-留空宽度[左右侧都需要])/(记录数量-1)
                int xSpace = (wd - 100) / (count - 1);

                //定义纵坐标间隔,不能随便修改，跟高度和横坐标线的条数有关，最佳值=(绘图的高度-上面留空-下面留空)
                int ySpace = 12;
                //纵坐标最大值和间隔值
                int yMaxValue = 100;               //纵坐标标注最大值
                //绘制底色
                g.DrawRectangle(new Pen(Color.White, hd), 0, 0, img.Width, img.Height);
                //定义黑色过渡型笔刷
                LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, img.Width, img.Height), Color.Black, Color.Black, 1.2F, true);
                //定义蓝色过渡型笔刷
                LinearGradientBrush Bluebrush = new LinearGradientBrush(new Rectangle(0, 0, img.Width, img.Height), Color.Blue, Color.Blue, 1.2F, true);
                //定义绿色过渡型笔刷
                LinearGradientBrush Greenbrush = new LinearGradientBrush(new Rectangle(0, 0, img.Width, img.Height), Color.Green, Color.Green, 1.2F, true);
                //绘制大标题
                g.DrawString("用电量曲线图", Bfont, brush, 40, 5);
                //绘制信息简报
                //string info = " 亮度曲线图生成时间：" + DateTime.Now.ToString();
                string info = strName.Replace("'", "''").Trim() + "用电量曲线图生成时间：" + Convert.ToDateTime(ds.Tables[0].Rows[0]["dbtime"]).ToString() + "  记录数据：" + count;   // + "  当前探头：" + strName.Replace("'", "''").Trim() + "#"

                g.DrawString(info, Tfont, Bluebrush, 40, 25);
                //绘制图片边框
                g.DrawRectangle(Bp, 0, 0, img.Width - 1, img.Height - 1);
                //绘制竖坐标轴
                g.DrawLine(Bp, 40, 55, 40, 360);
                //绘制横坐标轴 x2的60是右侧空出部分
                g.DrawLine(Bp, 40, 360, 30 + xSpace * (count - 1), 360);
                //绘制竖坐标标题
                if (strName == "每小时")
                {
                    g.DrawString(Convert.ToDateTime(ds.Tables[0].Rows[Convert.ToByte(0)]["dbtime"]).ToString("dd") + "日每小时用电量", Tfont, brush, 5, 40);
                }
                else if (strName == "每天")
                {
                    g.DrawString(Convert.ToDateTime(ds.Tables[0].Rows[Convert.ToByte(0)]["dbtime"]).ToString("yyyy-MM") + "月每天用电量", Tfont, brush, 5, 40);

                }
                //绘制横坐标标题
                g.DrawString("测试时间", Tfont, brush, 40, 380);
                //绘制竖坐标线
                for (int i = 0; i < count; i++)
                {
                    g.DrawLine(Sp, 40 + xSpace * i, 60, 40 + xSpace * i, 360);
                }
                //绘制时间轴坐标标签
                for (int i = 0; i < count; i++)
                {
                    //string st = Convert.ToDateTime(dt[i, 1]).ToString("MM:dd");

                    string st = null;

                    if (strName == "每小时")
                    {
                        st = Convert.ToDateTime(ds.Tables[0].Rows[Convert.ToByte(i)]["dbtime"]).ToString("HH:mm"); //ds.Tables[0].Rows[i]["createtime"].ToString();        
                    }
                    else if (strName == "每天")
                    {
                        st = Convert.ToDateTime(ds.Tables[0].Rows[Convert.ToByte(i)]["dbtime"]).ToString("MM-dd"); //ds.Tables[0].Rows[i]["createtime"].ToString();        

                    }
                    g.DrawString(st, font, brush, 30 + xSpace *i, 370);


                }
                //绘制横坐标线
                for (int i = 0; i < 10; i++)
                {
                    g.DrawLine(Sp, 40, 60 + (360 / ySpace) * i, 40 + xSpace * (count - 1), 60 + (360 / ySpace) * i);
                    //横坐标轴的值间隔是最大值除以间隔数
                    int s = yMaxValue - i * (yMaxValue / 10);
                    //绘制发送量轴坐标标签
                    g.DrawString(s.ToString(), font, brush, 10, 60 + (360 / ySpace) * i);
                }
                //定义纵坐标单位数值=纵坐标最大值/标量最大值（300/30）
                int yAveValue = 360 / ySpace;
                //定义曲线转折点
                Point[] pEnergy = new Point[count];
                Double valueCar = 0;
                for (int i = 0; i < count; i++)
                {
                    pEnergy[i].X = 40 + xSpace * i;
                    valueCar = Convert.ToDouble(ds.Tables[0].Rows[i][0]);
                    pEnergy[i].Y = 360 - Convert.ToInt16(valueCar * 100 / (yAveValue));

                    //绘制用电量值记录点
                    g.DrawString(valueCar.ToString("F2"), font, Greenbrush, pEnergy[i].X, pEnergy[i].Y - 10);
                    g.DrawRectangle(Rp, pEnergy[i].X - 1, pEnergy[i].Y - 1, 2, 2);
                }
                //绘制折线图
                //g.DrawLines(Rp, p);
                //绘制曲线图
                //g.DrawCurve(Rp, p);
                //绘制自定义张力的曲线图（0.5F是张力值，默认就是这个值）
                g.DrawCurve(Rp, pEnergy, 0.5F);
                //当需要在一个图里绘制多条曲线的时候，就多定义个point数组，然后画出来就可以了。

                //保存绘制的图片
                MemoryStream stream = new MemoryStream();
                img.Save(stream, ImageFormat.Jpeg);
                //图片输出
                page.Response.Clear();
                page.Response.ContentType = "image/jpeg";
                page.Response.BinaryWrite(stream.ToArray());

            }
            catch (Exception exc)
            {
                string s = exc.ToString();
            }

        }

        public static void drawtext(Page page, int Tnum, string strName)
        {
            try
            {
                //取得记录数量
                int count = 30;
                //记算图表宽度
                int wd = 80 + 20 * (count - 1);
                //设置最小宽度为800
                if (wd < 800) wd = 800;
                //设置高度为400
                int hd = 400;
                //生成Bitmap对像
                Bitmap img = new Bitmap(wd, hd);
                //生成绘图对像
                Graphics g = Graphics.FromImage(img);
                //定义黑色画笔
                Pen Bp = new Pen(Color.Black);
                //定义红色画笔
                Pen Rp = new Pen(Color.Red);
                //定义银灰色画笔
                Pen Sp = new Pen(Color.Silver);
                //定义大标题字体
                Font Bfont = new Font("Arial", 12, FontStyle.Bold);
                //定义一般字体
                Font font = new Font("Arial", 6);
                //定义大点的字体
                Font Tfont = new Font("Arial", 9);
                //定义横坐标间隔，(最佳值是总宽度-留空宽度[左右侧都需要])/(记录数量-1)
                int xSpace = (wd - 100) / (count - 1);
                int ySpace = 12;
                //纵坐标最大值和间隔值
                int yMaxValue = 3900;            //纵坐标标注最大值
                //绘制底色
                g.DrawRectangle(new Pen(Color.White, 400), 0, 0, img.Width, img.Height);
                //定义黑色过渡型笔刷
                LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, img.Width, img.Height), Color.Black, Color.Black, 1.2F, true);
                //定义蓝色过渡型笔刷
                LinearGradientBrush Bluebrush = new LinearGradientBrush(new Rectangle(0, 0, img.Width, img.Height), Color.Blue, Color.Blue, 1.2F, true);
                //绘制大标题
                g.DrawString("用电量曲线图", Bfont, brush, 40, 5);
                //绘制信息简报
                string info = strName.Replace("'", "''").Trim() + "用电量数据尚未导入";
                g.DrawString(info, Tfont, Bluebrush, 40, 25);
                //绘制图片边框
                g.DrawRectangle(Bp, 0, 0, img.Width - 1, img.Height - 1);
                //绘制竖坐标轴
                g.DrawLine(Bp, 40, 55, 40, 360);
                //绘制横坐标轴 x2的60是右侧空出部分
                g.DrawLine(Bp, 40, 360, 60 + xSpace * (count - 2), 360);
                //绘制竖坐标标题
                g.DrawString("每小时用电量值", Tfont, brush, 5, 40);
                //绘制横坐标标题
                g.DrawString("测试时间", Tfont, brush, 40, 385);
                //绘制竖坐标线
                for (int i = 0; i < count; i++)
                {
                    g.DrawLine(Sp, 40 + xSpace * i, 60, 40 + xSpace * i, 360);
                }

                //绘制横坐标线
                for (int i = 0; i < 10; i++)
                {
                    g.DrawLine(Sp, 40, 60 + (360 / ySpace) * i, 40 + xSpace * (count - 1), 60 + (360 / ySpace) * i);
                    //横坐标轴的值间隔是最大值除以间隔数
                    int s = yMaxValue - i * (yMaxValue / 10);
                    //绘制发送量轴坐标标签
                    g.DrawString(s.ToString(), font, brush, 10, 60 + (360 / ySpace) * i);
                }

                //保存绘制的图片
                MemoryStream stream = new MemoryStream();
                img.Save(stream, ImageFormat.Jpeg);
                //图片输出
                page.Response.Clear();
                page.Response.ContentType = "image/jpeg";
                page.Response.BinaryWrite(stream.ToArray());

            }
            catch (Exception exc)
            {
                string s = exc.ToString();
            }

        }
    }
}