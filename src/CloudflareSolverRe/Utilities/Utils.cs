using CloudflareSolverRe.Constants;
using System;

namespace CloudflareSolverRe.Utilities
{
    public static class Utils
    {
        private static readonly Random random = new Random();

        public static string GetGenerateRandomUserAgent() =>
            UserAgents.UserAgentList[random.Next(0, UserAgents.UserAgentList.Count)];
    }
}
