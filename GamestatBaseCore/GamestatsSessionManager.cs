﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;

namespace GamestatsBase;

public class GamestatsSessionManager
{
    public readonly Dictionary<string, GamestatsSession> Sessions = [];

    public GamestatsSessionManager()
    {
    }

    public void PruneSessions()
    {
        Dictionary<string, GamestatsSession> sessions = Sessions;
        DateTime now = DateTime.UtcNow;

        lock (sessions)
        {
            Queue<string> toRemove = new Queue<string>();
            foreach (var session in sessions)
            {
                if (session.Value.ExpiryDate < now) toRemove.Enqueue(session.Key);
            }
            while (toRemove.Count > 0)
            {
                sessions.Remove(toRemove.Dequeue());
            }
        }
    }

    public void Add(GamestatsSession session)
    {
        Sessions.Add(session.Hash, session);
    }

    public void Remove(GamestatsSession? session)
    {
        if (session != null)
            Sessions.Remove(session.Hash);
    }

    /// <summary>
    /// Finds a session matching a player ID and URL. You may need this if
    /// the game begins a second session but needs info from the first.
    /// </summary>
    /// <param name="pid">Gamespy player ID</param>
    /// <param name="url">URL where the desired session began</param>
    /// <returns>The found session or null if none was found</returns>
    public GamestatsSession? FindSession(int pid, string url)
    {
        // todo: keep a hash table of pids that maps them onto session objects
        GamestatsSession? result = null;

        foreach (GamestatsSession sess in Sessions.Values)
        {
            if (sess.PID == pid && sess.URL == url)
            {
                if (result != null)
                {
                    // todo: there's more than one matching session... delete them all.
                }
                return sess; // temp until I get it to cleanup old sessions
                //result = sess;
            }
        }
        return result;
    }
}
