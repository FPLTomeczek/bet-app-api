namespace BetApp.Api;

/// <summary>
/// Machine-readable validation codes returned in <c>ValidationProblemDetails.errors</c>.
/// </summary>
public static class ErrorCodes
{
    public const string UsernameTaken = "username_taken";
    public const string EmailTaken = "email_taken";
    public const string SportCategoryNotFound = "sport_category_not_found";
    public const string TeamNotFound = "team_not_found";
    public const string PlayerNotFound = "player_not_found";
    public const string UserNotFound = "user_not_found";
    public const string BonusNotFound = "bonus_not_found";
    public const string CategoryNameTaken = "category_name_taken";
    public const string ParticipantRequiresTeamOrPlayer = "participant_requires_team_or_player";
    public const string SelectionEventNotFound = "selection_event_not_found";
}
