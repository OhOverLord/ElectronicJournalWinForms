using System;
using System.Text;
using System.Security.Cryptography;

namespace ElectronicJournal
{
    class User
    {
        public static string email;
        public static string firstName;
        public static string lastName;
        public static string patronymic;
        public static string password;
        public static int isStuff;
        public static string group;

        public static string makePassword(string password)
        {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hash);
        }

        public static string GetRandomPassword()
        {
            int[] arr = new int[8];
            Random rnd = new Random();
            string Password = "";

            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = rnd.Next(33, 125);
                Password += (char)arr[i];
            }
            return Password;
        }
    }
}
