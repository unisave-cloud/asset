using System;

namespace AcceptanceTests.Backend
{
    /// <summary>
    /// Simple assertions so that NUnit need not be compiled on the server
    /// </summary>
    public static class Assert
    {
        public static void IsTrue(bool value)
        {
            if (!value)
                Fail("Expected true, but false given.");
        }
        
        public static void IsFalse(bool value)
        {
            if (value)
                Fail("Expected false, but true given.");
        }
        
        public static void IsNull<T>(T value)
        {
            if (value != null)
                Fail("Expected null, but instance given: " + value);
        }
        
        public static void IsNotNull<T>(T value)
        {
            if (value == null)
                Fail("Expected not null, but null given.");
        }
        
        public static void AreEqual<T>(T expected, T actual)
            where T : IEquatable<T>
        {
            if (!expected.Equals(actual))
                Fail($"Expected {expected} but instead got {actual}");
        }

        public static void Fail(string message)
        {
            throw new Exception("Assertion: " + message);
        }
    }
}