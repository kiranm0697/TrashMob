﻿namespace TrashMob.Shared.Persistence
{
    using DocuSign.eSign.Client;
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using static DocuSign.eSign.Client.Auth.OAuth;

    public class DocusignCertificateAuthenticator : IDocusignAuthenticator
    {
        private readonly IKeyVaultManager keyVaultManager;

        public DocusignCertificateAuthenticator(IKeyVaultManager keyVaultManager)
        {
            this.keyVaultManager = keyVaultManager;
        }

        /// <summary>
        /// Uses Json Web Token (JWT) Authentication Method to obtain the necessary information needed to make API calls.
        /// </summary>
        /// <returns>Auth token needed for API calls</returns>
        public OAuthToken AuthenticateWithJWT(string clientId, string impersonatedUserId, string authServer)
        {
            var secret = keyVaultManager.GetSecret("Docusign");
            // var password = keyVaultManager.GetSecret("DocusignPassword");
            var password = "";
            var privateKeyBytes = Convert.FromBase64String(secret);
            var certificate = new X509Certificate2(privateKeyBytes, password);
            var pk = certificate.GetRSAPrivateKey();

            var bytes = new Span<byte>();
            pk.TryExportRSAPrivateKey(bytes, out var bytesWritten);

            var apiClient = new ApiClient();
            var scopes = new List<string>
                {
                    "signature",
                    "impersonation",
                };

            return apiClient.RequestJWTUserToken(clientId, impersonatedUserId, authServer, bytes.ToArray(), 1, scopes);
        }
    }
}
