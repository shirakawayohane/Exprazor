using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exprazor.TSInterop.ConsoleSandbox
{
    public partial class TSInteropCheck : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            Add(1, 2);
            Sub(1, "hoge");
            throw new NotImplementedException();
        }
    }

    partial class TSInteropCheck
    {
        void hoge() {  }
    }
}
