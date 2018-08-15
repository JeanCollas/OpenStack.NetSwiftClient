using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSwiftClient.Demo.AspNetCore.ViewModels
{
    public class TextAreaModel: BaseFieldModel
    {
        public string Value { get; set; }
        public string PlaceHolder { get; set; }
    }
}
