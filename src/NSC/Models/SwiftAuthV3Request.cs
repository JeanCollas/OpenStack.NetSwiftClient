using System;
using System.Collections.Generic;
using System.Text;

namespace NetSwiftClient.Models
{
    public class SwiftAuthV3Request
    {
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
        public class DomainObject
        {
            public string Name { get; internal set; }
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


