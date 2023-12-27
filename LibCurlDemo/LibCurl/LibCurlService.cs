using System;
using System.Collections.Concurrent;
using LibCurlDemo.LibCurl.Head;

namespace LibCurlDemo.LibCurl
{
    public class LibCurlService : IDisposable
    {
        private static readonly Lazy<LibCurlService> LazyInstance = new Lazy<LibCurlService>();
        public static LibCurlService Instance => LazyInstance.Value;

        private readonly ConcurrentDictionary<string, MyEasy> _easyDict = new ConcurrentDictionary<string, MyEasy>();

        public LibCurlService()
        {
            Curl.GlobalInit((int)CURLinitFlag.CURL_GLOBAL_ALL);
        }

        ~LibCurlService()
        {
            Dispose();
        }

        public void Init(){}

        /// <summary>
        /// 下载对外接口
        /// </summary>
        public int DownLoadFile(string sUrl, int timeOut,
            Func<byte[], int, int, int> wFun,
            Func<double, double, double, double, int> pFun,
            long rang = 0, long maxSpeed = 1 * 1024 * 1024)
        {
            var easy = new MyEasy();
            _easyDict.TryAdd(sUrl, easy);
            var iCode = easy.DownLoadFile(sUrl, timeOut, wFun, pFun, rang, maxSpeed);
            _easyDict.TryRemove(sUrl, out _);
            return iCode;
        }

        /// <summary>
        /// 暂停文件下载
        /// </summary>
        public void PauseDownload(string sUrl)
        {
            if (!_easyDict.ContainsKey(sUrl))
                return;

            var easy = _easyDict[sUrl];
            if (easy == null)
                return;

            easy.PauseDownload();
        }

        public void Dispose()
        {
            Curl.GlobalCleanup();
        }

    }
}
