import type { components } from "./api-types";
import { texts } from "./texts";

type ValidationProblem = components["schemas"]["ValidationProblemDetails"];

const ERROR_TEXTS = {
  username_taken: texts.errorUsernameTaken,
  email_taken: texts.errorEmailTaken,
  sport_category_not_found: texts.errorSportCategoryNotFound,
  team_not_found: texts.errorTeamNotFound,
  player_not_found: texts.errorPlayerNotFound,
  user_not_found: texts.errorUserNotFound,
  bonus_not_found: texts.errorBonusNotFound,
  category_name_taken: texts.errorCategoryNameTaken,
  participant_requires_team_or_player: texts.errorParticipantRequiresTeamOrPlayer,
  selection_event_not_found: texts.errorSelectionEventNotFound,
};

type ErrorCode = keyof typeof ERROR_TEXTS;

function isKnownCode(value: string): value is ErrorCode {
  return value in ERROR_TEXTS;
}

export function translateValidationErrors(
  problem: ValidationProblem,
): Record<string, string[]> {
  const translated: Record<string, string[]> = {};

  for (const [field, codes] of Object.entries(problem.errors ?? {})) {
    translated[field] = codes.map((code) =>
      isKnownCode(code) ? ERROR_TEXTS[code] : texts.errorUnknown,
    );
  }

  return translated;
}
