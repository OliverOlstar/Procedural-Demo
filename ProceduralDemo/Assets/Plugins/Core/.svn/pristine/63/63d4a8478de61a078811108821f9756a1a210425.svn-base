namespace Core
{
    public abstract class Singleton<T> where T : class, new()
    {
        private static T s_Instance;

        public static T Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new T();
                }
                return s_Instance;
            }
        }

        public static bool Exists()
        {
            return s_Instance != null;
        }
    }
}
