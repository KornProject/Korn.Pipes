using System.Collections.Generic;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Korn.Pipes
{
    public static class NameGlobalizer
    {
        // get hash for name, this will be global name
        public static string GlobalizeName(string name)
        {
            if (HashedNamesCache.TryGetHashForName(name, out var cachedHash))
                return TransformHash(cachedHash);

            var hash = NameHasher.HashNameToString($"Korn.Service.Shared {name}");
            HashedNamesCache.PutHashForName(name, hash);

            return TransformHash(hash);

            string TransformHash(string input) => "korn." + input;
        }

        class NameHasher
        {
            static MD5 hashingAlgorithm = MD5.Create();

            public static string HashNameToString(string name) => HashNameToGuid(name).ToString();

            public static Guid HashNameToGuid(string name) => new Guid(HashNameToBinary(name));

            public static byte[] HashNameToBinary(string name) => hashingAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(name));
        }

        class HashedNamesCache
        {
            const int DictionaryThreshold = 0x7F;
            static Dictionary<int, string> cachedHashesForNames = new Dictionary<int, string>();

            public static bool TryGetHashForName(string name, out string result) => TryGetHash(name.GetHashCode(), out result);

#pragma warning disable CS8601 // Possible null reference assignment.
            static bool TryGetHash(int hashCode, out string result) => cachedHashesForNames.TryGetValue(hashCode, out result);
#pragma warning restore CS8601 // Possible null reference assignment.

            public static void PutHashForName(string name, string hash) => PutHash(name.GetHashCode(), hash);

            static void PutHash(int hashCode, string hash)
            {
                // not very effective from a distance, but it's a quick enough
                if (cachedHashesForNames.Count > DictionaryThreshold)
                    cachedHashesForNames.Clear();

                cachedHashesForNames[hashCode] = hash;
            }
        }
    }


}