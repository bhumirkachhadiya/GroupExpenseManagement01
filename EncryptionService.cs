namespace GroupExpenseManagement01.Services
{
    public interface IEncryptionService
    {
        string EncryptInteger(int number);
        int DecryptInteger(string encryptedString);

        string EncryptString(string plainText);

        string DecryptString(string encryptedString);
    }

    public class EncryptionService : IEncryptionService
    {
        private readonly int _key;

        public EncryptionService(int key)
        {
            _key = key;
        }

        public string EncryptInteger(int number)
        {
            //int keyInt = GetKeyFromString(_key);
            int transformedNumber = number ^ _key;
            byte[] byteArray = BitConverter.GetBytes(transformedNumber);
            return Convert.ToBase64String(byteArray);
        }

        public int DecryptInteger(string encryptedString)
        {
            //int keyInt = GetKeyFromString(_key);
            byte[] byteArray = Convert.FromBase64String(encryptedString);
            int transformedNumber = BitConverter.ToInt32(byteArray, 0);
            return transformedNumber ^ _key;
        }

        public string EncryptString(string plainText)
        {
            //int keyInt = GetKeyFromString(_key);
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(plainText);

            // XOR each byte with the keyInt
            for (int i = 0; i < byteArray.Length; i++)
            {
                byteArray[i] = (byte)(byteArray[i] ^ _key);
            }

            // Convert byte array to Base64 string
            return Convert.ToBase64String(byteArray);
        }

        public string DecryptString(string encryptedString)
        {
            //int keyInt = GetKeyFromString(_key);
            byte[] byteArray = Convert.FromBase64String(encryptedString);

            // XOR each byte with the keyInt
            for (int i = 0; i < byteArray.Length; i++)
            {
                byteArray[i] = (byte)(byteArray[i] ^ _key);
            }

            // Convert byte array back to original string
            return System.Text.Encoding.UTF8.GetString(byteArray);
        }

        //private int GetKeyFromString(string key)
        //{
        //    int keyInt = 0;
        //    foreach (char c in key)
        //    {
        //        keyInt += (int)c; // Sum of ASCII values of characters
        //    }
        //    return keyInt;
        //}
    }
}
