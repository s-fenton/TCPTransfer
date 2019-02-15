using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;


namespace TCPTransfer
{
    class Receiver
    {
        private TcpListener s;
        private string port;
        private Queue<byte[]> recieveList;
        private int chunkSize = 524288;

        public Receiver(IPAddress ip, string port, Queue<byte[]> recieveList)
        {
            this.s = new TcpListener(ip, Convert.ToInt32(port));
            this.port = port;
            this.recieveList = recieveList;
        }

        private byte[] fullRead(byte[] container, int amount, NetworkStream n)
        {
            int bytesRead = 0;
            int bytesToRead = amount;

            while (bytesRead < bytesToRead)
            {
                int bytesJustRead = n.Read(container, bytesRead, bytesToRead);
                bytesRead += bytesJustRead;
                bytesToRead -= bytesJustRead;
            }

            return container;
        }

        private byte[] metaDataRead(byte[] container, int amount, NetworkStream n)
        {
            int bytesRead = 0;
            int bytesToRead = amount;

            while (bytesRead < bytesToRead)
            {
                int bytesJustRead = n.Read(container, bytesRead, bytesToRead);
                bytesRead += bytesJustRead;
                bytesToRead -= bytesJustRead;
            }

            for (int i = bytesRead; i < amount - bytesRead; i++)
            {
                container[i] = 0x00; //add padding
            }

            return container;
        }

        public void listen()
        {
            while (true)
            {
                s.Start();
                TcpClient c = s.AcceptTcpClient();
                NetworkStream stream = c.GetStream();

                int fCount = stream.ReadByte(); // first byte is always file count according to the sending procedure.

                for (int i = 0; i < fCount; i++)
                {
                    try
                    {
                        byte[] sizeDataBuffer = new byte[1024]; // make buffers for byte meta data.
                        byte[] typeDataBuffer = new byte[1024];
                        byte[] nameDataBuffer = new byte[1024];

                        sizeDataBuffer = metaDataRead(sizeDataBuffer, 1024, stream);
                        typeDataBuffer = metaDataRead(typeDataBuffer, 1024, stream);
                        nameDataBuffer = metaDataRead(nameDataBuffer, 1024, stream);


                        string size = Encoding.UTF32.GetString(sizeDataBuffer, 0, sizeDataBuffer.Length).Replace("\0", String.Empty); // get file  size

                        int fChunks = Convert.ToInt32(size) / chunkSize; // chunk size = 512kB.
                        int leftOver = Convert.ToInt32(size) % chunkSize;

                        for (int j = 0; j < fChunks; j++) // splits data into chunks, so doesnt crash with bigger files.
                        {
                            
                            if (j == 0) // signals that this is a new file
                            {
                                recieveList.Enqueue(Encoding.UTF8.GetBytes("BEGINNEWFILE")); // denotes start of new file
                                recieveList.Enqueue(nameDataBuffer);
                                recieveList.Enqueue(typeDataBuffer);
                            }

                            byte[] chunk = new byte[chunkSize];

                            chunk = fullRead(chunk, chunkSize, stream);                            

                            recieveList.Enqueue(chunk);

                            if (recieveList.Count > 100000)
                            {
                                while (recieveList.Count > 1000)
                                {
                                    Thread.Sleep(50);
                                }

                            }
                        }

                        if (leftOver == Convert.ToInt32(size) || leftOver != 0) // if some left over data
                        {
                            if (fChunks == 0) // make sure to send meta data even if file is smaller than 1 chunk.
                            {
                                recieveList.Enqueue(Encoding.UTF8.GetBytes("BEGINNEWFILE")); // denotes start of new file
                                recieveList.Enqueue(nameDataBuffer);
                                recieveList.Enqueue(typeDataBuffer);
                            }

                            byte[] leftOverData = new byte[leftOver];

                            leftOverData = fullRead(leftOverData, leftOver, stream);

                            recieveList.Enqueue(leftOverData);

                            if (recieveList.Count > 100000)
                            {
                                while (recieveList.Count > 1000)
                                {
                                    Thread.Sleep(50);
                                }

                            }

                        }
                      
                    }
                    catch
                    {
                        s.Stop();
                    }
                }
                s.Stop();
            }
        } 
    }
}
