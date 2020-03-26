using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace AutoCode
{
    /// <summary>
    /// 
    /// ErrorLog 的摘要说明
    /// 
    /// 程序功能 : 通过错误捕捉,将错误以TXT文本记录到站点根目录下的[ErrorReport]目录.
    /// 
    /// 其他功能 : 当日志文件大于3*1024千字节时将自动以序列方式备份日志.
    /// 
    /// 注意内容 : 日志文件是存放在站点[绝对]根目录的[ErrorReport]目录.
    /// </summary>
    public class ErrorLog
    {

        /// <summary>
        /// 将错误以文本的形式记录下来.
        /// </summary>
        /// <param name="Ex">捕获的异常.</param>
        public static void WriteLog(Exception Ex)
        {
            try
            {
                string ErrTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string ErrSource = Ex.Source;
                string ErrTargetSite = Ex.TargetSite.ToString();
                string ErrMsg = Ex.Message;
                string ErrStackTrace = Ex.StackTrace;

                string webPath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                webPath = System.IO.Path.GetDirectoryName(webPath);
                string FilePath = webPath.Substring(6) + "\\ErrorReport\\";


                if (!Directory.Exists(FilePath))
                {
                    Directory.CreateDirectory(FilePath);
                }
                string FileName = FilePath + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                if (GetFileSize(FileName) > 1024 * 3)
                {
                    CopyToBak(FileName);
                }
                StreamWriter MySw = new StreamWriter(FileName, true);
                MySw.WriteLine("错误时间 : " + ErrTime);
                MySw.WriteLine("错误对象 : " + ErrSource);
                MySw.WriteLine("异常方法 : " + ErrTargetSite);
                MySw.WriteLine("错误信息 : " + ErrMsg);
                MySw.WriteLine("堆栈内容 : ");
                MySw.WriteLine(ErrStackTrace);
                MySw.WriteLine("\r\n**********************************\r\n");
                MySw.Close();
            }
            catch (Exception ex)
            {

            }
            //MySw.Dispose();

        }



        /// <summary>
        /// 将错误以文本的形式记录下来.
        /// </summary>
        /// <param name="Ex">捕获的异常.</param>
        public static void WriteLogCan(string Ex)
        {
            try
            {
                string ErrTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                string webPath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                webPath = System.IO.Path.GetDirectoryName(webPath);
                string FilePath = webPath.Substring(6) + "\\ErrorReport\\CAN\\";
                if (!Directory.Exists(FilePath))
                {
                    Directory.CreateDirectory(FilePath);
                }
                string FileName = FilePath + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                if (GetFileSize(FileName) > 1024 * 3)
                {
                    CopyToBak(FileName);
                }
                StreamWriter MySw = new StreamWriter(FileName, true);
                MySw.WriteLine("报文时间 : " + ErrTime);
                MySw.WriteLine("报文内容 : " + Ex);
                MySw.WriteLine("\r\n***************** *****************\r\n");
                MySw.Close();
            }
            catch (Exception ex)
            {

            }
            //MySw.Dispose();

        }

        /// <summary>
        /// 将错误以文本的形式记录下来.
        /// </summary>
        /// <param name="Ex">捕获的异常.</param>
        public static void WriteLogSocket(string Ex)
        {
            try
            {
                string ErrTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                string webPath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                webPath = System.IO.Path.GetDirectoryName(webPath);
                string FilePath = webPath.Substring(6) + "\\ErrorReport\\Socket\\";
                if (!Directory.Exists(FilePath))
                {
                    Directory.CreateDirectory(FilePath);
                }
                string FileName = FilePath + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                if (GetFileSize(FileName) > 1024 * 3)
                {
                    CopyToBak(FileName);
                }
                StreamWriter MySw = new StreamWriter(FileName, true);
                MySw.WriteLine("报文时间 : " + ErrTime);
                MySw.WriteLine("报文内容 : " + Ex);
                MySw.WriteLine("\r\n***************** *****************\r\n");
                MySw.Close();
            }
            catch (Exception ex)
            {

            }
            //MySw.Dispose();

        }
        private static long GetFileSize(string FileName)
        {
            long strRe = 0;
            if (File.Exists(FileName))
            {
                FileStream MyFs = new FileStream(FileName, FileMode.Open);
                strRe = MyFs.Length / 1024;
                MyFs.Close();
                MyFs.Dispose();
            }
            return strRe;
        }

        private static void CopyToBak(string sFileName)
        {
            int FileCount = 0;
            string sBakName = "";
            do
            {
                FileCount++;
                sBakName = sFileName + "." + FileCount.ToString() + ".BAK";
            }
            while (File.Exists(sBakName));
            File.Copy(sFileName, sBakName);
            File.Delete(sFileName);
        }
    }
}
