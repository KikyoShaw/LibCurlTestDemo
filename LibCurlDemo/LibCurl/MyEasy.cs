using System;
using System.Diagnostics;
using LibCurlDemo.LibCurl.Head;

namespace LibCurlDemo.LibCurl
{
    public class MyEasy : Easy
    {
        public MyEasy()
        {
        }

        //是否取消任务
        private bool _isCancel = false;

        /// <summary>
        /// 下载进度回调
        /// </summary>
        private Func<double, double, double, double, int> _processAct = null;

        public int GetProgressFunction(Object extraData, double dlTotal, double dlNow, double ulTotal, double ulNow)
        {
            if (_processAct == null)
                return 0;

            return _processAct.Invoke(dlTotal, dlNow, ulTotal, ulNow);
        }

        /// <summary>
        /// 写入文件回调
        /// </summary>
        private Func<byte[], int, int, int> _writeAct = null;

        public int WriteDataToFile(byte[] buf, int size, int nmemb, Object extraData)
        {
            if (_writeAct == null)
                return 0;

            // 检查是否需要取消下载任务
            if (_isCancel)
            {
                return (int)CURLcode.CURLE_ABORTED_BY_CALLBACK;
            }

            return _writeAct.Invoke(buf, size, nmemb);
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        public int DownLoadFile(string sUrl, int timeOut,
            Func<byte[], int, int, int> wFun,
            Func<double, double, double, double, int> pFun, 
            long rang = 0, long maxSpeed = 1 * 1024 * 1024)
        {
            _isCancel = false;
            int iCode = -1; // 自定义错误码-1
            try
            {
                _processAct = pFun;
                _writeAct = wFun;
                // 超时
                SetOpt(CURLoption.CURLOPT_NOSIGNAL, 1);
                SetOpt(CURLoption.CURLOPT_TIMEOUT, timeOut);
                // 忽略证书
                SetOpt(CURLoption.CURLOPT_SSL_VERIFYPEER, 0L);
                SetOpt(CURLoption.CURLOPT_SSL_VERIFYHOST, 0L);
                // 强制ipv4
                SetOpt(CURLoption.CURLOPT_IPRESOLVE, CURLipResolve.CURL_IPRESOLVE_V4);
                SetOpt(CURLoption.CURLOPT_FOLLOWLOCATION, 1);
                SetOpt(CURLoption.CURLOPT_URL, sUrl);
                //断点续传
                SetOpt(CURLoption.CURLOPT_RESUME_FROM, rang);
                // 限速 CURLOPT_MAX_RECV_SPEED_LARGE
                SetOpt((CURLoption)30146, maxSpeed);
                // 回调
                SetOpt(CURLoption.CURLOPT_WRITEFUNCTION, (Easy.WriteFunction)this.WriteDataToFile);
                SetOpt(CURLoption.CURLOPT_PROGRESSFUNCTION, (Easy.ProgressFunction)this.GetProgressFunction);
                var retCode = Perform();
                iCode = (int)retCode;
                if (retCode == CURLcode.CURLE_WRITE_ERROR)
                {
                    throw new Exception("下载已经被取消");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"DownLoadFile error, url = {sUrl}, error msg = {ex.Message}");
            }
            this.Dispose();
            return iCode;
        }

        public void PauseDownload()
        {
            _isCancel = true;
        }
    }
}
