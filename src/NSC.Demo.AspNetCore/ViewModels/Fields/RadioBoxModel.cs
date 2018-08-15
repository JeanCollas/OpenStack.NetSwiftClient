using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSwiftClient.Demo.AspNetCore.ViewModels
{
    public class RadioBoxModel: BaseFieldModel
    {
        public override string Id { get; set; }
        public string InputClass { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsChecked { get; set; }
        public string Value { get; set; }
    }
}
