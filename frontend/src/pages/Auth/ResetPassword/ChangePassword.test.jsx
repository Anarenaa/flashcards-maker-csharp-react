import { render, fireEvent, waitFor } from "@testing-library/react";
import { BrowserRouter } from "react-router";
import { http, HttpResponse } from "msw";
import { server } from "../../../setupTests";
import ChangePassword from "./ChangePassword";

describe("Form and local validation", () => {
  let container;
  let newPasswordInput;
  let confirmPasswordInput;

  const setUrlParams = (params = "?email=mika.akima.22@gmail.com&token=valid-token") => {
    window.history.pushState({}, "Test page", `/change-password${params}`);
  };

  it("should redirect (render empty form) if email or token is missing in URL", () => {
    setUrlParams("");
    const rendered = render(
      <BrowserRouter>
        <ChangePassword />
      </BrowserRouter>
    );
    expect(rendered.container.querySelector("form")).not.toBeInTheDocument();
  });

  describe("With valid parameters", () => {
    beforeEach(() => {
      setUrlParams();
      const rendered = render(
        <BrowserRouter>
          <ChangePassword />
        </BrowserRouter>
      );
      container = rendered.container;
      newPasswordInput = container.querySelector('input[name="newPassword"]');
      confirmPasswordInput = container.querySelector('input[name="confirmPassword"]');
    });

    it("should load with correct readonly email and empty password fields initially", () => {
      const emailInput = container.querySelector("input[readonly]");
      const visibleErrors = Array.from(container.querySelectorAll(".text-danger")).filter(el => el.textContent !== "");

      expect(emailInput).not.toBeNull();
      expect(emailInput.value).toBe("mika.akima.22@gmail.com");
      expect(newPasswordInput.value).toBe("");
      expect(confirmPasswordInput.value).toBe("");
      expect(visibleErrors).toHaveLength(0);
    });

    it("should display error message when passwords do not match", async () => {
      fireEvent.change(newPasswordInput, { target: { value: "Password123!" } });
      fireEvent.change(confirmPasswordInput, { target: { value: "Different123!" } });
      fireEvent.click(container.querySelector('button[type="submit"]'));

      await waitFor(() => {
        const errorMessages = Array.from(container.querySelectorAll(".text-danger")).filter(el => el.textContent !== "");
        expect(errorMessages).toHaveLength(1);
        expect(confirmPasswordInput).toHaveClass("error-input");
        expect(errorMessages[0].textContent).toContain("не збігаються");
      });
    });

    it("should display error message when password consists only of spaces", async () => {
      fireEvent.change(newPasswordInput, { target: { value: "      " } });
      fireEvent.change(confirmPasswordInput, { target: { value: "      " } });
      fireEvent.click(container.querySelector('button[type="submit"]'));

      await waitFor(() => {
        const errorMessages = Array.from(container.querySelectorAll(".text-danger")).filter(el => el.textContent !== "");
        expect(errorMessages).not.toHaveLength(0);
        expect(errorMessages[0].textContent).toContain("не може складатися лише з пробілів");
      });
    });
  });
});

describe("API interaction (MSW)", () => {
  let container;
  let newPasswordInput;
  let confirmPasswordInput;
  let submitButton;

  beforeEach(() => {
    window.history.pushState({}, "Test page", "/change-password?email=mika.akima.22@gmail.com&token=valid-token");
    const rendered = render(
      <BrowserRouter>
        <ChangePassword />
      </BrowserRouter>
    );
    container = rendered.container;
    newPasswordInput = container.querySelector('input[name="newPassword"]');
    confirmPasswordInput = container.querySelector('input[name="confirmPassword"]');
    submitButton = container.querySelector('button[type="submit"]');
  });

  it("should display custom backend error when token is invalid", async () => {
    server.use(
      http.post("*/api/auth/change-password", () => {
        return HttpResponse.json({ global: ["Invalid token."] }, { status: 400 });
      }),
    );

    fireEvent.change(newPasswordInput, { target: { value: "Password123!" } });
    fireEvent.change(confirmPasswordInput, { target: { value: "Password123!" } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(container.textContent).toContain("Invalid token.");
    });
  });

  it("should display custom backend error when password was used before", async () => {
    server.use(
      http.post("*/api/auth/change-password", () => {
        return HttpResponse.json(
          { newPassword: ["Новий пароль не може бути таким самим, як старий"] },
          { status: 400 },
        );
      }),
    );

    fireEvent.change(newPasswordInput, { target: { value: "OldPassword123!" } });
    fireEvent.change(confirmPasswordInput, { target: { value: "OldPassword123!" } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      const passwordError = Array.from(container.querySelectorAll(".text-danger")).find(el => el.textContent !== "");
      expect(passwordError.textContent).toContain("таким самим, як старий");
    });
  });

  describe("edge tests", () => {
    it("should catch network errors and display internet connection message", async () => {
      server.use(http.post("*/api/auth/change-password", () => HttpResponse.error()));

      fireEvent.change(newPasswordInput, { target: { value: "Password123!" } });
      fireEvent.change(confirmPasswordInput, { target: { value: "Password123!" } });
      fireEvent.click(submitButton);

      await waitFor(() => {
        expect(container.textContent).toContain("Здається, у вас зник інтернет");
      });
    });
  });
});