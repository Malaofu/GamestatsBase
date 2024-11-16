/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;

namespace GamestatsBase;

public class GamestatsException : ApplicationException
{
    public readonly int ResponseCode;

    public GamestatsException() : this(500)
    {
    }

    public GamestatsException(int responseCode)
        : base(DefaultMessage(responseCode))
    {
        ResponseCode = responseCode;
    }

    public GamestatsException(int responseCode, string message)
        : base(message)
    {
        ResponseCode = responseCode;
    }

    public GamestatsException(int responseCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ResponseCode = responseCode;
    }

    public static string DefaultMessage(int responseCode) => 
        responseCode switch
        {
            400 => "Bad request",
            404 => "This handler is not supported. (404)",
            _   => "Server error",
        };
}
