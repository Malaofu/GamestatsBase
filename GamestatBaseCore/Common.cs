/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;

namespace GamestatsBase;

public static class Common
{
    public static string ToHexStringUpper(this byte[] bytes)
    {
        // http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa/14333437#14333437
        char[] c = new char[bytes.Length * 2];
        int b;
        for (int i = 0; i < bytes.Length; i++)
        {
            b = bytes[i] >> 4;
            c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
            b = bytes[i] & 0xF;
            c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
        }
        return new string(c);
    }

    public static string ToHexStringLower(this byte[] bytes)
    {
        char[] c = new char[bytes.Length * 2];
        int b;
        for (int i = 0; i < bytes.Length; i++)
        {
            b = bytes[i] >> 4;
            c[i * 2] = (char)(87 + b + (((b - 10) >> 31) & -39));
            b = bytes[i] & 0xF;
            c[i * 2 + 1] = (char)(87 + b + (((b - 10) >> 31) & -39));
        }
        return new string(c);
    }

    public static byte[] FromHexString(string hex)
    {
        // very suboptimal but error tolerant
        byte output = 0;
        List<byte> result = new(hex.Length / 2);
        bool havePrev = false;
        foreach (char c in hex.ToCharArray())
        {
            if (c >= '0' && c <= '9')
            {
                output |= (byte)(c - '0');
            }
            if (c >= 'A' && c <= 'F')
            {
                output |= (byte)(c - '7');
            }
            if (c >= 'a' && c <= 'f')
            {
                output |= (byte)(c - 'W');
            }
            if (havePrev)
            {
                havePrev = false;
                result.Add(output);
                output = 0;
            }
            else
            {
                havePrev = true;
                output <<= 4;
            }
        }

        return result.ToArray();
    }

    /// <summary>
    /// Decrypts the NDS &amp;data= querystring into readable binary data.
    /// The PID (little endian) is left at the start of the output
    /// but the (unencrypted) checksum is removed.
    /// </summary>
    public static byte[] DecryptData(GamestatsConfig config, string data)
    {
        byte[] data2 = FromUrlSafeBase64String(data);
        if (config.RequestVersion == GamestatsRequestVersions.Version1) return data2;
        if (data2.Length < 4) throw new FormatException("Data must contain at least 4 bytes.");

        int checksum = BitConverter.ToInt32(data2, 0);
        checksum = IPAddress.NetworkToHostOrder(checksum); // endian flip
        checksum ^= (int)config.HashMask;

        byte[] data3;
        if (config.EncryptedRequest)
            data3 = DecryptMain(config, data2, checksum & 0xffff | checksum << 16);
        else
        {
            data3 = new byte[data2.Length - 4];
            Array.Copy(data2, 4, data3, 0, data2.Length - 4);
        }

        int checkedsum = 0;
        foreach (byte b in data3)
            checkedsum += b;

        if (checkedsum != checksum) throw new FormatException("Data checksum is incorrect.");

        return data3;
    }

    private static byte[] DecryptMain(GamestatsConfig config, byte[] data2, int rand)
    {
        byte[] data3 = new byte[data2.Length - 4];

        for (int pos = 0; pos < data3.Length; pos++)
        {
            rand = DecryptRNG(config, rand);
            data3[pos] = (byte)(data2[pos + 4] ^ (byte)(rand >> 16));
        }
        return data3;
    }

    private static int DecryptRNG(GamestatsConfig config, int prev) =>
        DecryptRNG(prev, config.RngMul, config.RngAdd, config.RngMod);

    private static int DecryptRNG(int prev, uint mul, uint add, uint mod)
    {
        return (int)(((uint)prev * mul + add) % mod);
    }

    public static byte[] FromUrlSafeBase64String(string data)
    {
        return Convert.FromBase64String(data.Replace('-', '+').Replace('_', '/'));
    }

    public static string ToUrlSafeBase64String(byte[] data)
    {
        return Convert.ToBase64String(data).Replace('+', '-').Replace('/', '_');
    }

    public static string ResponseChecksum(GamestatsConfig config, byte[] responseArray)
    {
        string toCheck = config.Salt + ToUrlSafeBase64String(responseArray) + config.Salt;

        byte[] data = new byte[toCheck.Length];
        MemoryStream stream = new(data);
        StreamWriter writer = new(stream);
        writer.Write(toCheck);
        writer.Flush();

        return SHA1.HashData(data).ToHexStringLower();
    }
}
