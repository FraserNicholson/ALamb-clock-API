﻿namespace Shared.Contracts;

public class QueryMatchesResponse
{
    public int Count { get; set; }
    public int CurrentPageCount { get; set; }
    public MatchesResponse[] Matches { get; set; } = default!;
}

public class MatchesResponse
{
    public string Id { get; set; } = default!;
    public string DateTimeGmt { get; set; }
    public string MatchType { get; set; } = default!;
    /// <summary>
    ///  Describes in more detail the match state e.g. what the current score/session/day is
    /// </summary>
    public string Status { get; set; } = default!;
    /// <summary>
    /// Describes the state the match is currently in, can be result/live/fixture
    /// </summary>
    public string MatchStatus { get; set; } = default!;
    public string Team1 { get; set; } = default!;
    public string Team2 { get; set; } = default!;
}