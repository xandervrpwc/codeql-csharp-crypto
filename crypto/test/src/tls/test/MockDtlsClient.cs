﻿using System;
using System.Collections.Generic;
using System.IO;

using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Tls.Crypto;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Tls.Tests
{
    internal class MockDtlsClient
        : DefaultTlsClient
    {
        internal TlsSession m_session;

        internal MockDtlsClient(TlsSession session)
            : base(new BcTlsCrypto(new SecureRandom()))
        {
            this.m_session = session;
        }

        public override TlsSession GetSessionToResume()
        {
            return this.m_session;
        }

        public override void NotifyAlertRaised(short alertLevel, short alertDescription, string message,
            Exception cause)
        {
            TextWriter output = (alertLevel == AlertLevel.fatal) ? Console.Error : Console.Out;
            output.WriteLine("DTLS client raised alert: " + AlertLevel.GetText(alertLevel)
                + ", " + AlertDescription.GetText(alertDescription));
            if (message != null)
            {
                output.WriteLine("> " + message);
            }
            if (cause != null)
            {
                output.WriteLine(cause);
            }
        }

        public override void NotifyAlertReceived(short alertLevel, short alertDescription)
        {
            TextWriter output = (alertLevel == AlertLevel.fatal) ? Console.Error : Console.Out;
            output.WriteLine("DTLS client received alert: " + AlertLevel.GetText(alertLevel)
                + ", " + AlertDescription.GetText(alertDescription));
        }

        public override void NotifyServerVersion(ProtocolVersion serverVersion)
        {
            base.NotifyServerVersion(serverVersion);

            Console.WriteLine("DTLS client negotiated " + serverVersion);
        }

        public override TlsAuthentication GetAuthentication()
        {
            return new MyTlsAuthentication(m_context);
        }

        public override void NotifyHandshakeComplete()
        {
            base.NotifyHandshakeComplete();

            ProtocolName protocolName = m_context.SecurityParameters.ApplicationProtocol;
            if (protocolName != null)
            {
                Console.WriteLine("Client ALPN: " + protocolName.GetUtf8Decoding());
            }

            TlsSession newSession = m_context.Session;
            if (newSession != null)
            {
                if (newSession.IsResumable)
                {
                    byte[] newSessionID = newSession.SessionID;
                    string hex = ToHexString(newSessionID);

                    if (m_session != null && Arrays.AreEqual(m_session.SessionID, newSessionID))
                    {
                        Console.WriteLine("Client resumed session: " + hex);
                    }
                    else
                    {
                        Console.WriteLine("Client established session: " + hex);
                    }

                    this.m_session = newSession;
                }

                byte[] tlsServerEndPoint = m_context.ExportChannelBinding(ChannelBinding.tls_server_end_point);
                if (null != tlsServerEndPoint)
                {
                    Console.WriteLine("Client 'tls-server-end-point': " + ToHexString(tlsServerEndPoint));
                }

                byte[] tlsUnique = m_context.ExportChannelBinding(ChannelBinding.tls_unique);
                Console.WriteLine("Client 'tls-unique': " + ToHexString(tlsUnique));
            }
        }

        public override IDictionary<int, byte[]> GetClientExtensions()
        {
            if (m_context.SecurityParameters.ClientRandom == null)
                throw new TlsFatalAlert(AlertDescription.internal_error);

            return base.GetClientExtensions();
        }

        public override void ProcessServerExtensions(IDictionary<int, byte[]> serverExtensions)
        {
            if (m_context.SecurityParameters.ServerRandom == null)
                throw new TlsFatalAlert(AlertDescription.internal_error);

            base.ProcessServerExtensions(serverExtensions);
        }

        protected virtual string ToHexString(byte[] data)
        {
            return data == null ? "(null)" : Hex.ToHexString(data);
        }

        protected override ProtocolVersion[] GetSupportedVersions()
        {
            return ProtocolVersion.DTLSv12.Only();
        }

        internal class MyTlsAuthentication
            : TlsAuthentication
        {
            private readonly TlsContext m_context;

            internal MyTlsAuthentication(TlsContext context)
            {
                this.m_context = context;
            }

            public void NotifyServerCertificate(TlsServerCertificate serverCertificate)
            {
                TlsCertificate[] chain = serverCertificate.Certificate.GetCertificateList();

                Console.WriteLine("DTLS client received server certificate chain of length " + chain.Length);
                for (int i = 0; i != chain.Length; i++)
                {
                    X509CertificateStructure entry = X509CertificateStructure.GetInstance(chain[i].GetEncoded());
                    // TODO Create fingerprint based on certificate signature algorithm digest
                    Console.WriteLine("    fingerprint:SHA-256 " + TlsTestUtilities.Fingerprint(entry) + " ("
                        + entry.Subject + ")");
                }

                bool isEmpty = serverCertificate == null || serverCertificate.Certificate == null
                    || serverCertificate.Certificate.IsEmpty;

                if (isEmpty)
                    throw new TlsFatalAlert(AlertDescription.bad_certificate);

                string[] trustedCertResources = new string[]{ "x509-server-dsa.pem", "x509-server-ecdh.pem",
                    "x509-server-ecdsa.pem", "x509-server-ed25519.pem", "x509-server-ed448.pem",
                    "x509-server-rsa_pss_256.pem", "x509-server-rsa_pss_384.pem", "x509-server-rsa_pss_512.pem",
                    "x509-server-rsa-enc.pem", "x509-server-rsa-sign.pem" };

                TlsCertificate[] certPath = TlsTestUtilities.GetTrustedCertPath(m_context.Crypto, chain[0],
                    trustedCertResources);

                if (null == certPath)
                    throw new TlsFatalAlert(AlertDescription.bad_certificate);

                TlsUtilities.CheckPeerSigAlgs(m_context, certPath);
            }

            public TlsCredentials GetClientCredentials(CertificateRequest certificateRequest)
            {
                short[] certificateTypes = certificateRequest.CertificateTypes;
                if (certificateTypes == null || !Arrays.Contains(certificateTypes, ClientCertificateType.rsa_sign))
                    return null;

                return TlsTestUtilities.LoadSignerCredentials(m_context,
                    certificateRequest.SupportedSignatureAlgorithms, SignatureAlgorithm.rsa, "x509-client-rsa.pem",
                    "x509-client-key-rsa.pem");
            }
        }
    }
}
