/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Globalization;

namespace GamestatsBase;

public class GamestatsConfig
{
    public string Salt { get; protected set; }
    public uint RngMul { get; protected set; }
    public uint RngAdd { get; protected set; }
    public uint RngMod { get; protected set; }
    public uint HashMask { get; protected set; }
    public string GameId { get; protected set; }
    public GamestatsRequestVersions RequestVersion { get; protected set; }
    public GamestatsResponseVersions ResponseVersion { get; protected set; }
    public bool EncryptedRequest { get; protected set; }
    public bool RequireSession { get; protected set; }

    public GamestatsConfig(
        string initString,
        GamestatsRequestVersions reqVersion,
        GamestatsResponseVersions respVersion,
        bool encryptedRequest = true,
        bool requireSession = true)
    {
        if (initString.Length < 44) throw new FormatException();

        Salt = initString[..20];
        RngMul = uint.Parse(initString[20..28], NumberStyles.AllowHexSpecifier);
        RngAdd = uint.Parse(initString[28..36], NumberStyles.AllowHexSpecifier);
        RngMod = uint.Parse(initString[36..44], NumberStyles.AllowHexSpecifier);
        HashMask = uint.Parse(initString[44..52], NumberStyles.AllowHexSpecifier);
        GameId = initString[52..];

        RequestVersion = reqVersion;
        ResponseVersion = respVersion;
        EncryptedRequest = encryptedRequest;
        RequireSession = requireSession;
    }

    public GamestatsConfig(
        string salt,
        uint rngMul,
        uint rngAdd,
        uint rngMod,
        uint hashMask,
        string gameId,
        GamestatsRequestVersions reqVersion,
        GamestatsResponseVersions respVersion,
        bool encryptedRequest = true,
        bool requireSession = true)
    {
        if (salt.Length != 20) throw new FormatException();
        Salt = salt;
        RngMul = rngMul;
        RngAdd = rngAdd;
        RngMod = rngMod;
        HashMask = hashMask;
        GameId = gameId;
        RequestVersion = reqVersion;
        ResponseVersion = respVersion;
        EncryptedRequest = encryptedRequest;
        RequireSession = requireSession;
    }
}

public enum GamestatsRequestVersions
{
    /// <summary>
    /// Version 1 has very little validation. Request data doesn't contain
    /// any of the headers found in later versions and is unencrypted.
    /// </summary>
    Version1,
    /// <summary>
    /// Data contains an obfuscated checksum and pid, and supports
    /// encryption.
    /// </summary>
    Version2,
    /// <summary>
    /// Data contains an obfuscated checksum, pid, and payload length, and
    /// supports encryption.
    /// </summary>
    Version3
}

public enum GamestatsResponseVersions
{
    /// <summary>
    /// Response is plain raw binary data.
    /// </summary>
    Version1,
    /// <summary>
    /// Response contains a 40-byte salted hash at the end, in hex coded ASCII.
    /// </summary>
    Version2
}