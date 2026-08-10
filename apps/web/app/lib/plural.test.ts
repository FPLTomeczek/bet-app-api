import { plural } from "./plural";
import { EVENTS_TEXTS } from "../constants";

describe("plural", () => {
  it.each([
    [1, "wydarzenie"],
    [2, "wydarzenia"],
    [3, "wydarzenia"],
    [4, "wydarzenia"],
    [5, "wydarzeń"],
    [0, "wydarzeń"],
    [11, "wydarzeń"],
    [22, "wydarzenia"],
    [25, "wydarzeń"],
  ])("picks the Polish form for %i", (count, expected) => {
    expect(plural(count, EVENTS_TEXTS.count)).toBe(expected);
  });

  it("falls back to `other` for a category the caller did not enumerate", () => {
    expect(plural(1.5, { one: "jedno", other: "inne" })).toBe("inne");
  });
});
