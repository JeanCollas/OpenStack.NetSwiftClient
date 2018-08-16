using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSwiftClient.Demo.AspNetCore.ViewModels
{
    public class TextAreaModel : BaseFieldModel
    {
        public string Value { get; set; }
        public string PlaceHolder { get; set; }
        public static TextAreaModel FromNameLabel(string name, string label, string value = null, string placeHolder = null)
        {
            var tm = new TextAreaModel()
            {
                Name = name,
                Label = label,
            };
            if (!value.IsNullOrEmpty()) tm.Value = value;
            if (!placeHolder.IsNullOrEmpty()) tm.PlaceHolder = placeHolder;
            return tm;
        }

    }
}
