using System;

namespace LocalCommons.Cookie
{
    public static class Cookie
    {
        /// <summary>
        /// генерируем cookie
        /// </summary>
        /// <returns></returns>
        public static int Generate()
        {
            var random = new Random();
            var cookie = random.Next(255);
            cookie += 0 << 8;
            cookie += random.Next(255) << 16;
            cookie += random.Next(255) << 24;
            return cookie;
        }
    }
}
