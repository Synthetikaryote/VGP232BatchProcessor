using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

using BatchFileFramework;
namespace batchFileProcessor
{

    public static class StringCipher
    {
        // This constant string is used as a "salt" value for the PasswordDeriveBytes function calls.
        // This size of the IV (in bytes) must = (keysize / 8).  Default keysize is 256, so the IV must be
        // 32 bytes long.  Using a 16 character string here gives us 32 bytes when converted to a byte array.
        private static readonly byte[] initVectorBytes = Encoding.ASCII.GetBytes( "tu89geji340t89u2" );

        // This constant is used to determine the keysize of the encryption algorithm.
        private const int keysize = 256;

        public static string Encrypt( string plainText, string passPhrase )
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes( plainText );
            using( PasswordDeriveBytes password = new PasswordDeriveBytes( passPhrase, null ) )
            {
                byte[] keyBytes = password.GetBytes( keysize / 8 );
                using( RijndaelManaged symmetricKey = new RijndaelManaged() )
                {
                    symmetricKey.Mode = CipherMode.CBC;
                    using( ICryptoTransform encryptor = symmetricKey.CreateEncryptor( keyBytes, initVectorBytes ) )
                    {
                        using( MemoryStream memoryStream = new MemoryStream() )
                        {
                            using( CryptoStream cryptoStream = new CryptoStream( memoryStream, encryptor, CryptoStreamMode.Write ) )
                            {
                                cryptoStream.Write( plainTextBytes, 0, plainTextBytes.Length );
                                cryptoStream.FlushFinalBlock();
                                byte[] cipherTextBytes = memoryStream.ToArray();
                                return Convert.ToBase64String( cipherTextBytes );
                            }
                        }
                    }
                }
            }
        }

        public static string Decrypt( string cipherText, string passPhrase )
        {
            byte[] cipherTextBytes = Convert.FromBase64String( cipherText );
            using( PasswordDeriveBytes password = new PasswordDeriveBytes( passPhrase, null ) )
            {
                byte[] keyBytes = password.GetBytes( keysize / 8 );
                using( RijndaelManaged symmetricKey = new RijndaelManaged() )
                {
                    symmetricKey.Mode = CipherMode.CBC;
                    using( ICryptoTransform decryptor = symmetricKey.CreateDecryptor( keyBytes, initVectorBytes ) )
                    {
                        using( MemoryStream memoryStream = new MemoryStream( cipherTextBytes ) )
                        {
                            using( CryptoStream cryptoStream = new CryptoStream( memoryStream, decryptor, CryptoStreamMode.Read ) )
                            {
                                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                                int decryptedByteCount = cryptoStream.Read( plainTextBytes, 0, plainTextBytes.Length );
                                return Encoding.UTF8.GetString( plainTextBytes, 0, decryptedByteCount );
                            }
                        }
                    }
                }
            }
        }
    }



	class Program
	{
		static long totalFilesLen = 0;
        static long fileCount = 0;

        // this is a comment
        // this is a comment
		static void ProcessFile(IFileAccessLogic lo, System.IO.FileInfo fi)
		{
			totalFilesLen += fi.Length;
            ++fileCount;
            Console.WriteLine(fi.Name);
            Console.WriteLine("  size: {0} bytes", fi.Length);
            Console.WriteLine("  full path: {0}", fi.DirectoryName);
            System.IO.File.Move(fi.FullName, fi.DirectoryName + "\\visited_" + fi.Name);
        }

		static void Main(string[] args)
		{
			if(args.Length < 1)
			{
				Console.WriteLine("Missing file or folder path: \nUsage: batchFileProcessor filepath(folderpath)");
				return;
			}

            // This is another comment
            // This is another comment
			FileAccessLogic accessor = new FileAccessLogic();
            accessor.FilePattern = "*.txt";
			accessor.OnProcess += ProcessFile;

			accessor.Recursive = true;
			accessor.Execute(args[0]);

			Console.WriteLine("{0} *.txt files in directory \"{1}\" with total size {2} bytes and average size {3} bytes", fileCount, args[0], totalFilesLen, (float)totalFilesLen / fileCount);
            // Keep the console window open in debug mode.
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

		}		       

	}
}
