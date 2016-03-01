using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftLife.CSharp.Validation
{
    /// <summary>
    /// This class is a DTO for an HMACSHA1 implementation of a digital signature. 
    /// </summary>
    public class SimpleSignature
    {
        public string username { get; set; }
        public string baseString { get; set; }
        public string password;
        public string signature { get; set; }

        /// <summary>
        /// [WARNING] The password needs to be replaced in periodic intervals (say every 6 months or so) to avoid compromising the already fragile security even further...
        /// The way this should be done is:
        /// Obtains a password from a DB given the userName and signs the current timestamp and stores this signature.
        /// </summary>
        /// <param name="encodeForUrl">Flag for url encoding, in cases where the signature needs to be send over an url (loytySiteAccessLayer)</param>
        public SimpleSignature(bool encodeForUrl = false)
        {
            username = "Administrador";
            baseString = DateTime.Now.ToString();
            password = "admin401wq3";
            if (encodeForUrl)
                signature = SignatureManager.signHMACSHA1URLEncoded(baseString, password);
            else
                signature = SignatureManager.signHMACSHA1(baseString, password);
        }


        public void Validate(string userName, string baseString, string signature)
        {
            //Validar timestamp
            if (!SignatureManager.IsValidTime(baseString))
                throw new Exception("Timestamp inválido!");

            //Validar assinatura
            if (!SignatureManager.IsValidSignature(baseString, this.password, signature))
                throw new Exception("Credenciais inválidas!");
        }


        public void ValidateURLEncoded(string userName, string baseString, string signature)
        {
            //Validar timestamp
            if (!SignatureManager.IsValidTime(baseString))
                throw new Exception("Timestamp inválido!");

            //Validar assinatura
            if (!SignatureManager.IsValidSignatureURLEncoded(baseString, this.password, signature))
                throw new Exception("Credenciais inválidas!");
        }
    }
}