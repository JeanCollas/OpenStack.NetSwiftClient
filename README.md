# OpenStack .Net SwiftClient

A client developed in .Net Standard to access OpenStack servers and perform the most frequent actions.

A WPF client is included to demonstrate some of the most important functions.

It has been developed to address the lack of .Net client for the OVH object storage service.

## ASP.NET Core demo project

Just launch the project, on first start, the site should ask for your authentication endpoint and credentials. 
Then you may browse your account.

## WPF demo project

Add a `DemoContext.NotIncluded.cs` file on the WPF demo project, with the following code in order to test the project more easily:

```csharp
    namespace NetSwiftClient.Demo
    {
        public partial class DemoContext
        {
            public DemoContext()
            {
                _AuthUrl = "https://auth.cloud.ovh.net/v3";
                _AuthName = "XXXXX";
                _AuthPassword = "YYYYY";
                _AuthDomain = "Default";
                _ObjectStoreUrl = "https://storage.sbg3.cloud.ovh.net/v1/AUTH_zzzz"; 
            }
        }
    }
