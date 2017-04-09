using System;
using System.Collections.Generic;

namespace clientNomade
{
    class As3910Answer
    {
        int code;
        List<String> values;

        public As3910Answer(int code, List<String> values)
        {
            this.Code = code;
            this.Values = values;   
        }

        public int Code { get => code; set => code = value; }
        public List<string> Values { get => values; set => values = value; }

        public override string ToString()
        {
            return String.Format("As3910Answer({0}, [{1}])", this.Code, string.Join(", ", this.Values));
        }

    }
}
