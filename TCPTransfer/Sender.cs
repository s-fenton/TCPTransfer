using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

namespace TCPTransfer
{
    class Sender 
    {
        public Sender(ListBox filePaths, TcpClient c)
        {
            toSendFiles = filePaths;
            this.c = c;
            n = c.GetStream();
            fileCount = toSendFiles.Items.Count;
        }

        #region Data Members
        private  ListBox toSendFiles;
        private  int fileCount;
        private  TcpClient c;
        private  NetworkStream n;
        private  int chunkSize = 524288;
        #endregion

        private byte[] fileRead(byte[] container, int amount, FileStream fs)
        {
            int bytesRead = 0;
            int bytesToRead = amount;

            while (bytesRead < bytesToRead)
            {
                int bytesJustRead = fs.Read(container, bytesRead, bytesToRead);
     
                bytesRead += bytesJustRead;
                bytesToRead -= bytesJustRead;
            }

            return container;

        }

        public void sendFiles() // need to chunk the file sending.axd
        {
            n.WriteByte((byte)fileCount);

            for (int i = 0; i < fileCount; i++)
            {

                n.Write(getFileSize(i), 0, 1024);
                n.Write(getFileType(i), 0, 1024);
                n.Write(getFileName(i), 0, 1024);
                

                using (FileStream fs = new FileStream(toSendFiles.Items[i].ToString(), FileMode.Open))
                {
                    int fChunks = (int)fs.Length / chunkSize; // chunk size = 512kB.

                    int leftOver = (int)fs.Length % chunkSize; // any leftover data less than 1 chunk.
                    
                    for (int j = 0; j < fChunks; j++) // splits data into chunks, so doesnt crash with bigger files.
                    {
                        byte[] chunk = new byte[chunkSize];

                        chunk = fileRead(chunk, chunkSize, fs);

                        n.Write(chunk, 0, chunk.Length);
                    }

                    if (leftOver == fs.Length || leftOver != 0)
                    {

                        byte[] leftOverData = new byte[leftOver];

                        leftOverData = fileRead(leftOverData, leftOver, fs);

                        n.Write(leftOverData, 0, leftOver);
                    }
                }
                
            }

            toSendFiles.Items.Clear();

        }

        private byte[] getFileSize(int fileIndex)
        {
            string path = Convert.ToString(toSendFiles.Items[fileIndex]);
            byte[] old = Encoding.UTF32.GetBytes(Convert.ToString((Int32)new FileInfo(path).Length));
            byte[] ret = new byte[1024];
            old.CopyTo(ret, 0);
            for (int i = old.Length; i < 1024; i++)
            {
                ret[i] = 0x00; // add padding
            }

            return ret;
        }

        private byte[] getFileType(int fileIndex)
        {
            string path = Convert.ToString(toSendFiles.Items[fileIndex]);
            string ftype = Path.GetExtension(path).ToLowerInvariant();
            byte[] old = Encoding.UTF8.GetBytes(ftype);
            byte[] ret = new byte[1024];
            old.CopyTo(ret, 0);
            for (int i = old.Length; i < 1024; i++)
            {
                ret[i] = 0x00; // add padding
            }

            return ret;
        }

        private byte[] getFileName(int fileIndex)
        {
            string path = Convert.ToString(toSendFiles.Items[fileIndex]);
            string name = Path.GetFileNameWithoutExtension(path);
            byte[] old = Encoding.UTF8.GetBytes(name);
            byte[] ret = new byte[1024];
            old.CopyTo(ret, 0);

            for (int i = old.Length; i < 1024; i++)
            {
                ret[i] = 0x00; // add padding
            }
            return ret;
        }

    }
}
