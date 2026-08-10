import { translateValidationErrors } from "./errors";
import { texts } from "./texts";

describe("translateValidationErrors", () => {
  it("maps an API error code onto Polish copy", () => {
    const result = translateValidationErrors({
      errors: { SportCategoryId: ["sport_category_not_found"] },
    });

    expect(result).toEqual({
      SportCategoryId: [texts.errorSportCategoryNotFound],
    });
  });

  it("translates every field of a multi-field failure", () => {
    const result = translateValidationErrors({
      errors: {
        Username: ["username_taken"],
        Email: ["email_taken"],
      },
    });

    expect(result).toEqual({
      Username: [texts.errorUsernameTaken],
      Email: [texts.errorEmailTaken],
    });
  });

  it("keeps several errors reported on one field", () => {
    const result = translateValidationErrors({
      errors: { TeamId: ["participant_requires_team_or_player", "team_not_found"] },
    });

    expect(result.TeamId).toEqual([
      texts.errorParticipantRequiresTeamOrPlayer,
      texts.errorTeamNotFound,
    ]);
  });

  it("hides framework English behind the generic message", () => {
    // DataAnnotations messages land in the same dictionary and carry no code to look up.
    const result = translateValidationErrors({
      errors: { Email: ["The Email field is not a valid e-mail address."] },
    });

    expect(result.Email).toEqual([texts.errorUnknown]);
  });

  it("returns nothing when the problem carries no errors", () => {
    expect(translateValidationErrors({ title: "Bad Request", status: 400 })).toEqual({});
  });
});
