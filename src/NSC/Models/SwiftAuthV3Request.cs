using System;
using System.Collections.Generic;
using System.Text;

namespace NetSwiftClient.Models
{
    public class SwiftAuthV3Request
    {
        public SwiftAuthV3Request(string name, string password, string domain = "Default", IScopeDescriptorObject scope = null)
        {
            Auth = new AuthObject()
            {
                Identity = new IdentityObject()
                {
                    Methods = new List<string>() { "password" },
                    Password = new PasswordObject()
                    {
                        User = new UserObject()
                        {
                            Name = name,
                            Password = password,
                            Domain = new DomainObject() { Name = domain }
                        }
                    }
                },
                Scope = new ScopeObject(scope)
            };
        }

        public SwiftAuthV3Request(string name, string password, string domain = "Default")
        {
            Auth = new AuthObject()
            {
                Identity = new IdentityObject()
                {
                    Methods = new List<string>() { "password" },
                    Password = new PasswordObject()
                    {
                        User = new UserObject()
                        {
                            Name = name,
                            Password = password,
                            Domain = new DomainObject() { Name = domain }
                        }
                    }
                }
            };
        }

        public SwiftAuthV3Request(string token)
        {
            Auth = new AuthObject()
            {
                Identity = new IdentityObject()
                {
                    Methods = new List<string>() { "token" },
                    Token = new TokenObject()
                    {
                        Id = token
                    }
                }
            };
        }

        public AuthObject Auth { get; set; }

        public class AuthObject
        {
            public IdentityObject Identity { get; set; }
            public ScopeObject Scope { get; set; }
        }
        public class IdentityObject
        {
            public List<string> Methods { get; set; } = new List<string>();
            public PasswordObject Password { get; set; }
            public TokenObject Token { get; set; }
        }
        public class PasswordObject
        {
            public UserObject User { get; set; }
        }
        public class TokenObject
        {
            public string Id { get; set; }
        }
        public class UserObject
        {
            public string Name { get; set; }
            public string Password { get; set; }
            public DomainObject Domain { get; set; }

        }
        public class DomainObject : IScopeDescriptorObject
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public class ScopeObject
        {
            public ScopeObject(IScopeDescriptorObject scope = null)
            {
                System = scope as SystemObject;
                Project = scope as ProjectObject;
                Domain = scope as DomainObject;
            }

            public SystemObject System { get; set; }
            public ProjectObject Project { get; set; }
            public DomainObject Domain { get; set; }
        }
        public interface IScopeDescriptorObject { }
        public class SystemObject : IScopeDescriptorObject
        {
            public bool? All { get; set; }
        }
        public class ProjectObject : IScopeDescriptorObject
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public DomainObject Domain { get; set; }
        }
    }

}

//    {"auth":
//{
//"identity": 
//	{
//        "methods": [
//            "password"
//        ],
//        "password": {
//            "user": {
//                "name": "XXXX",
//                "domain": {
//                    "name": "Default"
//                },
//                "password": "XXXXXX"
//            }
//        }
//    }
//}

//}


