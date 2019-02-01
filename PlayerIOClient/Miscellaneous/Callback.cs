using System;

namespace PlayerIOClient
{
    public delegate void Callback();
    public delegate void Callback<T>(T value);

    public static class CallbackHandler
    {
        public static void CreateHandler(Action function, ref Callback successCallback, ref Callback<PlayerIOError> errorCallback)
        {
            try
            {
                function();

                successCallback?.Invoke();
            }
            catch (PlayerIOError error)
            {
                errorCallback?.Invoke(error);
            }
        }

        public static void CreateHandler<T>(Func<T> function, ref Callback<T> successCallback, ref Callback<PlayerIOError> errorCallback)
        {
            try
            {
                var output = function();

                successCallback?.Invoke(output);
            }
            catch (PlayerIOError error)
            {
                errorCallback?.Invoke(error);
            }
        }
    }
}
