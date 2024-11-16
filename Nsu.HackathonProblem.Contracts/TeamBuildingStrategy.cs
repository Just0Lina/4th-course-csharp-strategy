namespace Nsu.HackathonProblem.Contracts;


public class TeamBuildingStrategy
{
    public IEnumerable<Team> BuildTeams(
        IEnumerable<Employee> teamLeads,
        IEnumerable<Employee> juniors,
        IEnumerable<Wishlist> teamLeadsWishlists,
        IEnumerable<Wishlist> juniorsWishlists)
    {
        var teamLeadWishlistDict = teamLeadsWishlists.ToDictionary(wl => wl.EmployeeId);
        var juniorWishlistDict = juniorsWishlists.ToDictionary(wl => wl.EmployeeId);

        var freeJuniors = new HashSet<int>(juniorsWishlists.Select(j => j.EmployeeId));
        var teamLeadMatches = new Dictionary<int, int>();
        var juniorProposals = juniorsWishlists.ToDictionary(
            j => j.EmployeeId,
            j => new Queue<int>(j.DesiredEmployees.Distinct())
        );

        while (freeJuniors.Count > 0)
        {
            var currentJuniorId = freeJuniors.First();
            freeJuniors.Remove(currentJuniorId);

            if (!juniorProposals[currentJuniorId].Any()) continue;

            var preferredTeamLeadId = juniorProposals[currentJuniorId].Dequeue();

            if (!teamLeadMatches.TryGetValue(preferredTeamLeadId, out var currentJuniorInMatch))
            {
                teamLeadMatches[preferredTeamLeadId] = currentJuniorId;
            }
            else
            {
                HandleMatch(
                    preferredTeamLeadId,
                    currentJuniorId,
                    currentJuniorInMatch,
                    teamLeadWishlistDict,
                    juniorWishlistDict,
                    teamLeadMatches,
                    freeJuniors);
            }
        }

        return CreateTeams(teamLeads, juniors, teamLeadMatches);
    }

    private void HandleMatch(
        int teamLeadId,
        int newJuniorId,
        int currentJuniorId,
        Dictionary<int, Wishlist> teamLeadWishlistDict,
        Dictionary<int, Wishlist> juniorWishlistDict,
        Dictionary<int, int> teamLeadMatches,
        HashSet<int> freeJuniors)
    {
        var teamLeadWishlist = teamLeadWishlistDict[teamLeadId].DesiredEmployees;
        var currentJuniorIndex = Array.IndexOf(teamLeadWishlist, currentJuniorId);
        var newJuniorIndex = Array.IndexOf(teamLeadWishlist, newJuniorId);

        if (newJuniorIndex < currentJuniorIndex)
        {
            freeJuniors.Add(currentJuniorId);
            teamLeadMatches[teamLeadId] = newJuniorId;
        }
        else
        {
            freeJuniors.Add(newJuniorId);
        }
    }

    private IEnumerable<Team> CreateTeams(
        IEnumerable<Employee> teamLeads,
        IEnumerable<Employee> juniors,
        Dictionary<int, int> teamLeadMatches)
    {
        var teamLeadDict = teamLeads.ToDictionary(tl => tl.Id);
        var juniorDict = juniors.ToDictionary(j => j.Id);

        return teamLeadMatches.Select(match => new Team(
            teamLeadDict[match.Key],
            juniorDict[match.Value]));
    }
}