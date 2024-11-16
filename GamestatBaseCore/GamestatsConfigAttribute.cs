using System;

namespace GamestatsBase;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class GamestatsConfigAttribute : Attribute
{
    public GamestatsConfig Config { get; }

    public GamestatsConfigAttribute(
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
        Config = new(
            salt,
            rngMul,
            rngAdd,
            rngMod,
            hashMask,
            gameId,
            reqVersion,
            respVersion,
            encryptedRequest,
            requireSession);
    }

    public GamestatsConfigAttribute(
        string initString,
        GamestatsRequestVersions reqVersion,
        GamestatsResponseVersions respVersion,
        bool encryptedRequest = true,
        bool requireSession = true)
    {
        Config = new(initString, reqVersion, respVersion, encryptedRequest, requireSession);
    }
}
