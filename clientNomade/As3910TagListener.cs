using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;
using System.Threading;

namespace clientNomade
{
    class As3910TagListener
    {
        const int BAUD_RATE = 38400;
        const int SLEEP_TIME = 100;

        public event EventHandler<NewTagEventArgs> NewTagEvent;

        SerialPort serialPort;
        Thread tagRequestThread;
        Boolean ShouldStop;

        public As3910TagListener(String portName)
        {
            this.serialPort = new SerialPort(portName, BAUD_RATE);
            this.serialPort.Open();

            this.ShouldStop = false;
            this.tagRequestThread = new Thread(this.TagRequestThreadMethod);
            this.tagRequestThread.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data">data to send as an hexadecimal string</param>
        private void Send(string data)
        {
            int length = data.Length / 2 + 7;

            this.serialPort.Write("01");
            this.serialPort.Write(length.ToString("X2"));
            this.serialPort.Write("000304");
            this.serialPort.Write(data);
            this.serialPort.Write("0000");
        }

        private As3910Answer SendComamand(string command)
        {
            this.Send(command);
            return this.ReadAnswer();
        }

        private void OnNewTag(string tag)
        {
            // from https://msdn.microsoft.com/en-us/library/w369ty8x.aspx
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<NewTagEventArgs> handler = NewTagEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                NewTagEventArgs e = new NewTagEventArgs(tag);
                handler(this, e);
            }
        }

        private string ReadUntil(char end)
        {
            string result = "";
            int b = this.serialPort.ReadByte();
            if (b == -1)
            {
                return null;
            }

            while (b != end)
            {
                result += (char)b;

                b = this.serialPort.ReadByte();
                if (b == -1)
                {
                    return null;
                }
            }

            return result;

        }

        private As3910Answer ReadAnswer()
        {

            List<String> values = new List<string>();

            int b = this.serialPort.ReadByte();
            if (b == -1)
            {
                return null;
            }

            // skip everything until the first value or the code
            while (!"{[(".Contains((char)b))
            {
                b = this.serialPort.ReadByte();
                if (b == -1)
                {
                    return null;
                }
            }

            while ("[(".Contains((char)b))
            {
                char end;
                if (b == '[')
                {
                    end = ']';
                }
                else
                {
                    end = ')';
                }
                values.Add(this.ReadUntil(end));

                b = this.serialPort.ReadByte();
                if (b == -1)
                {
                    return null;
                }

            }

            if (b == '{')
            {
                string code_string = this.ReadUntil('}');
                int code = int.Parse(code_string);
                return new As3910Answer(code, values);

            }
            else
            {
                return null;
            }


        }

        private string RequestTag()
        {

            this.SendComamand("A0");
            As3910Answer answer = this.SendComamand("A126");
            if (answer.Code == 0 && answer.Values.Count == 6)
            {
                return answer.Values[4];
            }
            else
            {
                return null;
            };
        }

        private void TagRequestThreadMethod()
        {

            string previous_tag = null;

            while (!this.ShouldStop)
            {
                string tag = this.RequestTag();
                if (tag != null && tag != previous_tag)
                {
                    this.OnNewTag(tag);
                }

                previous_tag = tag;
                Thread.Sleep(SLEEP_TIME);
            }

        }

        public void CloseThread() {
            this.ShouldStop = true;
        }
    }
}
