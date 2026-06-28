import { render, fireEvent, waitFor } from "@testing-library/react";
import { BrowserRouter } from "react-router";
import { http, HttpResponse } from "msw";
import { server } from "../../setupTests";
import LoginPage from "./LoginPage";

describe("Form and local validation", () => {
  let container;
  let usernameInput;
  let passwordInput;

  beforeEach(() => {
    const rendered = render(
      <BrowserRouter>
        <LoginPage />
      </BrowserRouter>,
    );
    container = rendered.container;

    usernameInput = container.querySelector('input[name="userNameOrEmail"]');
    passwordInput = container.querySelector('input[type="password"]');
  });

  it("should load with empty inputs and no error messages initially", () => {
    const errorInputs = container.querySelectorAll(".text-danger");

    expect(usernameInput).not.toBeNull();
    expect(passwordInput).not.toBeNull();
    expect(usernameInput.value).toBe("");
    expect(passwordInput.value).toBe("");
    expect(errorInputs).toHaveLength(0);
  });

  it("should allow the user to fill out the form fields successfully", () => {
    fireEvent.change(usernameInput, { target: { value: "ananas" } });
    fireEvent.change(passwordInput, {
      target: { value: "super-password-123" },
    });

    expect(usernameInput.value).toBe("ananas");
    expect(passwordInput.value).toBe("super-password-123");
  });

  it("should highlight fields and display validation errors when submitting an empty form", () => {
    const submitButton = container.querySelector('button[type="submit"]');

    fireEvent.change(usernameInput, { target: { value: " " } });
    fireEvent.change(passwordInput, { target: { value: " " } });

    fireEvent.click(submitButton);

    const errorMessages = container.querySelectorAll(".text-danger");
    expect(errorMessages).toHaveLength(2);

    expect(usernameInput).toHaveClass("error-input");
    expect(passwordInput).toHaveClass("error-input");

    expect(errorMessages[0].textContent).toContain("ім'я");
    expect(errorMessages[1].textContent).toContain("пароль");
  });
});

describe("API interaction (MSW)", () => {
  let container;
  let usernameInput;
  let passwordInput;
  let submitButton;

  beforeEach(() => {
    const rendered = render(
      <BrowserRouter>
        <LoginPage />
      </BrowserRouter>,
    );
    container = rendered.container;
    usernameInput = container.querySelector('input[name="userNameOrEmail"]');
    passwordInput = container.querySelector('input[type="password"]');
    submitButton = container.querySelector('button[type="submit"]');
  });

  it("should successfully log in and handle 200 OK response with JWT", async () => {
    server.use(
      http.post("*/api/auth/login", () => {
        return HttpResponse.json({
          message: "Вхід успішний",
          user: "mika.akima.22@gmail.com",
          userToken: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
          redirectTo: "/",
        });
      }),
    );

    fireEvent.change(usernameInput, {
      target: { value: "mika.akima.22@gmail.com" },
    });
    fireEvent.change(passwordInput, { target: { value: "correct-password" } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      const errorMessages = container.querySelectorAll(".text-danger");
      expect(errorMessages).toHaveLength(0);
      expect(window.location.pathname).toBe("/");
    });
  });

  it("should display error message when username or email is not found", async () => {
    server.use(
      http.post("*/api/auth/login", () => {
        return HttpResponse.json(
          {
            UserNameOrEmail: [
              "Користувача з таким логіном або email не знайдено",
            ],
          },
          { status: 400 },
        );
      }),
    );

    fireEvent.change(usernameInput, { target: { value: "unknown@gmail.com" } });
    fireEvent.change(passwordInput, { target: { value: "some-password" } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      const errorText = container.querySelector(".text-danger");
      expect(errorText.textContent).toContain("не знайдено");
    });
  });

  it("should display custom backend error when password is incorrect", async () => {
    server.use(
      http.post("*/api/auth/login", () => {
        return HttpResponse.json(
          {
            Password: ["Неправильний пароль"],
          },
          { status: 400 },
        );
      }),
    );

    fireEvent.change(usernameInput, {
      target: { value: "mika.akima.22@gmail.com" },
    });
    fireEvent.change(passwordInput, {
      target: { value: "wrong-password-123" },
    });
    fireEvent.click(submitButton);

    await waitFor(() => {
      const passwordError = container.querySelector(".text-danger");
      expect(passwordError.textContent).toContain("Неправильний пароль");
    });
  });
});
