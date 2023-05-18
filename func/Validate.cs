using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Ondfisk.B2C
{
    public class Validate
    {
        private readonly ILogger _logger;

        public Validate(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Validate>();
        }

        [Function(nameof(Validate))]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            if (req.FunctionContext.BindingContext.BindingData.TryGetValue("ClientCertificate", out var obj) &&
                obj is X509Certificate2 certificate &&
                ValidateCertificate(certificate))
            {
                // to stuff...

                return req.CreateResponse(HttpStatusCode.NoContent);
            }

            return req.CreateResponse(HttpStatusCode.Unauthorized);
        }

        private static bool ValidateCertificate(X509Certificate2 certificate)
        {
            // Implement your certificate validation logic here
            // You can compare the client certificate against a trusted certificate or thumbprint
            // Return true if the certificate is valid, otherwise return false

            // Example: Validate against a specific thumbprint
            var expectedThumbprint = Environment.GetEnvironmentVariable("CERTIFICATE_THUMBPRINT");

            if (certificate.NotBefore > DateTime.UtcNow)
            {
                return false;
            }
            if (certificate.NotAfter < DateTime.UtcNow)
            {
                return false;
            }
            if (!certificate.Thumbprint.Equals(expectedThumbprint, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }
    }
}
