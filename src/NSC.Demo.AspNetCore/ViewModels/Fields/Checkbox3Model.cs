using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSwiftClient.Demo.AspNetCore.ViewModels
{
    public class Checkbox3Model: BaseFieldModel
    {
        public override string Id { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsChecked { get; set; }
        //public bool Value { get; set; } = true;
        public string Type { get; set; } = "checkbox";
    }
}
