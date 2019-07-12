using System.Collections.Generic;

namespace CloudflareSolverRe.Types.Javascript
{
    public struct JsScript
    {
        public string ClassName { get; set; }
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }   
        public IEnumerable<IJsCalculation> Calculations { get; set; }
        public bool IsHostLength { get; set; }
        public int Round { get; set; }
        public int Delay { get; set; }
    }
}
