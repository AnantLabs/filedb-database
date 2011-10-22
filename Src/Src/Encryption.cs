using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FileDbNs
{
    internal class Encryptor
    {
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

        /* brettg: these are the original encryption methods, which I've saved for future ref
         * 
        internal string Encrypt( byte[] key, string dataToEncrypt )
        {
            // Initialise
            AesManaged encryptor = new AesManaged();

            // Set the key
            encryptor.Key = key;
            encryptor.IV = key;

            // create a memory stream
            using( MemoryStream encryptionStream = new MemoryStream() )
            {
                // Create the crypto stream
                using( CryptoStream encrypt = new CryptoStream( encryptionStream, encryptor.CreateEncryptor(), CryptoStreamMode.Write ) )
                {
                    // Encrypt
                    byte[] utfD1 = UTF8Encoding.UTF8.GetBytes( dataToEncrypt );
                    encrypt.Write( utfD1, 0, utfD1.Length );
                    encrypt.FlushFinalBlock();
                    encrypt.Close();

                    // Return the encrypted data
                    return Convert.ToBase64String( encryptionStream.ToArray() );
                }
            }
        }

        internal string Decrypt( byte[] key, string encryptedString )
        {
            // Initialise
            AesManaged decryptor = new AesManaged();
            byte[] encryptedData = Convert.FromBase64String( encryptedString );

            // Set the key
            decryptor.Key = key;
            decryptor.IV = key;

            // create a memory stream
            using( MemoryStream decryptionStream = new MemoryStream() )
            {
                // Create the crypto stream
                using( CryptoStream decrypt = new CryptoStream( decryptionStream, decryptor.CreateDecryptor(), CryptoStreamMode.Write ) )
                {
                    // Encrypt
                    decrypt.Write( encryptedData, 0, encryptedData.Length );
                    decrypt.Flush();
                    decrypt.Close();

                    // Return the unencrypted data
                    byte[] decryptedData = decryptionStream.ToArray();
                    return UTF8Encoding.UTF8.GetString( decryptedData, 0, decryptedData.Length );
                }
            }
        }*/
    }
}
