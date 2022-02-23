using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Data.SqlClient;
using System.Data;
using System.Xml;
using System.Timers;
using System.Text.RegularExpressions;
// added for access to RegistryKey
using Microsoft.Win32;
// added for access to socket classes
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.ComponentModel;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace FileService
{
    public class DatabaseUtility
    {
        public static int RegisterFile(String fileName, String selector, int filenum)
        {
            int affectedRows = -1;

            using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[selector].ToString()))
            {
                SqlCommand sqlCommand = new SqlCommand("proc_RegisterRawDataFile", con);
                con.Open();
                sqlCommand.CommandTimeout = 0;

                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@FileName", fileName);
                sqlCommand.Parameters.AddWithValue("@FileTypeNum", filenum);

                affectedRows = sqlCommand.ExecuteNonQuery();
            }
            return affectedRows;
        }

        public static void DeleteFileRegisteration(String selector)
        {
            using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[selector].ToString()))
            {
                SqlCommand sqlCommand = new SqlCommand("proc_DeleteAllFileImportLogs_Admin", con);
                con.Open();
                sqlCommand.CommandTimeout = 0;
                sqlCommand.CommandType = CommandType.StoredProcedure;
                int affectedRows = sqlCommand.ExecuteNonQuery();
            }
        }

        public static int GetRowCount(String selector, string sqlQry)
        {
            int affectedRows = -1;

            try
            {
                using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[selector].ToString()))
                {
                    SqlCommand sqlCommand = new SqlCommand(sqlQry, con);
                    con.Open();
                    sqlCommand.CommandTimeout = 0;
                    sqlCommand.CommandType = CommandType.Text;
                    affectedRows = (int)sqlCommand.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return affectedRows;
        }

        public static void ExecuteNonQuery(String selector, string sqlQry)
        {
            using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[selector].ToString()))
            {
                SqlCommand sqlCommand = new SqlCommand(sqlQry, con);
                con.Open();
                sqlCommand.CommandTimeout = 0;
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.ExecuteNonQuery();
            }
        }
    }
}
