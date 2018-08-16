using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSwiftClient.Demo.AspNetCore.ViewModels
{
    public class Checkbox3Model: BaseFieldModel
    {
        public override string Id { get; set; } = Guid.NewGuid().ToString(); // Default value that can be customized
        public bool IsChecked { get; set; }
        //public bool Value { get; set; } = true;
        public string Type { get; set; } = "checkbox";
        public static Checkbox3Model FromNameLabel(string name, string label, bool isChecked)
            => new Checkbox3Model()
            {
                Name = name,
                Label = label,
                IsChecked = isChecked
            };
    }
}
