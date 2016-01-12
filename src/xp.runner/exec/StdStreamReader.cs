using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace Xp.Runners.Exec
{

    public class DataReceived : EventArgs
    {
        public string Data { get; set; }
    }

    /// Inspired by https://multipleinheritance.wordpress.com/2012/09/05/process-async-outputdatareceivederrordatareceived-has-a-flaw-when-dealing-with-prompts/
    public class StdStreamReader : IStdStreamReader
    {
        private static int bufferSize = 1024;
        private byte[] buffer = new byte[bufferSize];
        private StringBuilder queue = new StringBuilder();
        private ManualResetEvent done = new ManualResetEvent(false);
        private Encoding encoding;
        private object synchronization;

        /// <summary>Add an event to this reader</summary>
        public event EventHandler<DataReceived> DataReceivedEvent;

        /// <summary>Creates a new reader with a specified encoding and synchronization</summary>
        public StdStreamReader(Encoding encoding, object synchronization)
        {
            this.synchronization = synchronization;
            this.encoding = encoding;
        }

        /// <summary>Starts reading</summary>
        public async void Start(StreamReader reader)
        {
            var count = await reader.BaseStream.ReadAsync(buffer, 0, bufferSize);
            ReaderCallback(reader.BaseStream, count);
        }

        /// <summary>Wait until we've read until the end</summary>
        public bool WaitForEnd() { return done.WaitOne(); }

        public async void ReaderCallback(Stream stream, int count)
        {
            lock (synchronization)
            {
                if (count > 0)
                {
                    var bytes = encoding.GetString(buffer, 0, count);

                    lock (queue)
                    {
                        queue.Append(bytes);
                        if (DataReceivedEvent != null)
                        {
                            DataReceivedEvent(stream, new DataReceived { Data = queue.ToString() });
                            queue.Clear();
                        }
                    }
                }
                else
                {
                    done.Set();
                    return;
                }
            }

            count = await stream.ReadAsync(buffer, 0, bufferSize);
            ReaderCallback(stream, count);
        }
    }
}