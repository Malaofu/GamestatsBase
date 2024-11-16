/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GamestatsBase;

public class GamestatsBaseMiddleware
{
    private readonly RequestDelegate _next;
    private readonly GamestatsSessionManager _sessionManager;

    public GamestatsBaseMiddleware(RequestDelegate next, GamestatsSessionManager sessionManager)
    {
        _next = next;
        _sessionManager = sessionManager;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get Config for controller
        var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
        var config = endpoint?.Metadata.GetMetadata<GamestatsConfigAttribute>()?.Config;
        if (config == null)
        {
            await _next(context);
            return;
        }

        var request = context.Request;
        var response = context.Response;

        // Get pid
        if (!TryGetPid(request, out int pid))
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        string dataString = request.Query["data"];
        string hash = request.Query["hash"];

        if (dataString == null && hash == null)
        {
            await NewSession(context, config, pid);
            return;
        }

        if (dataString == null || 
            hash == null && config.RequireSession || 
            dataString.Length < (config.RequestVersion != GamestatsRequestVersions.Version3 ? 12 : 16))
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var session = GetSession(hash, config);
        if (config.RequireSession && session is null)
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        byte[] data;
        try
        {
            data = Common.DecryptData(config, dataString);
            if (data.Length < (config.RequestVersion != GamestatsRequestVersions.Version3 ? 4 : 8))
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }
        }
        catch (FormatException)
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        int pid2 = BitConverter.ToInt32(data, 0);
        if (pid2 != pid)
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        if (config.RequestVersion == GamestatsRequestVersions.Version3)
        {
            int length = BitConverter.ToInt32(data, 4);
            if (length + 8 != data.Length)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }
        }

        int trimLength = config.RequestVersion != GamestatsRequestVersions.Version3 ? 4 : 8;
        byte[] dataTrim = new byte[data.Length - trimLength];
        Array.Copy(data, trimLength, dataTrim, 0, data.Length - trimLength);

        var originalBodyStream = response.Body;
        using MemoryStream responseBody = new();
        response.Body = responseBody;

        try
        {
            // Set Items for use elsewhere
            context.Items["pid"] = pid;
            context.Items["data"] = dataTrim;
            context.Items["session"] = session;

            // Redefine 'data' i query to be 'dataTrim'
            var query = QueryHelpers.ParseQuery(request.QueryString.Value);
            query["data"] = Convert.ToBase64String(dataTrim);

            var queryBuilder = new QueryBuilder(query.Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value.ToString())));
            request.QueryString = queryBuilder.ToQueryString();

            // Call _next
            await _next(context);
        }
        catch (GamestatsException ex)
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            await response.WriteAsync(ex.Message);
            return;
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            await response.WriteAsync(ex.Message);
            return;
        }

        byte[] byteArray = (response.Body as MemoryStream)?.ToArray() ?? [];

        response.Body = originalBodyStream;
        await response.Body.WriteAsync(byteArray);

        if (config.ResponseVersion != GamestatsResponseVersions.Version1)
            await response.WriteAsync(Common.ResponseChecksum(config, byteArray));
    }

    private static bool TryGetPid(HttpRequest request, out int pid)
    {
        var pidString = request.Query["pid"].FirstOrDefault();
        return int.TryParse(pidString, out pid);
    }

    private async Task NewSession(HttpContext context, GamestatsConfig config, int pid)
    {
        var newSession = CreateSession(config, pid, context.Request.Path);
        _sessionManager.Add(newSession);
        context.Response.StatusCode = StatusCodes.Status200OK;
        await context.Response.WriteAsync(newSession.Token);
    }

    private GamestatsSession? GetSession(string? hash, GamestatsConfig config)
    {
        GamestatsSession? session = null;
        if (hash != null && _sessionManager.Sessions.TryGetValue(hash, out var foundSession))
        {
            session = foundSession;
            if (session.GameId != config.GameId)
                return null;
        }
        return session;
    }

    /// <summary>
    /// Session factory if you need it. You probably don't.
    /// </summary>
    public virtual GamestatsSession CreateSession(GamestatsConfig config, int pid, string url)
    {
        return new GamestatsSession(config.GameId, config.Salt, pid, url);
    }
}
