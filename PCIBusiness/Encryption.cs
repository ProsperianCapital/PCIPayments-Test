using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using PgpCore;

namespace PCIBusiness
{
	public static class Encryption
	{
		public static byte Encrypt(string dataPlain,byte method=0)
		{
			byte err = 10;

			try
			{

//			string dataEncrypted = "";

//			using (PGP pgp = new PGP())
//			{
//			// Generate keys
//				pgp.GenerateKey(@"C:\Temp\PGP\Public.asc", @"C:\Temp\PGP\Private.asc","PaulKilfoil@gmail.com", "123456");
//			}

				err = 20;
				(new PGP()).GenerateKey(@"C:\Temp\PGP\Public.asc", @"C:\Temp\PGP\Private.asc","PaulKilfoil@gmail.com", "123456");

			// Load keys
				err = 30;
				FileInfo       publicKey      = new FileInfo(@"C:\Temp\PGP\public.asc");
				EncryptionKeys encryptionKeys = new EncryptionKeys(publicKey);

			// Reference input/output files
				err = 50;
				FileInfo      inputFile       = new FileInfo(@"C:\Temp\PGP\Plain.txt");
				FileInfo      encryptedFile   = new FileInfo(@"C:\Temp\PGP\Encrypted.pgp");

			// Encrypt
				err = 70;
				PGP pgp = new PGP(encryptionKeys);
				pgp.EncryptFile(inputFile, encryptedFile);
			//	pgp.EncryptFileAsync(inputFile, encryptedFile);
		
				publicKey     = null;
				inputFile     = null;
				encryptedFile = null;
				err = 0;

//				if ( method <= 1 ) // PGP
//				{
//					EncryptionKeys encryptionKeys = new EncryptionKeys(new [] { "Public1", "Public2" }, "Private1", "blah");
//					PGP            pgp            = new PGP(encryptionKeys);
//					dataEncrypted                 = pgp.EncryptArmoredStringAndSign(dataPlain);
//				}
//				return dataEncrypted;

//	Now FTP

/*

            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://www.contoso.com/test.htm");
            request.Method        = WebRequestMethods.Ftp.UploadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential("anonymous", "janeDoe@contoso.com");

            // Copy the contents of the file to the request stream.
            using (FileStream fileStream = File.Open("testfile.txt", FileMode.Open, FileAccess.Read))
            {
                using (Stream requestStream = request.GetRequestStream())
                {
                    await fileStream.CopyToAsync(requestStream);
                    using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                    {
                        Console.WriteLine($"Upload File Complete, status {response.StatusDescription}");
                    }
                }
           }
        }
*/


			}
			catch (Exception ex)
			{

			}
			return 0;
		}
	}
}
