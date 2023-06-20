using Shared.Contracts;
using Shared.Models;
using Shared.Models.Database;

namespace UnitTests.Services;

public static class TestData
{
    public static IEnumerable<NotificationDbModel> MockActiveNotifications()
    {
        return new[]
        {
            new NotificationDbModel
            {
                Id = "notification1",
                MatchId = "match1",
                DateTimeGmt = DateTime.Now,
                NotificationType = NotificationType.ChangeOfInnings,
                TeamInQuestion = "team1",
                RegistrationTokens = new List<string> { "reg-token" }
            },
            new NotificationDbModel
            {
                Id = "notification2",
                MatchId = "match2",
                DateTimeGmt = DateTime.Now,
                NotificationType = NotificationType.WicketCount,
                TeamInQuestion = "team2",
                RegistrationTokens = new List<string> { "reg-token" },
                NumberOfWickets = 5
            }
        };
    }
    
    public static CricketDataCurrentMatchesResponse[] NoNotificationsSatisfiedTestCases =
    {
        new () { Data = Array.Empty<CricketDataCurrentMatch>()},
        new()
        {
            Data = new CricketDataCurrentMatch[]
            {
                new()
                {
                    Id = "match1",
                    MatchStarted = false,
                    MatchEnded = false,
                },
                new()
                {
                    Id = "match2",
                    MatchStarted = false,
                    MatchEnded = false,
                }
            }
        },
        new()
        {
            Data = new CricketDataCurrentMatch[]
            {
                new()
                {
                    Id = "match1",
                    MatchStarted = true,
                    MatchEnded = false,
                    Score = new Score[]
                    {
                        new ()
                        {
                            Inning = "randomTeam",
                            Wickets = 4
                        }
                    }
                },
                new()
                {
                    Id = "match2",
                    MatchStarted = true,
                    MatchEnded = false,
                    Score = new Score[]
                    {
                        new ()
                        {
                            Inning = "randomTeam",
                            Wickets = 4
                        }
                    }
                }
            }
        },
        new()
        {
            Data = new CricketDataCurrentMatch[]
            {
                new()
                {
                    Id = "match1",
                    MatchStarted = true,
                    MatchEnded = false,
                    Score = new Score[]
                    {
                        new ()
                        {
                            Inning = "randomTeam",
                            Wickets = 4
                        }
                    }
                },
                new()
                {
                    Id = "match2",
                    MatchStarted = true,
                    MatchEnded = false,
                    Score = new Score[]
                    {
                        new ()
                        {
                            Inning = "randomTeam",
                            Wickets = 4
                        },
                        new ()
                        {
                            Inning = "team2",
                            Wickets = 4
                        }
                    }
                }
            }
        }
    };
    
    public static CricketDataCurrentMatchesResponse[] OneNotificationsSatisfiedTestCases =
    {
        new()
        {
            Data = new CricketDataCurrentMatch[]
            {
                new()
                {
                    Id = "match1",
                    MatchStarted = true,
                    MatchEnded = false,
                    Score = new Score[]
                    {
                        new ()
                        {
                            Inning = "randomTeam",
                            Wickets = 4
                        },
                        new ()
                        {
                            Inning = "team1"
                        }
                    }
                },
                new()
                {
                    Id = "match2",
                    MatchStarted = true,
                    MatchEnded = false,
                    Score = new Score[]
                    {
                        new ()
                        {
                            Inning = "randomTeam",
                            Wickets = 4
                        }
                    }
                }
            }
        },
        new()
        {
            Data = new CricketDataCurrentMatch[]
            {
                new()
                {
                    Id = "match1",
                    MatchStarted = true,
                    MatchEnded = false,
                    Score = new Score[]
                    {
                        new ()
                        {
                            Inning = "randomTeam",
                            Wickets = 4
                        }
                    }
                },
                new()
                {
                    Id = "match2",
                    MatchStarted = true,
                    MatchEnded = false,
                    Score = new Score[]
                    {
                        new ()
                        {
                            Inning = "randomTeam",
                            Wickets = 4
                        },
                        new ()
                        {
                            Inning = "team2",
                            Wickets = 5
                        }
                    }
                }
            }
        }
    };
    
    public static CricketDataCurrentMatchesResponse[] BothNotificationsSatisfiedTestCases = 
    {
        new()
        {
            Data = new CricketDataCurrentMatch[]
            {
                new()
                {
                    Id = "match1",
                    MatchStarted = true,
                    MatchEnded = false,
                    Score = new Score[]
                    {
                        new ()
                        {
                            Inning = "randomTeam",
                            Wickets = 10
                        }
                    }
                },
                new()
                {
                    Id = "match2",
                    MatchStarted = true,
                    MatchEnded = false,
                    Score = new Score[]
                    {
                        new ()
                        {
                            Inning = "team2",
                            Wickets = 5
                        }
                    }
                }
            }
        },
        new()
        {
            Data = new CricketDataCurrentMatch[]
            {
                new()
                {
                    Id = "match1",
                    MatchStarted = true,
                    MatchEnded = false,
                    Score = new Score[]
                    {
                        new ()
                        {
                            Inning = "randomTeam",
                            Wickets = 4
                        },
                        new ()
                        {
                            Inning = "team1"
                        }
                    }
                },
                new()
                {
                    Id = "match2",
                    MatchStarted = true,
                    MatchEnded = false,
                    Score = new Score[]
                    {
                        new ()
                        {
                            Inning = "randomTeam",
                            Wickets = 4
                        },
                        new ()
                        {
                            Inning = "team2",
                            Wickets = 5
                        }
                    }
                }
            }
        }
    };
}