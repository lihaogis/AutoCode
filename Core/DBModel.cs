using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    public abstract class DBModel
    {
        string connectString;

        public string ConnectString
        {
            get { return connectString; }
            set { connectString = value; }
        }


        public virtual bool GetStatue()
        {
            return false;
        }


        public virtual void BeginTask()
        {

        }

    }
}
