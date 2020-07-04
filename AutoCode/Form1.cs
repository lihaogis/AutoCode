﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Npgsql;
using NpgsqlTypes;

namespace AutoCode
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        string connectString = "SERVER=127.0.0.1;PORT=5432;DATABASE=bimgis;USER ID=postgres;PASSWORD=meitu001";
        string dbType = "PostgreSQL";

        private void Form1_Load(object sender, EventArgs e)
        {
            Core.DBModel dbModel = Core.DBFactory.createDBModel(dbType, connectString);
            dbModel.GetStatue();
            dbModel.BeginTask();
        }

        /// <summary>
        /// 开始生成任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBeginTask_Click(object sender, EventArgs e)
        {
            if (connectString != "")
            {
                BeginTask(connectString);
            }
        }

        /// <summary>
        /// 测试数据库连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTestConnect_Click(object sender, EventArgs e)
        {
            if (txtIp.Text.Trim() != "" && txtPort.Text.Trim() != "" && txtBDName.Text.Trim() != "" && txtUserName.Text.Trim() != "" && txtUserPsw.Text.Trim() != "")
            {
                connectString = "SERVER=" + txtIp.Text.Trim() + ";PORT=" + txtPort.Text.Trim() + ";DATABASE=" + txtBDName.Text.Trim() + ";USER ID=" + txtUserName.Text.Trim() + ";PASSWORD=" + txtUserPsw.Text.Trim() + "";
                NpgsqlConnection connection = new NpgsqlConnection(connectString);
                try
                {
                    connection.Open();
                    MessageBox.Show("连接成功");
                }
                catch (Exception exp)
                {
                    MessageBox.Show(exp.Message.ToString());
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                MessageBox.Show("数据库参数不正确");
            }
        }

        /// <summary>
        /// 开始生成三层任务
        /// </summary>
        public void BeginTask(string connectionString)
        {
            //获取所有数据表
            string str_tableNames = "select tablename from pg_tables where schemaname='public'";
            NpgsqlConnection conn = new NpgsqlConnection(connectionString);
            NpgsqlDataAdapter ad = new NpgsqlDataAdapter(str_tableNames, conn);
        }

    }
}
