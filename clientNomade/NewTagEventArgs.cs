using System;

namespace clientNomade
{
    public class NewTagEventArgs : EventArgs
    {
        public NewTagEventArgs(string tag)
        {
            this.tag = tag;
        }
        private string tag;
        public string Tag
        {
            get { return tag; }
        }
    }
}
