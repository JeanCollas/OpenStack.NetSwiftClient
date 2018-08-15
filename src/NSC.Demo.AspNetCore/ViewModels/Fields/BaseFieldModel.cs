using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSwiftClient.Demo.AspNetCore.ViewModels
{
    public class BaseFieldModel
    {
        string _Id;
        string _Name;
        public virtual string Id { get => _Id.IfNullOrEmpty(Name); set => _Id = value; }
        public string Name { get => _Name.IfNullOrEmpty(_Id); set => _Name = value; }
        public string LabelKey { get; set; }
        public string Label { get; set; }
        public string Class { get; set; }
        public bool IsRequired { get; set; }
        public bool Inline { get; set; } = false;
        public int MaxLenght { get; set; } = int.MaxValue;
        public bool IsReadOnly { get; set; }
    }
}
