using System.Collections.Generic;

namespace Cloudflare.Structs
{
    public class JsChallenge
    {
        public string ClassDefinition { get; set; }
        public string ClassName { get; set; }
        public string PropertyName { get; set; }

        public JsForm Form { get; set; }

        public string CfdnHidden { get; set; }

        public string Script { get; set; }

        public IEnumerable<string> Calculations { get; set; }

        public bool IsHostLength { get; set; }
        
        public int Delay { get; set; }
        
    }
}
