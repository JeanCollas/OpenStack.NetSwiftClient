using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetSwiftClient.Demo.AspNetCore
{
    public class SiteErrorCodes
    {
        public const string InternalError = "internal_error";
        public const string BadRequest = "bad_request";
        public const string NotAuthorized = "not_authorized";
        public const string NotImplemented = "not_implemented";
        public const string NotFound = "not_found";
        public const string InvalidCredentials = "invalid_credentials";
        public const string InvalidName = "invalid_name";
        public const string InvalidPassword = "invalid_password";
        public const string InvalidAuthEndpoint = "invalid_authentication_endpoint";
        public const string InvalidAccountUrl = "invalid_account_url";
        public const string InvalidContainer = "invalid_container";
        public const string InvalidObject = "invalid_object";
        public const string TempUrlKeysNotSet = "temp_keys_not_set";
        public const string InvalidKeyNumber = "invalid_key_number";

        /// <summary>
        /// Error codes: lang => code => value
        /// TODO: set an expiring cache
        /// </summary>
        public static Dictionary<string, Dictionary<string, string>> CustomCodes = new Dictionary<string, Dictionary<string, string>>();
        public static void AddErrorMessage(string errorCode, string lang, string msg)
        {
            if (!CustomCodes.ContainsKey(lang)) CustomCodes.Add(lang, new Dictionary<string, string>());
            var vals = CustomCodes[lang];
            vals[errorCode] = msg;
        }


        public static bool GetErrorMessage(string errorCode, out string msg, string lang = "en")
        {
            if (lang != "en" && lang != "fr") { msg = null; return false; }

            if (lang == "en")
                switch (errorCode)
                {
                    case NotFound: { msg = "File not found"; return true; }
                    case BadRequest: { msg = "Bad request"; return true; }
                    case InternalError: { msg = "Internal error, please try again or contact us"; return true; }
                    case NotImplemented: { msg = "Function not implemented, please contact us"; return true; }
                    case NotAuthorized: { msg = "Not authorized. Please log in with the correct account"; return true; }
                    case InvalidCredentials: { msg = "Incorrect credentials"; return true; }
                    case InvalidPassword: { msg = "Invalid credentials"; return true; }
                    case InvalidName: { msg = "Invalid name"; return true; }
                    case InvalidAuthEndpoint: { msg = "Invalid authentication endpoint"; return true; }
                    case InvalidAccountUrl: { msg = "Invalid account URL/object store URL"; return true; }
                    case InvalidContainer: { msg = "Invalid container"; return true; }
                    case InvalidObject: { msg = "Invalid object"; return true; }
                    case TempUrlKeysNotSet: { msg = "Temporary URL keys not set"; return true; }
                    case InvalidKeyNumber: { msg = "Invalid key number"; return true; }
                }
            if (lang == "fr")
            {
                switch (errorCode)
                {
                    case NotFound: { msg = "Fichier introuvable"; return true; }
                    case BadRequest: { msg = "Requête au serveur mal formulée"; return true; }
                    case InternalError: { msg = "Erreur interne. Merci de réessayer ou de nous contacter."; return true; }
                    case NotImplemented: { msg = "Fonction non implémentée, merci de nous contacter"; return true; }
                    case NotAuthorized: { msg = "Non autorisé. Merci de vous connecter avec le bon compte."; return true; }
                    case InvalidCredentials: { msg = "Identification incorrecte"; return true; }
                    case InvalidPassword: { msg = "Identification incorrecte"; return true; }
                    case InvalidName: { msg = "Nom invalide"; return true; }
                    case InvalidAuthEndpoint: { msg = "Endpoint d'authentification incorrect"; return true; }
                    case InvalidAccountUrl: { msg = "URL de compte invalide / object store URL invalide"; return true; }
                    case InvalidContainer: { msg = "Container invalide"; return true; }
                    case InvalidObject: { msg = "Objet invalide"; return true; }
                    case TempUrlKeysNotSet: { msg = "Clés d'URL temporaires non renseignées"; return true; }
                    case InvalidKeyNumber: { msg = "Numéro de clé invalide"; return true; }
                }
            }

            if (CustomCodes.TryGetValue(lang, out var dico))
                if (dico.TryGetValue(errorCode, out msg))
                    return true;

            msg = null; return false;
        }

    }
}
