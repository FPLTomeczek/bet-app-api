import { render, screen } from "@testing-library/react";
import { StatusBadge } from "./status-badge";
import { STATUS_STYLES, STATUS_LABELS } from "./constants";
import type { EventResponse } from "./lib/api";

describe("StatusBadge", () => {
  const statuses: EventResponse["status"][] = [
    "Scheduled",
    "Live",
    "Finished",
    "Cancelled",
  ];

  it.each(statuses)("renders the Polish label for %s", (status) => {
    render(<StatusBadge status={status} />);

    expect(screen.getByText(STATUS_LABELS[status])).toBeInTheDocument();
  });

  it("applies the status-specific style classes", () => {
    render(<StatusBadge status="Live" />);

    const badge = screen.getByText(STATUS_LABELS.Live);

    for (const className of STATUS_STYLES.Live.split(" ")) {
      expect(badge).toHaveClass(className);
    }
  });
});
