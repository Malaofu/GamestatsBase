/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GamestatsBase;

public class GamestatsSession
{
    public GamestatsSession(string gameId, string salt, int pid, string url)
    {
        PID = pid;
        URL = url;
        ExpiryDate = DateTime.UtcNow.AddMinutes(10);
        Token = CreateToken();
        Hash = CreateHash(Token, salt);
        GameId = gameId;
    }

    /// <summary>
    /// A PID is a user ID which is associated with a game cartridge.
    /// </summary>
    public int PID { get; protected set; }

    /// <summary>
    /// The URL in which this session began
    /// </summary>
    public string URL { get; protected set; }

    /// <summary>
    /// 32 chars of random data which both functions as a challenge
    /// and as a session ID
    /// </summary>
    public string Token { get; protected set; }

    public string Hash { get; protected set; }

    public DateTime ExpiryDate { get; protected set; }

    public string GameId { get; protected set; }

    public object? Tag { get; set; }

    public static string CreateToken()
    {
        char[] token = new char[32];
        byte[] data = new byte[4];

        for (int x = 0; x < token.Length; x++)
        {
            // tokens have 62 possible chars: 0-9, A-Z, and a-z
            data = RandomNumberGenerator.GetBytes(4);
            uint rand = BitConverter.ToUInt32(data, 0) % 62u;

            if (rand < 10)
                token[x] = (char)('0' + rand);
            else if (rand < 36)
                token[x] = (char)('7' + rand); // 'A' + rand - 10
            else
                token[x] = (char)('=' + rand); // 'a' + rand - 36
        }
        return new string(token);
    }

    public static string CreateHash(string token, string salt)
    {
        string longToken = salt + token;

        byte[] data = new byte[longToken.Length];
        var stream = new MemoryStream(data);
        var writer = new StreamWriter(stream, Encoding.ASCII);
        writer.Write(longToken);
        writer.Flush();

        return SHA1.HashData(data).ToHexStringLower();
    }
}
