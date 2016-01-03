namespace Xp.Runners
{
    static class Strings
    {
        /// <summary>PHP's ucfirst() in C#</summary>
        public static string UpperCaseFirst(this string self)
        {
            if (string.IsNullOrEmpty(self))
            {
                return string.Empty;
            }
            else
            {
                var a = self.ToCharArray();
                a[0] = char.ToUpper(a[0]);
                return new string(a);
            }
        }
    }
}