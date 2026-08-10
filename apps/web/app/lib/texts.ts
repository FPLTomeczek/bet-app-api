export const texts = {
  // metadata (app/layout.tsx)
  metadataTitle: "BetApp — wydarzenia",
  metadataDescription: "Lista wydarzeń sportowych dostępnych do obstawiania.",

  // events list (app/page.tsx)
  eventsTitle: "Wydarzenia",
  eventsSource: "z API",
  // Polish plural forms as Intl.PluralRules reports them: 1 → one, 2-4 → few,
  // 0 and 5-21 → many. A two-form ternary cannot express this.
  eventsCountOne: "wydarzenie",
  eventsCountFew: "wydarzenia",
  eventsCountMany: "wydarzeń",

  // event status (app/status-badge.tsx)
  statusScheduled: "Zaplanowane",
  statusLive: "Na żywo",
  statusFinished: "Zakończone",
  statusCancelled: "Odwołane",

  // theme toggle (components/theme/theme-toggle.tsx)
  themeToggleLabel: "Przełącz motyw",

  // validation failures, keyed by the codes in apps/api/BetApp.Api/ErrorCodes.cs
  errorUsernameTaken: "Ta nazwa użytkownika jest już zajęta.",
  errorEmailTaken: "Ten adres e-mail jest już zarejestrowany.",
  errorSportCategoryNotFound: "Wybrana kategoria sportowa nie istnieje.",
  errorTeamNotFound: "Wybrana drużyna nie istnieje.",
  errorPlayerNotFound: "Wybrany zawodnik nie istnieje.",
  errorUserNotFound: "Wybrany użytkownik nie istnieje.",
  errorBonusNotFound: "Wybrany bonus nie istnieje.",
  errorCategoryNameTaken: "Kategoria o tej nazwie już istnieje.",
  errorParticipantRequiresTeamOrPlayer: "Uczestnik musi wskazywać drużynę albo zawodnika.",
  errorSelectionEventNotFound: "Co najmniej jeden typ wskazuje nieistniejące wydarzenie.",
  errorUnknown: "Wystąpił nieoczekiwany błąd. Spróbuj ponownie.",
} as const;
