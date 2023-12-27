// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EasyDataRecorder.cs">
//   Copyright (c) 2016 Patrick Quirk
// 
//   This software is licensed as described in the file LICENSE.md, which you should have received as part 
//   of this distribution.
// 
//   You may opt to use, copy, modify, merge, publish, distribute and/or sell copies of this Software, and 
//   permit persons to whom the Software is furnished to do so, under the terms of the LICENSE.md file.
// 
//   This software is distributed on an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express 
//   or implied.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace LibCurlDemo.LibCurl.Head
{
    public class EasyDataRecorder
    {
        readonly List<byte> header = new List<byte>();
        readonly List<byte> written = new List<byte>();

        public IList<byte> Header => header;

        public string HeaderAsString => Header == null ? null : Encoding.UTF8.GetString(header.ToArray());

        public IList<byte> Written => written;

        public string WrittenAsString => Written == null ? null : Encoding.UTF8.GetString(written.ToArray());

        public int HandleHeader(byte[] buf, int size, int nmemb, object extradata)
        {
            int totalBytes = size * nmemb;
            byte[] temp = new byte[totalBytes];
            buf.CopyTo(temp, 0);
            header.AddRange(temp);
            return totalBytes;
        }

        public int HandleWrite(byte[] buf, int size, int nmemb, object extradata)
        {
            int totalBytes = size * nmemb;
            byte[] temp = new byte[totalBytes];
            buf.CopyTo(temp, 0);
            written.AddRange(temp);
            return totalBytes;
        }

        private System.IO.FileStream _fileStream = null;

        public void SetFileStream(System.IO.FileStream fileStream)
        {
            _fileStream = fileStream;
        }
        public int HandleWriteFile(byte[] buf, int size, int nmemb, object extradata)
        {
            int totalBytes = size * nmemb;
            byte[] temp = new byte[totalBytes];
            buf.CopyTo(temp, 0);
            _fileStream?.Write(temp, 0, totalBytes);
            return totalBytes;
        }

        private int _copyOffSet = 0;

        public int HandleRead(byte[] buf, int size, int nmemb, object extradata)
        {
            if (!(extradata is byte[] data)) return 0;

            var readBytes = size * nmemb;
            readBytes = Math.Min(readBytes, data.Length - _copyOffSet);

            Array.Copy(data, _copyOffSet, buf, 0, readBytes);
            _copyOffSet = readBytes;

            return readBytes;
        }
    }
}