import { render, screen } from "@testing-library/react";
import MainPage from "./MainPage";

// Test MainPage in isolation: replace the real SetsPageLayout (with
// hooks, MSW requests, etc.) with a stub that just records the props
// it received in a data-attribute (from the real SetsPageLayout component).
// We only need to check that MainPage passes the correct endpoint/isMine
// — not how SetsPageLayout itself works (that's already covered in SetsPageLayout.test.jsx).
vi.mock("../../components/Layouts/SetsPageLayout", () => ({
  default: (props) => (
    <div data-testid="layout" data-props={JSON.stringify(props)} />
  ),
}));

it("renders SetsPageLayout with endpoint='/sets' and isMine=false", () => {
  render(<MainPage />);
  const layout = screen.getByTestId("layout");
  const props = JSON.parse(layout.dataset.props);

  expect(props.endpoint).toBe("/sets");
  expect(props.isMine).toBe(false);
});
