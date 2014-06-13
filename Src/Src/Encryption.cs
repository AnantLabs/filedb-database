/* Copyright (C) EzTools Software - All Rights Reserved
 * Proprietary and confidential source code.
 * This is not free software.  Any copying of this file 
 * via any medium is strictly prohibited except as allowed
 * by the FileDb license agreement.
 * Written by Brett Goodman <eztools-software.com>, October 2014
 */
using System.IO;
using System.Text;
#if !WINDOWS_PHONE_APP
using System.Security.Cryptography;
#endif

namespace FileDbNs
{
    internal class Encryptor
    {
#if WINDOWS_PHONE_APP
        internal Encryptor( string hashKey, string productKey )
        {
        }

        internal byte[] Encrypt( byte[] dataToEncrypt )
        {
            return dataToEncrypt;
        }

        internal byte[] Decrypt( byte[] encryptedData )
        {
            return encryptedData;
        }
#else
        byte[] _key;
        AesManaged _encryptor;

        internal Encryptor( string hashKey, string productKey )
        {
            _key = GetHashKey( hashKey, productKey );
            _encryptor = new AesManaged();

            // Set the key
            _encryptor.Key = _key;
            _encryptor.IV = _key;
        }

        internal static byte[] GetHashKey( string hashKey, string salt )
        {
            // Initialise
            UTF8Encoding encoder = new UTF8Encoding();

            // Get the salt
            byte[] saltBytes = encoder.GetBytes( salt );

            // Setup the hasher
            Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes( hashKey, saltBytes );

            // Return the key
            return rfc.GetBytes( 16 );
        }

        internal byte[] Encrypt( byte[] dataToEncrypt )
        {
            byte[] bytes = null;
            MemoryStream outStrm = new MemoryStream( (int) (dataToEncrypt.Length * 1.5) );

            // Create the crypto stream
            using( CryptoStream encrypt = new CryptoStream( outStrm, _encryptor.CreateEncryptor(), CryptoStreamMode.Write ) )
            {
                // Encrypt
                encrypt.Write( dataToEncrypt, 0, dataToEncrypt.Length );
                encrypt.FlushFinalBlock();
                bytes = outStrm.ToArray();
                encrypt.Close();
            }
            return bytes;
        }

        internal byte[] Decrypt( byte[] encryptedData )
        {
            byte[] bytes = null;
            MemoryStream outStrm = new MemoryStream( (int) (encryptedData.Length * 1.5) );

            // Create the crypto stream
            using( CryptoStream decrypt = new CryptoStream( outStrm, _encryptor.CreateDecryptor(), CryptoStreamMode.Write ) )
            {
                // Encrypt
                decrypt.Write( encryptedData, 0, encryptedData.Length );
                decrypt.FlushFinalBlock();
                bytes = outStrm.ToArray();
                decrypt.Close();
            }
            return bytes;
        }

#endif
    }
}
