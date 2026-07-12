import { render, screen } from "@testing-library/react";
import MySetsPage from "./MySetsPage";

// Test MySetsPage in isolation: replace the real SetsPageLayout (with
// hooks, MSW requests, etc.) with a stub that just records the props
// it received in a data-attribute (from the real SetsPageLayout component).
// We only need to check that MySetsPage passes the correct endpoint/isMine
// — not how SetsPageLayout itself works (that's already covered in SetsPageLayout.test.jsx).
vi.mock("../../components/Layouts/SetsPageLayout", () => ({
  default: (props) => (
    <div
      data-testid="layout"
      // extraCard is a JSX element (React.createElement output), and
      // JSON.stringify can't serialize it safely — it may throw or
      // silently drop it. We don't need extraCard for this test
      // (only endpoint/isMine matter here), so we strip it before
      // stringifying the rest of the props.
      data-props={JSON.stringify({ ...props, extraCard: undefined })}
    />
  ),
}));

it("renders SetsPageLayout with endpoint='/my-sets' and isMine=true", () => {
  render(<MySetsPage />);
  const layout = screen.getByTestId("layout");
  const props = JSON.parse(layout.dataset.props);

  expect(props.endpoint).toBe("/my-sets");
  expect(props.isMine).toBe(true);
});
