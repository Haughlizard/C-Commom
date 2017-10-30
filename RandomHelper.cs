public static class RandomHelper
    {
        private static readonly Random RandomSeed = new Random();

        /// <summary>
        /// Generates a random string with the given length
        /// </summary>
        /// <param name="size">Size of the string</param>
        /// <param name="lowerCase">If true, generate lowercase string</param>
        /// <returns>Random string</returns>
        // 随机产生指定长度的大写或者小写字符串
        public static string RandomString(int size, bool lowerCase)
        {
            // StringBuilder is faster than using strings (+=)
            var randStr = new StringBuilder(size);

            // Ascii start position (65 = A / 97 = a)
            var start = (lowerCase) ? 97 : 65;

            // Add random chars
            for (var i = 0; i < size; i++)
                randStr.Append((char)(26 * RandomSeed.NextDouble() + start));

            return randStr.ToString();
        }
        
       //随机产生指定范围内的随机整数
        public static int RandomInt(int min, int max)
        {
            return RandomSeed.Next(min, max);
        }
        
       //随机产生Double类型数
        public static double RandomDouble()
        {
            return RandomSeed.NextDouble();
        }
       
     
        public static double RandomNumber(int min, int max, int digits)
        {
            return Math.Round(RandomSeed.Next(min, max - 1) + RandomSeed.NextDouble(), digits);
        }
        
        public static bool RandomBool()
        {
            return (RandomSeed.NextDouble() > 0.5);
        }

        public static DateTime RandomDate()
        {
            return RandomDate(new DateTime(1900, 1, 1), DateTime.Now);
        }

        public static DateTime RandomDate(DateTime from, DateTime to)
        {
            var range = new TimeSpan(to.Ticks - from.Ticks);
            return from + new TimeSpan((long)(range.Ticks * RandomSeed.NextDouble()));
        }

        public static Color RandomColor()
        {
            return Color.FromRgb((byte)RandomSeed.Next(255), (byte)RandomSeed.Next(255), (byte)RandomSeed.Next(255));
        }
    }
