using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pulsar.Common.Messages.Other;
using Pulsar.Common.Networking;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Pulsar.Common.Tests.Networking
{
    [TestClass]
    public class SecureMessageEnvelopeTests
    {
        [TestMethod]
        public void WrapAndUnwrap_RoundTrip_ReturnsOriginalMessage()
        {
            using (var rsa = RSA.Create(2048))
            {
                var request = new CertificateRequest("CN=SecureMessageEnvelopeTests", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                using (var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30)))
                {
                    var original = new ClientIdentification
                    {
                        Version = "1.0",
                        OperatingSystem = "Windows",
                        AccountType = "Admin",
                        Country = "Wonderland",
                        CountryCode = "WL",
                        ImageIndex = 3,
                        Id = new string('A', 64),
                        Username = "tester",
                        PcName = "test-pc",
                        Tag = "tag",
                        EncryptionKey = "key",
                        Signature = new byte[] { 0x01, 0x02, 0x03 },
                        PublicIP = "127.0.0.1"
                    };

                    var envelope = SecureMessageEnvelopeHelper.Wrap(original, certificate);

                    Assert.IsNotNull(envelope);
                    Assert.IsNotNull(envelope.Payload);
                    Assert.AreNotEqual(0, envelope.Payload.Length);

                    var roundTrip = SecureMessageEnvelopeHelper.Unwrap(envelope, certificate) as ClientIdentification;

                    Assert.IsNotNull(roundTrip);
                    Assert.AreEqual(original.Username, roundTrip.Username);
                    Assert.AreEqual(original.Id, roundTrip.Id);
                    Assert.AreEqual(original.PublicIP, roundTrip.PublicIP);
                }
            }
        }
    }
}
