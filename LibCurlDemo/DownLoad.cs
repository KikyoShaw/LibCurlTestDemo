using System;
using System.Diagnostics;
using LibCurlDemo.LibCurl;
using System.IO;
using System.Threading.Tasks;
using LibCurlDemo.LibCurl.Head;

namespace LibCurlDemo
{
    public class DownLoad
    {
        private string _downloadUrl = "";
        private string _downloadFileName = "";
        private string _downloadFileTemp = "";
        private string _downloadPackPath = "";

        //下载重试次数
        private int _retryCount = 3;

        //是否取消下载
        private bool _isCancel = false;

        /// <summary>
        /// 下载文件信息
        /// </summary>
        private void MakeupPathInfo(string url, string filename)
        {
            try
            {
                //  若传过来是空的字符串，取下载地址的
                if (string.IsNullOrEmpty(filename))
                {
                    filename = new FileInfo(_downloadUrl).Name;
                }

                if (!Path.IsPathFullyQualified(filename))
                {
                    filename = Path.Combine(Path.GetTempPath(), filename);
                }

                string md5Name = Helper.GetMd5Hash(url);
                string dirPath = Path.GetDirectoryName(filename);

                _downloadUrl = url;
                _downloadFileName = filename;
                _downloadFileTemp = Path.Combine(dirPath, md5Name + ".tmp");
                _downloadPackPath = Path.Combine(dirPath, md5Name + ".ts");
            }
            catch { }
        }

        /// <summary>
        /// 断点续传信息
        /// </summary>
        /// <returns></returns>
        public long IsResumeSupport()
        {
            long lResultLength = 0;

            if (File.Exists(_downloadFileTemp))
            {
                lResultLength = new FileInfo(_downloadFileTemp).Length;
            }
            return lResultLength;
        }

        public async Task<bool> Download(string url, string filename)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _isCancel = false;
                    MakeupPathInfo(url, filename);
                    //断点续传
                    long lFileExistLength = IsResumeSupport();

                    using FileStream fileStream = new FileStream(_downloadFileTemp, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

                    if (lFileExistLength > 0)
                        fileStream.Seek(lFileExistLength, SeekOrigin.Begin);

                    Func<byte[], int, int, int> wFun = (buf, size, nmemb) => {
                        try
                        {
                            fileStream.Write(buf, 0, nmemb);
                            Trace.WriteLine($"libCurl wFun, size:{size}, nmemb:{nmemb}");
                        }
                        catch (Exception ex)
                        {
                            return 0;
                        }
                        return nmemb;
                    };

                    Func<double, double, double, double, int> pFun = (dlTotal, dlNow, ulTotal, ulNow) => {
                        try
                        {
                            if (dlTotal <= 0)
                                return 0;

                            //下载进度
                            var lNow = (long)dlNow + lFileExistLength;
                            var lTotal = (long)dlTotal + lFileExistLength;

                            Trace.WriteLine($"libCurl pFun, lTotal:{lTotal}, lNow:{lNow}");
                        }
                        catch (Exception ex)
                        {
                            return 1;
                        }
                        return 0;
                    };

                    bool bRet = false;
                    while (true)
                    {
                        var iCode = LibCurlService.Instance.DownLoadFile(url, 60, wFun, pFun, lFileExistLength);
                        bRet = iCode == 0;
                        //成功或者取消都不进入重试
                        if (bRet || iCode == (int)CURLcode.CURLE_WRITE_ERROR)
                            break;
                        _retryCount--;
                        if (_retryCount <= 0)
                            break;
                    }
                    fileStream?.Close();
                    if (bRet)
                    { 
                        MergeFile(); //合并文件
                    }
                    else
                    {
                        if (_isCancel) //如果是取消下载，删除临时文件
                        {
                            //  删除文件
                            File.Delete(_downloadFileTemp);
                            File.Delete(_downloadPackPath);
                        }
                    }
                    return bRet;
                }
                catch /*(Exception ex)*/
                {
                    
                }

                return false;
            });
        }

        /// <summary>
        /// 合并文件
        /// </summary>
        private void MergeFile()
        {
            try
            {
                if (File.Exists(_downloadFileTemp))
                {
                    File.Move(_downloadFileTemp, _downloadFileName, true);
                }
                File.Delete(_downloadPackPath);
            }
            catch /*(Exception ex)*/ { }
        }

        /// <summary>
        /// 取消下载
        /// </summary>
        public void Cancel()
        {
            LibCurlService.Instance.PauseDownload(_downloadUrl);
            _isCancel = true;
        }

        /// <summary>
        /// 暂停下载
        /// </summary>
        public void Pause()
        {
            LibCurlService.Instance.PauseDownload(_downloadUrl);
        }
    }
}
